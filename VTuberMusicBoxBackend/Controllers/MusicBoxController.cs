using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        public MusicBoxController(MainDbContext mainContext)
        {
            _mainContext = mainContext;
        }

        [HttpGet]
        [EnableCors("allowGET")]
        public async Task<ContentResult> GetUserData()
        {
            string discordUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var userData = await _mainContext.User
                .Include((x) => x.TrackList)
                .Include((x) => x.CategorieList)
                .AsNoTracking()
                .AsSplitQuery()
                .SingleOrDefaultAsync((x) => x.DiscordId == discordUserId);

            return new APIResult(HttpStatusCode.OK,
                new
                {
                    track_list = userData?.TrackList,
                    categorie_list = userData?.CategorieList,
                    favorite_track_list = userData?.FavoriteTrackList
                }).ToContentResult();
        }

        [HttpPost]
        [EnableCors("allowPOST")]
        public async Task<ContentResult> AddTrack([FromBody] AddTrack track)
        {
            if (track.StartAt >= track.EndAt)
                return new APIResult(HttpStatusCode.BadRequest, "開始時間不可大於等於結束時間").ToContentResult();

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
                resultCode = HttpStatusCode.OK;
            }

            await _mainContext.SaveChangesAsync();

            return new APIResult(resultCode).ToContentResult();
        }
    }
}
