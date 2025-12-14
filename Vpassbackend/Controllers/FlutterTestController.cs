using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Primitives;

namespace Vpassbackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FlutterTestController : ControllerBase
    {
        // GET: api/FlutterTest
        [HttpGet]
        [AllowAnonymous]
        public IActionResult TestConnection()
        {
            // Get all headers for debugging
            var headers = new Dictionary<string, string>();
            foreach (var header in Request.Headers)
            {
                headers[header.Key] = header.Value.ToString();
            }

            // Return success message and information about the request
            return Ok(new
            {
                status = "success",
                message = "Flutter backend connection successful",
                timestamp = DateTime.UtcNow,
                requestInfo = new
                {
                    path = Request.Path.Value,
                    method = Request.Method,
                    scheme = Request.Scheme,
                    host = Request.Host.Value,
                    headers = headers
                }
            });
        }

        // GET: api/FlutterTest/cors
        [HttpGet("cors")]
        [AllowAnonymous]
        [HttpOptions("cors")]
        public IActionResult TestCors()
        {
            // Add CORS headers manually in case middleware isn't working
            Response.Headers["Access-Control-Allow-Origin"] = "*";
            Response.Headers["Access-Control-Allow-Headers"] = "Content-Type, Authorization";
            Response.Headers["Access-Control-Allow-Methods"] = "GET, POST, PUT, DELETE, OPTIONS";

            return Ok(new
            {
                status = "success",
                message = "CORS test successful",
                timestamp = DateTime.UtcNow,
                origin = Request.Headers.TryGetValue("Origin", out StringValues origin) ? origin.ToString() : "No origin header"
            });
        }

        // GET: api/FlutterTest/reminders/1
        [HttpGet("reminders/{vehicleId}")]
        [AllowAnonymous]
        public IActionResult GetTestReminders(int vehicleId)
        {
            // Return sample data so Flutter app can proceed with testing
            var sampleReminders = new List<object>
            {
                new {
                    serviceReminderId = 1,
                    vehicleId = vehicleId,
                    // serviceId removed
                    reminderDate = DateTime.UtcNow.AddDays(15),
                    intervalMonths = 6,
                    notifyBeforeDays = 14,
                    notes = "Oil change reminder",
                    isActive = true,
                    createdAt = DateTime.UtcNow.AddDays(-30),
                    updatedAt = DateTime.UtcNow,
                    serviceName = "Oil Change",
                    vehicleRegistrationNumber = "ABC-123",
                    vehicleBrand = "Toyota",
                    vehicleModel = "Corolla"
                },
                new {
                    serviceReminderId = 2,
                    vehicleId = vehicleId,
                    // serviceId removed
                    reminderDate = DateTime.UtcNow.AddDays(5),
                    intervalMonths = 3,
                    notifyBeforeDays = 7,
                    notes = "Tire rotation",
                    isActive = true,
                    createdAt = DateTime.UtcNow.AddDays(-45),
                    updatedAt = DateTime.UtcNow,
                    serviceName = "Tire Rotation",
                    vehicleRegistrationNumber = "ABC-123",
                    vehicleBrand = "Toyota",
                    vehicleModel = "Corolla"
                }
            };

            return Ok(sampleReminders);
        }
    }
}
