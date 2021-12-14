using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Rookie.AMO.WebApi.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class TestController : Controller
    {
        public TestController() { }

        [HttpGet("authen")]
        [Authorize]
        public ActionResult TestAuthen()
        {
            return Ok("Success!");
        }

        [HttpGet("adminRole")]
        [Authorize(Policy = "ADMIN_ROLE_POLICY")]
        public ActionResult TestAdmin()
        {
            return Ok("Success!");
        }

        [HttpGet("staffRole")]
        [Authorize(Policy = "STAFF_ROLE_POLICY")]
        public ActionResult TestStaff()
        {
            return Ok("Success!");
        }
    }
}
