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

            if (track.VideoId.Length != 11)
                return new APIResult(HttpStatusCode.BadRequest, "Video Id 長度錯誤").ToContentResult();

            if (string.IsNullOrEmpty(track.VideoTitle))
                return new APIResult(HttpStatusCode.BadRequest, "Video Title 不可空白").ToContentResult();

            string discordUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (_mainContext.Track.AsNoTracking().Any((x) => x.DiscordUserId == discordUserId && x.VideoId == track.VideoId && x.StartAt == track.StartAt && x.EndAt == track.EndAt))
            {
                return new APIResult(HttpStatusCode.BadRequest, "不可重複加入相同的歌曲").ToContentResult();
            }
            else
            {
                var userTrack = new Track()
                {
                    DiscordUserId = discordUserId,
                    VideoId = track.VideoId,
                    StartAt = track.StartAt,
                    EndAt = track.EndAt,
                    VideoTitle = track.VideoTitle,
                    Artist = track.Artist,
                    TrackTitle = track.TrackTitle
                };

                _mainContext.Track.Add(userTrack);
                await _mainContext.SaveChangesAsync();

                return new APIResult(HttpStatusCode.OK, new { guid = userTrack.Guid }).ToContentResult();
            }
        }

        [HttpPost]
        [EnableCors("allowPOST")]
        public async Task<ContentResult> SetTrack([FromBody] SetTrack track)
        {
            if (string.IsNullOrEmpty(track.Guid))
                return new APIResult(HttpStatusCode.BadRequest, "Guid 不可空白").ToContentResult();

            if (track.StartAt > track.EndAt)
                return new APIResult(HttpStatusCode.BadRequest, "開始時間不可大於結束時間").ToContentResult();

            if (track.VideoId.Length != 11)
                return new APIResult(HttpStatusCode.BadRequest, "Video Id 長度錯誤").ToContentResult();

            if (string.IsNullOrEmpty(track.VideoTitle))
                return new APIResult(HttpStatusCode.BadRequest, "Video Title 不可空白").ToContentResult();

            string discordUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var userTrack = await _mainContext.Track.SingleOrDefaultAsync((x) => x.DiscordUserId == discordUserId && x.Guid == track.Guid);
            if (userTrack == null)
            {
                return new APIResult(HttpStatusCode.NotFound, "找不到歌曲").ToContentResult();
            }
            else
            {
                userTrack.VideoId = track.VideoId;
                userTrack.VideoTitle = track.VideoTitle;
                userTrack.StartAt = track.StartAt;
                userTrack.EndAt = track.EndAt;
                userTrack.Artist = track.Artist;
                userTrack.TrackTitle = track.TrackTitle;

                _mainContext.Track.Update(userTrack);
                await _mainContext.SaveChangesAsync();

                return new APIResult(HttpStatusCode.NoContent).ToContentResult();
            }
        }

        [HttpDelete]
        [EnableCors("allowDELETE")]
        public async Task<ContentResult> DeleteTrack([FromBody] DeleteTrack track)
        {
            if (string.IsNullOrEmpty(track.Guid))
                return new APIResult(HttpStatusCode.BadRequest, "Guid 不可空白").ToContentResult();

            string discordUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var userTrack = await _mainContext.Track.SingleOrDefaultAsync((x) => x.DiscordUserId == discordUserId && x.Guid == track.Guid);
            if (userTrack == null)
            {
                return new APIResult(HttpStatusCode.NotFound, "Guid 不存在").ToContentResult();
            }
            else
            {
                _mainContext.Track.Remove(userTrack);
                await _mainContext.SaveChangesAsync();
            }

            return new APIResult(HttpStatusCode.NoContent).ToContentResult();
        }

        [HttpPost]
        [EnableCors("allowPOST")]
        public async Task<ContentResult> AddCategory([FromBody] AddCategory category)
        {
            if (string.IsNullOrEmpty(category.Name))
                return new APIResult(HttpStatusCode.BadRequest, "Name 不可空白").ToContentResult();

            if (category.Position <= 0)
                return new APIResult(HttpStatusCode.BadRequest, "Position 不可小於 1").ToContentResult();

            if (_mainContext.Category.AsNoTracking().Any((x) => x.Position == category.Position))
                return new APIResult(HttpStatusCode.BadRequest, "Position 不可跟其他分類重複").ToContentResult();

            string discordUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

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

            return new APIResult(HttpStatusCode.Created, new { guid = result.Guid }).ToContentResult();
        }

        [HttpDelete]
        [EnableCors("allowDELETE")]
        public async Task<ContentResult> DeleteCategory([FromBody] DeleteCategory category)
        {
            if (string.IsNullOrEmpty(category.Guid))
                return new APIResult(HttpStatusCode.BadRequest, "Guid 不可空白").ToContentResult();

            string discordUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var userCategory = await _mainContext.Category.SingleOrDefaultAsync((x) => x.DiscordUserId == discordUserId && x.Guid == category.Guid);
            if (userCategory == null)
            {
                return new APIResult(HttpStatusCode.NotFound, "Guid 不存在").ToContentResult();
            }
            else
            {
                _mainContext.Category.Remove(userCategory);
                await _mainContext.SaveChangesAsync();
            }

            return new APIResult(HttpStatusCode.NoContent).ToContentResult();
        }

        [HttpPost]
        [EnableCors("allowPOST")]
        public async Task<ContentResult> SetCategoryTrack([FromBody] SetCategoryTrack setCategoryTrack)
        {
            if (string.IsNullOrEmpty(setCategoryTrack.Guid))
                return new APIResult(HttpStatusCode.BadRequest, "Guid 不可空白").ToContentResult();

            if (setCategoryTrack.Guid.Length != 36)
                return new APIResult(HttpStatusCode.BadRequest, "Guid 長度錯誤").ToContentResult();

            if (setCategoryTrack.TrackGuidAndPosition == null)
                return new APIResult(HttpStatusCode.BadRequest, "TrackGuidAndPosition 不可空白").ToContentResult();

            // https://stackoverflow.com/a/18547390
            if (setCategoryTrack.TrackGuidAndPosition.Values.GroupBy((x) => x).Any((x) => x.Count() > 1))
                return new APIResult(HttpStatusCode.BadRequest, "Position 不可重複").ToContentResult();

            string discordUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var userCategory = await _mainContext.Category.SingleOrDefaultAsync((x) => x.DiscordUserId == discordUserId && x.Guid == setCategoryTrack.Guid);
            if (userCategory == null)
                return new APIResult(HttpStatusCode.NotFound, "查無分類").ToContentResult();

            // 檢測 Guid 是否為 36 字元
            foreach (var item in setCategoryTrack.TrackGuidAndPosition.Keys)
            {
                if (item.Length != 36)
                    setCategoryTrack.TrackGuidAndPosition.Remove(item);
            }

            userCategory.VideoIdList = setCategoryTrack.TrackGuidAndPosition;
            _mainContext.Category.Update(userCategory);
            await _mainContext.SaveChangesAsync();

            return new APIResult(HttpStatusCode.OK, setCategoryTrack.TrackGuidAndPosition.Count).ToContentResult();
        }

        [HttpPost]
        [EnableCors("allowPOST")]
        public async Task<ContentResult> SetCategoriesPosition([FromBody] SetCategoriesPosition setCategoriesPosition)
        {
            if (setCategoriesPosition.GuidAndPosition == null)
                return new APIResult(HttpStatusCode.BadRequest).ToContentResult();

            // https://stackoverflow.com/a/18547390
            if (setCategoriesPosition.GuidAndPosition.Values.GroupBy((x) => x).Any((x) => x.Count() > 1))
                return new APIResult(HttpStatusCode.BadRequest, "Position 不可重複").ToContentResult();

            string discordUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var userCategories = _mainContext.Category.Where((x) => x.DiscordUserId == discordUserId);
            if (!userCategories.Any())
                return new APIResult(HttpStatusCode.NotFound, "使用者無任何分類").ToContentResult();

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
