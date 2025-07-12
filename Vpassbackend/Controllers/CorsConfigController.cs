using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Vpassbackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "SuperAdmin")] // Only SuperAdmin can manage CORS origins
    public class CorsConfigController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public CorsConfigController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Get allowed CORS origins
        [HttpGet("origins")]
        [AllowAnonymous] // Allow this endpoint without authentication
        public IActionResult GetAllowedOrigins()
        {
            var allowedOrigins = new List<string>
            {
                "http://localhost:2027", // Flutter web
                "http://127.0.0.1:2027"  // Flutter web alternative
            };

            // In a production setup, you would fetch this from a database or configuration
            return Ok(new { allowedOrigins });
        }

        // This endpoint is for informational purposes only in this implementation
        // In a real application, you would store these in a database or configuration file
        [HttpPost("origins")]
        public IActionResult AddAllowedOrigin([FromBody] AddOriginRequest request)
        {
            // Validate the origin URL
            if (string.IsNullOrEmpty(request.Origin) || !Uri.TryCreate(request.Origin, UriKind.Absolute, out _))
            {
                return BadRequest(new { error = "Invalid origin URL" });
            }

            // In a production setup, you would add this to a database or configuration
            return Ok(new { message = "Origin added to allowed list", origin = request.Origin });
        }
    }

    public class AddOriginRequest
    {
        public string Origin { get; set; } = string.Empty;
    }
}
