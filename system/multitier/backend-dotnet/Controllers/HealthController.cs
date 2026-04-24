using Microsoft.AspNetCore.Mvc;

namespace MyCompany.MyShop.Backend.Controllers;

[ApiController]
[Route("")]
public class HealthController : ControllerBase
{
    [HttpGet("health")]
    public IActionResult CheckHealth()
    {
        return Ok(new { status = "UP" });
    }
}
