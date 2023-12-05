using Microsoft.AspNetCore.Http.Extensions;
using NLog;
using StackExchange.Redis;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace VTuberMusicBoxBackend.Middleware
{
    public class LogMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly Logger _logger = LogManager.GetLogger("ACCE");

        public LogMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var originalResponseBodyStream = context.Response.Body;

            try
            {
                var remoteIpAddress = context.GetRemoteIPAddress();
                var requestUrl = context.Request.GetDisplayUrl();
                string badReqRedisKey = $"server.errorcount:{remoteIpAddress.ToString().Replace(":", "-").Replace(".", "-")}";
                bool isRedisError = false;

                try
                {
                    var badCount = await Utility.RedisDb.StringGetAsync(badReqRedisKey);
                    if (badCount.HasValue && int.Parse(badCount.ToString()) >= 5)
                    {
                        await Utility.RedisDb.StringIncrementAsync(badReqRedisKey);
                        await Utility.RedisDb.KeyExpireAsync(badReqRedisKey, TimeSpan.FromHours(1));

                        var result = new APIResult(HttpStatusCode.TooManyRequests, "Too Many Requests");
                        var messageBytes = Encoding.UTF8.GetBytes(result.ToJson());

                        context.Response.StatusCode = result.Code;
                        await context.Response.Body.WriteAsync(messageBytes);
                        return;
                    }
                }
                catch (RedisConnectionException redisEx)
                {
                    _logger.Error(redisEx, "Redis 掛掉了\r\n");
                    isRedisError = true;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Middleware 請求錯誤\r\n");
                }

                await _next(context);

                // Generate from ChatGPT
                var route = context.GetRouteValue("action")?.ToString()?.ToLower();
                if (route != null && route == "statuscheck" && context.Response.StatusCode == 200)
                    return;

                _logger.Info($"{remoteIpAddress} | {context.Request.Method} | {context.Response.StatusCode} | {requestUrl}");

                if (!isRedisError && !Debugger.IsAttached)
                {
                    if (context.Response.StatusCode >= 400 && context.Response.StatusCode < 500)
                    {
                        await Utility.RedisDb.StringIncrementAsync(badReqRedisKey);
                        await Utility.RedisDb.KeyExpireAsync(badReqRedisKey, TimeSpan.FromHours(1));
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "LogMiddleware Error\r\n");

                var result = new APIResult(HttpStatusCode.InternalServerError, "伺服器內部錯誤");
                var messageBytes = Encoding.UTF8.GetBytes(result.ToJson());

                context.Response.StatusCode = result.Code;
                await context.Response.Body.WriteAsync(messageBytes);
            }
        }
    }
}