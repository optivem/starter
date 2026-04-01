using Microsoft.AspNetCore.Mvc;

namespace Optivem.Starter.Backend.Controllers;

[ApiController]
public class HealthController : ControllerBase
{
    [HttpGet("/health")]
    public IActionResult CheckHealth()
    {
        return Ok(new { status = "UP" });
    }
}
