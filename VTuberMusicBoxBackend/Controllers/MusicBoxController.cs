using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
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

            return new APIResult(ResultStatusCode.OK, discordUserId).ToContentResult();
        }
    }
}
