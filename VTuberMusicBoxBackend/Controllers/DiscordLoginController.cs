using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using VTuberMusicBoxBackend.Configs;
using VTuberMusicBoxBackend.Models;
using VTuberMusicBoxBackend.Models.DiscordOAuth;

#nullable disable

namespace VTuberMusicBoxBackend.Controllers
{
    [AllowAnonymous]
    [Route("[controller]/[action]")]
    [ApiController]
    public class DiscordController : Controller
    {
        private readonly ILogger<DiscordController> _logger;
        private readonly HttpClient _httpClient;
        private readonly MainDbContext _mainContext;
        private readonly DiscordConfig _discordConfig;
        private readonly JwtConfig _jwtConfig;

        public DiscordController(ILogger<DiscordController> logger,
            HttpClient httpClient,
            MainDbContext mainContext,
            IOptions<DiscordConfig> discordConfig,
            IOptions<JwtConfig> jwtConfig)
        {
            _logger = logger;
            _httpClient = httpClient;
            _mainContext = mainContext;
            _discordConfig = discordConfig.Value;
            _jwtConfig = jwtConfig.Value;
        }

        [EnableCors("allowGET")]
        [HttpGet]
        public async Task<ContentResult> GetToken(string code)
        {
            if (string.IsNullOrEmpty(code))
                return new APIResult(ResultStatusCode.BadRequest, "參數錯誤").ToContentResult();

            try
            {
                if (await Utility.RedisDb.KeyExistsAsync($"discord:code:{code}"))
                    return new APIResult(ResultStatusCode.BadRequest, "請確認是否有插件或軟體導致重複驗證\n如網頁正常顯示資料則無需理會").ToContentResult();

                await Utility.RedisDb.StringSetAsync($"discord:code:{code}", "0", TimeSpan.FromHours(3));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DiscordGetToken - Redis 設定錯誤\r\n");
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
                        new("client_id", _discordConfig.ClientId),
                        new("client_secret", _discordConfig.ClientSecret),
                        new("redirect_uri", _discordConfig.RedirectURI),
                        new("grant_type", "authorization_code")
                    });
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
                    var response = await _httpClient.PostAsync("https://discord.com/api/v10/oauth2/token", content);

                    response.EnsureSuccessStatusCode();

                    tokenData = JsonConvert.DeserializeObject<TokenData>(await response.Content.ReadAsStringAsync());

                    if (tokenData == null || tokenData.AccessToken == null)
                        return new APIResult(ResultStatusCode.Unauthorized, "認證錯誤，請重新登入 Discord").ToContentResult();
                }
                catch (HttpRequestException httpEx) when (httpEx.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    _logger.LogWarning("Discord Token 交換失敗: {Code}", code);
                    return new APIResult(ResultStatusCode.BadRequest, "請重新登入 Discord").ToContentResult();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "DiscordGetToken - Discord Token 交換錯誤\r\n");
                    return new APIResult(ResultStatusCode.InternalServerError, "伺服器內部錯誤，請向孤之界回報").ToContentResult();
                }

                UserData discordUser = null;
                try
                {
                    _httpClient.DefaultRequestHeaders.Clear();
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue($"Bearer", tokenData.AccessToken);

                    var discordMeJson = await _httpClient.GetStringAsync("https://discord.com/api/v10/users/@me");
                    discordUser = JsonConvert.DeserializeObject<UserData>(discordMeJson);

                    _logger.LogInformation("Discord User OAuth Done: {DiscordUsername} ({DiscordUserid})", discordUser.Username, discordUser.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "DiscordGetToken - Discord API 回傳錯誤\r\n");
                    return new APIResult(ResultStatusCode.InternalServerError, "伺服器內部錯誤，請向孤之界回報").ToContentResult();
                }

                if (!await _mainContext.User.AsNoTracking().AnyAsync((x) => x.DiscordId == discordUser.Id))
                {
                    _mainContext.User.Add(new User() { DiscordId = discordUser.Id });
                    await _mainContext.SaveChangesAsync();
                }

                string token = "";
                try
                {
                    token = GenerateJwtToken(discordUser.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "DiscordGetToken - 建立 JWT 錯誤\r\n");
                    return new APIResult(ResultStatusCode.InternalServerError, "伺服器內部錯誤，請向孤之界回報").ToContentResult();
                }

                return new APIResult(ResultStatusCode.OK, new { token, discord_data = discordUser }).ToContentResult();
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "DiscordGetToken - 整體錯誤\r\n");
                return new APIResult(ResultStatusCode.InternalServerError, "伺服器內部錯誤，請向孤之界回報").ToContentResult();
            }
        }

        // https://medium.com/selectprogram/asp-net-core%E4%BD%BF%E7%94%A8jwt%E9%A9%97%E8%AD%89-1b0609e6e8e3
        /// <summary>
        /// 產生 JWT Token
        /// </summary>
        /// <param name="discordUserId">Discord User Id</param>
        /// <returns>JWT Token</returns>
        private string GenerateJwtToken(string discordUserId)
        {
            //appsettings中JwtConfig的Secret值
            byte[] key = Encoding.ASCII.GetBytes(_jwtConfig.Secret);

            //定義token描述
            SecurityTokenDescriptor tokenDescriptor = new()
            {
                //設定要加入到 JWT Token 中的聲明資訊(Claims)
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, discordUserId),
                }),

                //設定Token的時效
                Expires = DateTime.UtcNow.AddDays(3),

                //設定加密方式，key(appsettings中JwtConfig的Secret值)與HMAC SHA512演算法
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
            };

            //宣告JwtSecurityTokenHandler，用來建立token
            JwtSecurityTokenHandler jwtTokenHandler = new();

            //使用SecurityTokenDescriptor建立JWT securityToken
            SecurityToken token = jwtTokenHandler.CreateToken(tokenDescriptor);

            //token序列化為字串
            return jwtTokenHandler.WriteToken(token);
        }
    }
}
