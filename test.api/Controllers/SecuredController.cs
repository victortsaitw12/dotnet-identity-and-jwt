using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace test.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // This attribute restricts access to authenticated users
    public class SecuredController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { Message = "This is a secured endpoint", User = User.Identity.Name });
        }

        [HttpGet("admin")]
        [Authorize(Roles = "Admin")] // This endpoint requires the "Admin" role
        public IActionResult AdminOnly()
        {
            return Ok(new { Message = "This is an admin-only endpoint" });
        }
    }
}
