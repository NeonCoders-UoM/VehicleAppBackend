using Microsoft.EntityFrameworkCore;
using Vpassbackend.Data;
using Vpassbackend.DTOs;
using Vpassbackend.Models;

namespace Vpassbackend.Services
{
    public interface IServiceRequestService
    {
        Task<ServiceRequestDto> CreateServiceRequestAsync(CreateServiceRequestDto dto, int userId);
        Task<List<ServiceRequestDto>> GetAllServiceRequestsAsync();
        Task<List<ServiceRequestDto>> GetPendingServiceRequestsAsync();
        Task<List<ServiceRequestDto>> GetMyServiceRequestsAsync(int userId);
        Task<ServiceRequestDto?> GetServiceRequestByIdAsync(int requestId);
        Task<ServiceRequestDto> ApproveServiceRequestAsync(int requestId, ApproveServiceRequestDto dto);
        Task<ServiceRequestDto> RejectServiceRequestAsync(int requestId, RejectServiceRequestDto dto);
    }

    public class ServiceRequestService : IServiceRequestService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly ILogger<ServiceRequestService> _logger;

        public ServiceRequestService(
            ApplicationDbContext context, 
            INotificationService notificationService,
            ILogger<ServiceRequestService> logger)
        {
            _context = context;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<ServiceRequestDto> CreateServiceRequestAsync(CreateServiceRequestDto dto, int userId)
        {
            // Check if service with same name already exists
            var existingService = await _context.Services
                .FirstOrDefaultAsync(s => s.ServiceName.ToLower() == dto.ServiceName.ToLower());

            if (existingService != null)
            {
                throw new InvalidOperationException("A service with this name already exists in the system");
            }

            // Check if there's a pending request with same name
            var pendingRequest = await _context.ServiceRequests
                .FirstOrDefaultAsync(sr => sr.ServiceName.ToLower() == dto.ServiceName.ToLower() 
                                        && sr.Status == ServiceRequestStatus.Pending);

            if (pendingRequest != null)
            {
                throw new InvalidOperationException("A pending request for this service already exists");
            }

            var serviceRequest = new ServiceRequest
            {
                ServiceName = dto.ServiceName,
                Description = dto.Description,
                Category = dto.Category,
                RequestedByUserId = userId,
                RequestedAt = DateTime.UtcNow,
                Status = ServiceRequestStatus.Pending
            };

            _context.ServiceRequests.Add(serviceRequest);
            await _context.SaveChangesAsync();

            // Notify admins about new request
            await NotifyAdminsAboutNewRequestAsync(serviceRequest);

            _logger.LogInformation($"Service request created: {serviceRequest.ServiceName} by User ID {userId}");

            return await MapToDtoAsync(serviceRequest);
        }

        public async Task<List<ServiceRequestDto>> GetAllServiceRequestsAsync()
        {
            var requests = await _context.ServiceRequests
                .Include(sr => sr.RequestedBy!)
                    .ThenInclude(u => u.ServiceCenter)
                .Include(sr => sr.ApprovedService)
                .OrderByDescending(sr => sr.RequestedAt)
                .ToListAsync();

            var dtos = new List<ServiceRequestDto>();
            foreach (var request in requests)
            {
                dtos.Add(await MapToDtoAsync(request));
            }

            return dtos;
        }

        public async Task<List<ServiceRequestDto>> GetPendingServiceRequestsAsync()
        {
            var requests = await _context.ServiceRequests
                .Include(sr => sr.RequestedBy!)
                    .ThenInclude(u => u.ServiceCenter)
                .Where(sr => sr.Status == ServiceRequestStatus.Pending)
                .OrderBy(sr => sr.RequestedAt)
                .ToListAsync();

            var dtos = new List<ServiceRequestDto>();
            foreach (var request in requests)
            {
                dtos.Add(await MapToDtoAsync(request));
            }

            return dtos;
        }

        public async Task<List<ServiceRequestDto>> GetMyServiceRequestsAsync(int userId)
        {
            var requests = await _context.ServiceRequests
                .Include(sr => sr.RequestedBy!)
                    .ThenInclude(u => u.ServiceCenter)
                .Include(sr => sr.ApprovedService)
                .Where(sr => sr.RequestedByUserId == userId)
                .OrderByDescending(sr => sr.RequestedAt)
                .ToListAsync();

            var dtos = new List<ServiceRequestDto>();
            foreach (var request in requests)
            {
                dtos.Add(await MapToDtoAsync(request));
            }

            return dtos;
        }

        public async Task<ServiceRequestDto?> GetServiceRequestByIdAsync(int requestId)
        {
            var request = await _context.ServiceRequests
                .Include(sr => sr.RequestedBy!)
                    .ThenInclude(u => u.ServiceCenter)
                .Include(sr => sr.ApprovedService)
                .FirstOrDefaultAsync(sr => sr.ServiceRequestId == requestId);

            if (request == null)
                return null;

            return await MapToDtoAsync(request);
        }

        public async Task<ServiceRequestDto> ApproveServiceRequestAsync(int requestId, ApproveServiceRequestDto dto)
        {
            var request = await _context.ServiceRequests
                .Include(sr => sr.RequestedBy!)
                    .ThenInclude(u => u.ServiceCenter)
                .FirstOrDefaultAsync(sr => sr.ServiceRequestId == requestId);

            if (request == null)
            {
                throw new InvalidOperationException("Service request not found");
            }

            if (request.Status != ServiceRequestStatus.Pending)
            {
                throw new InvalidOperationException($"Service request has already been {request.Status.ToString().ToLower()}");
            }

            // Create the new service
            var newService = new Service
            {
                ServiceName = request.ServiceName,
                Description = request.Description,
                Category = request.Category,
                BasePrice = dto.BasePrice
            };

            _context.Services.Add(newService);
            await _context.SaveChangesAsync();

            // Update the request
            request.Status = ServiceRequestStatus.Approved;
            request.ApprovedBasePrice = dto.BasePrice;
            request.ApprovedServiceId = newService.ServiceId;

            await _context.SaveChangesAsync();

            // Notify the requester
            await NotifyRequesterAboutApprovalAsync(request);

            _logger.LogInformation($"Service request approved: {request.ServiceName} (Service ID: {newService.ServiceId})");

            return await MapToDtoAsync(request);
        }

        public async Task<ServiceRequestDto> RejectServiceRequestAsync(int requestId, RejectServiceRequestDto dto)
        {
            var request = await _context.ServiceRequests
                .Include(sr => sr.RequestedBy!)
                    .ThenInclude(u => u.ServiceCenter)
                .FirstOrDefaultAsync(sr => sr.ServiceRequestId == requestId);

            if (request == null)
            {
                throw new InvalidOperationException("Service request not found");
            }

            if (request.Status != ServiceRequestStatus.Pending)
            {
                throw new InvalidOperationException($"Service request has already been {request.Status.ToString().ToLower()}");
            }

            request.Status = ServiceRequestStatus.Rejected;
            request.RejectionReason = dto.RejectionReason;

            await _context.SaveChangesAsync();

            // Notify the requester
            await NotifyRequesterAboutRejectionAsync(request);

            _logger.LogInformation($"Service request rejected: {request.ServiceName}");

            return await MapToDtoAsync(request);
        }

        // Helper methods
        private async Task<ServiceRequestDto> MapToDtoAsync(ServiceRequest request)
        {
            return new ServiceRequestDto
            {
                ServiceRequestId = request.ServiceRequestId,
                ServiceName = request.ServiceName,
                Description = request.Description,
                Category = request.Category,
                Status = request.Status.ToString(),
                ApprovedBasePrice = request.ApprovedBasePrice,
                RejectionReason = request.RejectionReason,
                RequestedByUserId = request.RequestedByUserId,
                RequestedByName = request.RequestedBy != null 
                    ? $"{request.RequestedBy.FirstName} {request.RequestedBy.LastName}" 
                    : null,
                RequestedByEmail = request.RequestedBy?.Email,
                RequestedAt = request.RequestedAt,
                ServiceCenterId = request.RequestedBy?.Station_id,
                ServiceCenterName = request.RequestedBy?.ServiceCenter?.Station_name,
                ApprovedServiceId = request.ApprovedServiceId
            };
        }

        private async Task NotifyAdminsAboutNewRequestAsync(ServiceRequest request)
        {
            try
            {
                // Get all admin and superadmin users
                var adminRoles = await _context.UserRoles
                    .Where(ur => ur.UserRoleName == "Admin" || ur.UserRoleName == "SuperAdmin")
                    .Select(ur => ur.UserRoleId)
                    .ToListAsync();

                var adminUsers = await _context.Users
                    .Where(u => adminRoles.Contains(u.UserRoleId))
                    .ToListAsync();

                // Since notifications are for customers, we'll log this instead
                // or you can create a separate admin notification system
                _logger.LogInformation($"New service request notification: {request.ServiceName} from User ID {request.RequestedByUserId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying admins about service request");
            }
        }

        private async Task NotifyRequesterAboutApprovalAsync(ServiceRequest request)
        {
            try
            {
                _logger.LogInformation($"Service request approved notification: {request.ServiceName} for User ID {request.RequestedByUserId}");
                // If you have a user notification system, implement it here
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying requester about approval");
            }
        }

        private async Task NotifyRequesterAboutRejectionAsync(ServiceRequest request)
        {
            try
            {
                _logger.LogInformation($"Service request rejected notification: {request.ServiceName} for User ID {request.RequestedByUserId}");
                // If you have a user notification system, implement it here
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying requester about rejection");
            }
        }
    }
}
