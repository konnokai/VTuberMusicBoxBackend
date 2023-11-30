using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace VTuberMusicBoxBackend.Controllers
{
    [AllowAnonymous]
    [Route("[action]")]
    [ApiController]
    public class StatusCheckController : Controller
    {
        [EnableCors("allowGET")]
        [HttpGet]
        public ContentResult StatusCheck()
        {
            var result = new ContentResult();

            if (HttpContext.Request.Headers.Authorization != "Basic Margaret_North")
            {
                result.StatusCode = 403;
                result.Content = JsonConvert.SerializeObject(new { ErrorMessage = "403 Forbidden" });
            }
            else
            {
                result.StatusCode = 200;
                result.Content = "Ok";
            }

            return result;
        }
    }
}