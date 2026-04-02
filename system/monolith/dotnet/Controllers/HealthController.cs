using Microsoft.AspNetCore.Mvc;

namespace Optivem.EShop.Monolith.Controllers;

[ApiController]
public class HealthController : ControllerBase
{
    [HttpGet("/health")]
    public IActionResult CheckHealth()
    {
        return Ok(new { status = "UP" });
    }
}
