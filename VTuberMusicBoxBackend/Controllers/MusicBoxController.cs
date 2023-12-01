using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using VTuberMusicBoxBackend.Models;

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

            var userData = await _mainContext.User.AsNoTracking().SingleOrDefaultAsync((x) => x.DiscordId == discordUserId);

            return new APIResult(ResultStatusCode.OK,
                new
                {
                    track_list = userData?.TrackList,
                    categorie_list = userData?.CategorieList,
                    favorite_track_list = userData?.FavoriteTrackList
                }).ToContentResult();
        }
    }
}
