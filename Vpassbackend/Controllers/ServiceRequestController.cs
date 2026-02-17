using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Vpassbackend.DTOs;
using Vpassbackend.Services;

namespace Vpassbackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ServiceRequestController : ControllerBase
    {
        private readonly IServiceRequestService _serviceRequestService;

        public ServiceRequestController(IServiceRequestService serviceRequestService)
        {
            _serviceRequestService = serviceRequestService;
        }

        /// <summary>
        /// Service Center Admin creates a service request
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "ServiceCenterAdmin")]
        public async Task<IActionResult> CreateServiceRequest([FromBody] CreateServiceRequestDto dto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized("Invalid user authentication");
                }

                var result = await _serviceRequestService.CreateServiceRequestAsync(dto, userId);
                return Ok(new { success = true, message = "Service request submitted successfully", data = result });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while creating the service request" });
            }
        }

        /// <summary>
        /// Admin/SuperAdmin gets all service requests
        /// </summary>
        [HttpGet("all")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> GetAllServiceRequests()
        {
            try
            {
                var result = await _serviceRequestService.GetAllServiceRequestsAsync();
                return Ok(new { success = true, data = result });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving service requests" });
            }
        }

        /// <summary>
        /// Admin/SuperAdmin gets pending service requests
        /// </summary>
        [HttpGet("pending")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> GetPendingServiceRequests()
        {
            try
            {
                var result = await _serviceRequestService.GetPendingServiceRequestsAsync();
                return Ok(new { success = true, data = result });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving pending requests" });
            }
        }

        /// <summary>
        /// Service Center Admin gets their own service requests
        /// </summary>
        [HttpGet("my-requests")]
        [Authorize(Roles = "ServiceCenterAdmin")]
        public async Task<IActionResult> GetMyServiceRequests()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized("Invalid user authentication");
                }

                var result = await _serviceRequestService.GetMyServiceRequestsAsync(userId);
                return Ok(new { success = true, data = result });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving your requests" });
            }
        }

        /// <summary>
        /// Get specific service request by ID
        /// </summary>
        [HttpGet("{requestId}")]
        [Authorize(Roles = "Admin,SuperAdmin,ServiceCenterAdmin")]
        public async Task<IActionResult> GetServiceRequestById(int requestId)
        {
            try
            {
                var result = await _serviceRequestService.GetServiceRequestByIdAsync(requestId);
                
                if (result == null)
                {
                    return NotFound(new { success = false, message = "Service request not found" });
                }

                return Ok(new { success = true, data = result });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving the service request" });
            }
        }

        /// <summary>
        /// Admin/SuperAdmin approves a service request
        /// </summary>
        [HttpPost("{requestId}/approve")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> ApproveServiceRequest(int requestId, [FromBody] ApproveServiceRequestDto dto)
        {
            try
            {
                var result = await _serviceRequestService.ApproveServiceRequestAsync(requestId, dto);
                return Ok(new { success = true, message = "Service request approved successfully", data = result });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while approving the request" });
            }
        }

        /// <summary>
        /// Admin/SuperAdmin rejects a service request
        /// </summary>
        [HttpPost("{requestId}/reject")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> RejectServiceRequest(int requestId, [FromBody] RejectServiceRequestDto dto)
        {
            try
            {
                var result = await _serviceRequestService.RejectServiceRequestAsync(requestId, dto);
                return Ok(new { success = true, message = "Service request rejected", data = result });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while rejecting the request" });
            }
        }
    }
}
