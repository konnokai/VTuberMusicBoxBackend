using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;

#nullable disable

namespace VTuberMusicBoxBackend.Controllers
{
    [Route("[action]")]
    [ApiController]
    public class DiscordLoginController : Controller
    {
        private readonly ILogger<DiscordLoginController> _logger;
        private readonly HttpClient _httpClient;

        public DiscordLoginController(ILogger<DiscordLoginController> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        [EnableCors("allowGET")]
        [HttpGet]
        public async Task<ContentResult> DiscordCallBack(string code)
        {
            if (string.IsNullOrEmpty(code))
                return new APIResult(ResultStatusCode.BadRequest, "參數錯誤").ToContentResult();

            try
            {
                if (await Utility.RedisDb.KeyExistsAsync($"discord:code:{code}"))
                    return new APIResult(ResultStatusCode.BadRequest, "請確認是否有插件或軟體導致重複驗證\n如網頁正常顯示資料則無需理會").ToContentResult();

                await Utility.RedisDb.StringSetAsync($"discord:code:{code}", "0", TimeSpan.FromHours(1));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DiscordCallBack - Redis設定錯誤");
                return new APIResult(ResultStatusCode.InternalServerError, "伺服器內部錯誤，請向孤之界回報").ToContentResult();
            }

            try
            {
                TokenData tokenData = null;
                try
                {
                    var content = new FormUrlEncodedContent(new KeyValuePair<string, string>[]
                    {
                        new("code", code),
                        new("client_id", Utility.ServerConfig.DiscordClientId),
                        new("client_secret", Utility.ServerConfig.DiscordClientSecret),
                        new("redirect_uri", Utility.ServerConfig.RedirectURI),
                        new("grant_type", "authorization_code")
                    });
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
                    var response = await _httpClient.PostAsync("https://discord.com/api/v10/oauth2/token", content);

                    response.EnsureSuccessStatusCode();

                    tokenData = JsonConvert.DeserializeObject<TokenData>(await response.Content.ReadAsStringAsync());

                    if (tokenData == null || tokenData.AccessToken == null)
                        return new APIResult(ResultStatusCode.Unauthorized, "認證錯誤，請重新登入Discord").ToContentResult();
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("400"))
                    {
                        _logger.LogWarning("{ExceptionMessage}", ex.ToString());
                        return new APIResult(ResultStatusCode.BadRequest, "請重新登入Discord").ToContentResult();
                    }

                    _logger.LogError(ex, "DiscordCallBack - Discord Token交換錯誤");
                    return new APIResult(ResultStatusCode.InternalServerError, "伺服器內部錯誤，請向孤之界回報").ToContentResult();
                }

                DiscordUser discordUser = null;
                try
                {
                    _httpClient.DefaultRequestHeaders.Clear();
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue($"Bearer", tokenData.AccessToken);

                    var discordMeJson = await _httpClient.GetStringAsync("https://discord.com/api/v10/users/@me");
                    discordUser = JsonConvert.DeserializeObject<DiscordUser>(discordMeJson);

                    _logger.LogInformation("Discord User OAuth Done: {DiscordUsername} ({DiscordUserid})", discordUser.Username, discordUser.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "DiscordCallBack - Discord API回傳錯誤");
                    return new APIResult(ResultStatusCode.InternalServerError, "伺服器內部錯誤，請向孤之界回報").ToContentResult();
                }

                string token = "";
                try
                {
                    token = await Auth.TokenManager.CreateTokenAsync(discordUser.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "DiscordCallBack - 建立JWT錯誤");
                    return new APIResult(ResultStatusCode.InternalServerError, "伺服器內部錯誤，請向孤之界回報").ToContentResult();
                }

                return new APIResult(ResultStatusCode.OK, new { token, discord_data = discordUser }).ToContentResult();
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "DiscordCallBack - 整體錯誤");
                return new APIResult(ResultStatusCode.InternalServerError, "伺服器內部錯誤，請向孤之界回報").ToContentResult();
            }
        }
    }
}
