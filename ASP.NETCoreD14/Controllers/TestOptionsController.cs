using ASP.NETCoreD14.Srttings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ASP.NETCoreD14.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestOptionsController : ControllerBase
    {
        /*------------------------------------------------------------------*/
        private readonly JwtSettings _jwtSettings;
        /*------------------------------------------------------------------*/
        public TestOptionsController(IOptions<JwtSettings> options)
        {
            _jwtSettings = options.Value;
        }
        /*------------------------------------------------------------------*/
        [HttpGet]
        public ActionResult Get()
        {
            return Ok(_jwtSettings);
        }
        /*------------------------------------------------------------------*/
    }
}
