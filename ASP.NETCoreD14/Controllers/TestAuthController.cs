using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASP.NETCoreD14.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestAuthController : ControllerBase
    {
        /*------------------------------------------------------------------*/
        [HttpGet]
        [Route("V01")]
        [Authorize]
        public ActionResult GetV01()
        {
            return Ok("This is a protected endpoint. You are authenticated.");
        }
        /*------------------------------------------------------------------*/
        [HttpGet]
        [Route("V02")]
        [Authorize(Policy = "AdminOnly")]
        public ActionResult GetV02()
        {
            return Ok("This is a protected endpoint. You are authenticated.");
        }
        /*------------------------------------------------------------------*/
    }
}
