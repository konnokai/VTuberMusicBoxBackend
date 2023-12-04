using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.Net;

#nullable disable

namespace VTuberMusicBoxBackend
{
    public static class Utility
    {
        public static ConnectionMultiplexer Redis { get; set; }
        public static IDatabase RedisDb { get; set; }

        /// <summary>
        /// Get remote ip address, optionally allowing for x-forwarded-for header check
        /// </summary>
        /// <param name="context">Http context</param>
        /// <param name="allowForwarded">Whether to allow x-forwarded-for header check</param>
        /// <returns>IPAddress</returns>
        public static IPAddress GetRemoteIPAddress(this HttpContext context, bool allowForwarded = true)
        {
            if (allowForwarded)
            {
                // if you are allowing these forward headers, please ensure you are restricting context.Connection.RemoteIpAddress
                // to cloud flare ips: https://www.cloudflare.com/ips/
                string header = context.Request.Headers["CF-Connecting-IP"].FirstOrDefault() ?? context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
                if (header == null)
                    return context.Connection.RemoteIpAddress;

                header = header.Split(',')[0];
                if (IPAddress.TryParse(header, out IPAddress ip))
                {
                    return ip;
                }
            }

            return context.Connection.RemoteIpAddress;
        }
    }

    /// <summary>
    /// API回傳物件
    /// </summary>
    public class APIResult
    {
        /// <summary>
        /// 建構子
        /// </summary>
        /// <param name="code">回傳狀態碼</param>
        /// <param name="message">Object訊息</param>
        public APIResult(HttpStatusCode code, object message = null)
        {
            Code = (int)code;
            Message = message;
        }

        public ContentResult ToContentResult()
        {
            return new ContentResult() { StatusCode = Code, Content = JsonConvert.SerializeObject(this) };
        }

        public string ToJson()
            => JsonConvert.SerializeObject(this);

        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("message")]
        public object Message { get; set; }
    }
}
