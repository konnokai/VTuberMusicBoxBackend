using Microsoft.AspNetCore.Http.Extensions;
using Newtonsoft.Json;
using NLog;
using StackExchange.Redis;
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
                        var errorMessage = JsonConvert.SerializeObject(new
                        {
                            ErrorMessage = "429 Too Many Requests"
                        });
                        var bytes = Encoding.UTF8.GetBytes(errorMessage);

                        context.Response.StatusCode = 429;
                        await originalResponseBodyStream.WriteAsync(bytes);
                        return;
                    }
                }
                catch (RedisConnectionException redisEx)
                {
                    _logger.Error(redisEx, "Redis掛掉了");
                    isRedisError = true;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Middleware錯誤");
                }

                await _next(context);

                if (requestUrl.EndsWith("statuscheck") && context.Response.StatusCode == 200)
                    return;

                _logger.Info($"{remoteIpAddress} | {context.Request.Method} | {context.Response.StatusCode} | {requestUrl}");

                if (!isRedisError)
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
                _logger.Error(e);

                var errorMessage = JsonConvert.SerializeObject(new
                {
                    ErrorMessage = e.Message
                });
                var bytes = Encoding.UTF8.GetBytes(errorMessage);

                await originalResponseBodyStream.WriteAsync(bytes);
            }
        }
    }
}