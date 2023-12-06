using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Net;
using System.Security.Claims;
using VTuberMusicBoxBackend.Models.Database;
using VTuberMusicBoxBackend.Models.Requests;

namespace VTuberMusicBoxBackend.Controllers
{
    [Authorize]
    [Route("[controller]/[action]")]
    [ApiController]
    public class MusicBoxController : Controller
    {
        private readonly MainDbContext _mainContext;
        private readonly ILogger<MusicBoxController> _logger;

        public MusicBoxController(MainDbContext mainContext, ILogger<MusicBoxController> logger)
        {
            _mainContext = mainContext;
            _logger = logger;
        }

        [HttpGet]
        [EnableCors("allowGET")]
        public async Task<ContentResult> GetUserData()
        {
            string discordUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // https://learn.microsoft.com/zh-tw/ef/core/querying/single-split-queries
            var userData = await _mainContext.User
                .Include((x) => x.TrackList)
                .Include((x) => x.CategorieList)
                .AsNoTracking()
                .AsSplitQuery() // 不確定這兩行會不會提升 SQL 執行速度 :thinking:
                .Select((x) => new { x.DiscordId, x.TrackList, x.CategorieList })
                .SingleOrDefaultAsync((x) => x.DiscordId == discordUserId);

            // 原則上不會發生但還是寫一下保險
            if (userData == null)
                return new APIResult(HttpStatusCode.BadRequest, "無此使用者資料，請重新登入").ToContentResult();

            return new APIResult(HttpStatusCode.OK,
                new
                {
                    track_list = userData?.TrackList,
                    categorie_list = userData?.CategorieList
                }).ToContentResult();
        }

        [HttpPost]
        [EnableCors("allowPOST")]
        public async Task<ContentResult> AddTrack([FromBody] AddTrack track)
        {
            if (track.StartAt > track.EndAt)
                return new APIResult(HttpStatusCode.BadRequest, "開始時間不可大於結束時間").ToContentResult();

            string discordUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var userData = await _mainContext.User
                .Include((x) => x.TrackList)
                .SingleOrDefaultAsync((x) => x.DiscordId == discordUserId);

            if (userData == null)
                return new APIResult(HttpStatusCode.BadRequest, "無此使用者資料，請重新登入").ToContentResult();

            var resultCode = HttpStatusCode.Created;
            var userTrack = userData.TrackList.SingleOrDefault((x) => x.VideoId == track.VideoId);
            if (userTrack == null)
            {
                userData.TrackList.Add(new Track() { VideoId = track.VideoId, StartAt = track.StartAt, EndAt = track.EndAt });
            }
            else
            {
                userTrack.StartAt = track.StartAt;
                userTrack.EndAt = track.EndAt;
                _mainContext.User.Update(userData);
                resultCode = HttpStatusCode.NoContent;
            }

            await _mainContext.SaveChangesAsync();

            return new APIResult(resultCode).ToContentResult();
        }

        [HttpPost]
        [EnableCors("allowPOST")]
        public async Task<ContentResult> AddCategorie([FromBody] AddCategorie categorie)
        {
            string discordUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var userData = await _mainContext.User
                .Include((x) => x.CategorieList)
                .SingleOrDefaultAsync((x) => x.DiscordId == discordUserId);

            if (userData == null)
                return new APIResult(HttpStatusCode.BadRequest, "無此使用者資料，請重新登入").ToContentResult();

            Category result;
            var userCategory = userData.CategorieList.SingleOrDefault((x) => x.Name == categorie.Name);
            if (userCategory == null)
            {
                result = new Category() { Name = categorie.Name, Position = categorie.Position };
                userData.CategorieList.Add(result);
                await _mainContext.SaveChangesAsync();
            }
            else
            {
                return new APIResult(HttpStatusCode.BadRequest, "已存在同名分類").ToContentResult();
            }

            return new APIResult(HttpStatusCode.Created, result).ToContentResult();
        }


        [HttpPost]
        [EnableCors("allowPOST")]
        public async Task<ContentResult> SetTrackCategorie([FromBody] SetTrackCategorie setTrackCategorie)
        {
            if (setTrackCategorie.Position <= 0)
                return new APIResult(HttpStatusCode.BadRequest, "Position 不可小於等於0").ToContentResult();

            string discordUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // ChatGPT: 可以考慮使用 AsNoTracking 來取消追蹤，然後再重新 Update 實體。這樣可以確保 EF Core 不會追蹤這些實體，並且在更新時直接將它們視為修改的實體。
            var userData = await _mainContext.User
                .AsNoTracking()
                .Include((x) => x.CategorieList)
                .SingleOrDefaultAsync((x) => x.DiscordId == discordUserId);

            if (userData == null)
                return new APIResult(HttpStatusCode.BadRequest, "無此使用者資料，請重新登入").ToContentResult();

            var userCategory = userData.CategorieList.SingleOrDefault((x) => x.Guid == setTrackCategorie.CategorieGuId);
            if (userCategory == null)
                return new APIResult(HttpStatusCode.BadRequest, "查無分類").ToContentResult();

            if (userCategory.VideoIdList.ContainsKey(setTrackCategorie.VideoId))
            {
                userCategory.VideoIdList[setTrackCategorie.VideoId] = setTrackCategorie.Position;
            }
            else
            {
                userCategory.VideoIdList.Add(setTrackCategorie.VideoId, setTrackCategorie.Position);
            }

            _mainContext.User.Update(userData);

            await _mainContext.SaveChangesAsync();

            return new APIResult(HttpStatusCode.OK, userCategory).ToContentResult();
        }
    }
}
