using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Vpassbackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApiInfoController : ControllerBase
    {
        // This endpoint allows the frontend to check API connectivity
        [HttpGet("health")]
        [AllowAnonymous]
        public IActionResult GetHealthCheck()
        {
            return Ok(new
            {
                status = "up",
                timestamp = DateTime.UtcNow,
                version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0"
            });
        }

        // This endpoint provides information about available API endpoints for the frontend
        [HttpGet("endpoints")]
        [AllowAnonymous]
        public IActionResult GetEndpoints()
        {
            var endpoints = new Dictionary<string, object>
            {
                ["auth"] = new
                {
                    login = new { method = "POST", url = "/api/Auth/login", description = "Login for staff/admin users" },
                    loginCustomer = new { method = "POST", url = "/api/Auth/login-customer", description = "Login for customers" },
                    registerCustomer = new { method = "POST", url = "/api/Auth/register-customer", description = "Register a new customer" },
                    verifyOtp = new { method = "POST", url = "/api/Auth/verify-otp", description = "Verify OTP for email verification" }
                },
                ["serviceCenters"] = new
                {
                    getAll = new { method = "GET", url = "/api/ServiceCenters", description = "Get all service centers" },
                    getById = new { method = "GET", url = "/api/ServiceCenters/{id}", description = "Get service center by ID" },
                    create = new { method = "POST", url = "/api/ServiceCenters", description = "Create a new service center" }
                },
                ["services"] = new
                {
                    getAll = new { method = "GET", url = "/api/Services", description = "Get all services" },
                    getById = new { method = "GET", url = "/api/Services/{id}", description = "Get service by ID" }
                },
                ["vehicles"] = new
                {
                    getCustomerVehicles = new { method = "GET", url = "/api/Vehicles/customer/{customerId}", description = "Get vehicles for a customer" },
                    getById = new { method = "GET", url = "/api/Vehicles/{id}", description = "Get vehicle by ID" },
                    register = new { method = "POST", url = "/api/Vehicles", description = "Register a new vehicle" }
                },
                ["serviceReminders"] = new
                {
                    getForVehicle = new { method = "GET", url = "/api/ServiceReminders/vehicle/{vehicleId}", description = "Get service reminders for a vehicle" },
                    create = new { method = "POST", url = "/api/ServiceReminders", description = "Create a new service reminder" }
                },
                ["vehicleServiceHistory"] = new
                {
                    getForVehicle = new { method = "GET", url = "/api/VehicleServiceHistory/vehicle/{vehicleId}", description = "Get service history for a vehicle" },
                    addEntry = new { method = "POST", url = "/api/VehicleServiceHistory", description = "Add a new service history entry" }
                }
            };

            return Ok(new
            {
                apiName = "Vehicle Passport API",
                baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}",
                endpoints
            });
        }
    }
}
