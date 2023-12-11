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
    [ApiController]
    [Produces("application/json")]
    [Route("[controller]/[action]")]
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
        public ContentResult GetUserData()
        {
            string discordUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var tracks = _mainContext.Track.Where((x) => x.DiscordUserId == discordUserId);
            var categories = _mainContext.Category.Where((x) => x.DiscordUserId == discordUserId);

            return new APIResult(HttpStatusCode.OK, new { tracks, categories }).ToContentResult();
        }

        [HttpPost]
        [EnableCors("allowPOST")]
        public async Task<ContentResult> AddTrack([FromBody] AddTrack track)
        {
            if (track.StartAt > track.EndAt)
                return new APIResult(HttpStatusCode.BadRequest, "開始時間不可大於結束時間").ToContentResult();

            string discordUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var resultCode = HttpStatusCode.Created;
            var userTrack = await _mainContext.Track.SingleOrDefaultAsync((x) => x.DiscordUserId == discordUserId && x.VideoId == track.VideoId);
            if (userTrack == null)
            {
                _mainContext.Track.Add(new Track() { DiscordUserId = discordUserId, VideoId = track.VideoId, StartAt = track.StartAt, EndAt = track.EndAt });
            }
            else
            {
                userTrack.StartAt = track.StartAt;
                userTrack.EndAt = track.EndAt;
                _mainContext.Track.Update(userTrack);
                resultCode = HttpStatusCode.NoContent;
            }

            await _mainContext.SaveChangesAsync();

            return new APIResult(resultCode).ToContentResult();
        }

        [HttpPost]
        [EnableCors("allowPOST")]
        public async Task<ContentResult> AddCategory([FromBody] AddCategory category)
        {
            string discordUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(category.Name))
                return new APIResult(HttpStatusCode.BadRequest, "Name 不可空白").ToContentResult();

            if (category.Position <= 0)
                return new APIResult(HttpStatusCode.BadRequest, "Position 不可小於 1").ToContentResult();

            if (_mainContext.Category.AsNoTracking().Any((x) => x.Position == category.Position))
                return new APIResult(HttpStatusCode.BadRequest, "Position 不可跟其他分類重複").ToContentResult();

            Category result;
            var userCategory = await _mainContext.Category.SingleOrDefaultAsync((x) => x.DiscordUserId == discordUserId && x.Name == category.Name);
            if (userCategory == null)
            {
                result = new Category() { DiscordUserId = discordUserId, Name = category.Name, Position = category.Position };
                _mainContext.Category.Add(result);
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
        public async Task<ContentResult> SetCategoryTrack([FromBody] SetCategoryTrack setCategoryTrack)
        {
            string discordUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (setCategoryTrack.VideoAndPosition == null)
                return new APIResult(HttpStatusCode.BadRequest).ToContentResult();

            // https://stackoverflow.com/a/18547390
            if (setCategoryTrack.VideoAndPosition.Values.GroupBy((x) => x).Any((x) => x.Count() > 1))
                return new APIResult(HttpStatusCode.BadRequest, "Position 不可重複").ToContentResult();

            var userCategory = await _mainContext.Category.SingleOrDefaultAsync((x) => x.DiscordUserId == discordUserId && x.Guid == setCategoryTrack.Guid);
            if (userCategory == null)
                return new APIResult(HttpStatusCode.BadRequest, "查無分類").ToContentResult();

            userCategory.VideoIdList = setCategoryTrack.VideoAndPosition;
            _mainContext.Category.Update(userCategory);
            await _mainContext.SaveChangesAsync();

            return new APIResult(HttpStatusCode.OK, userCategory).ToContentResult();
        }


        [HttpPost]
        [EnableCors("allowPOST")]
        public async Task<ContentResult> SetCategoriesPosition([FromBody] SetCategoriesPosition setCategoriesPosition)
        {
            string discordUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (setCategoriesPosition.GuidAndPosition == null)
                return new APIResult(HttpStatusCode.BadRequest).ToContentResult();

            // https://stackoverflow.com/a/18547390
            if (setCategoriesPosition.GuidAndPosition.Values.GroupBy((x) => x).Any((x) => x.Count() > 1))
                return new APIResult(HttpStatusCode.BadRequest, "Position 不可重複").ToContentResult();

            var userCategories = _mainContext.Category.Where((x) => x.DiscordUserId == discordUserId);
            if (!userCategories.Any())
                return new APIResult(HttpStatusCode.BadRequest, "無任何分類").ToContentResult();

            int result = 0;
            foreach (var item in setCategoriesPosition.GuidAndPosition)
            {
                var userCategory = userCategories.SingleOrDefault((x) => x.Guid == item.Key);
                if (userCategory == null)
                    continue;

                userCategory.Position = item.Value;
                _mainContext.Category.Update(userCategory);
                result++;
            }

            await _mainContext.SaveChangesAsync();
            return new APIResult(HttpStatusCode.OK, result).ToContentResult();
        }
    }
}
