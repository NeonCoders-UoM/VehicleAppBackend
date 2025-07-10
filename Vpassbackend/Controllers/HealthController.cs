using Microsoft.AspNetCore.Mvc;

namespace Vpassbackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly ILogger<HealthController> _logger;

        public HealthController(ILogger<HealthController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get()
        {
            _logger.LogInformation("Health check endpoint hit at: {time}", DateTime.UtcNow);
            return Ok(new { Status = "Healthy", Message = "API is running", Timestamp = DateTime.UtcNow });
        }
    }
}
