using Microsoft.EntityFrameworkCore;
using Vpassbackend.Data;
using Vpassbackend.DTOs;
using Vpassbackend.Models;

namespace Vpassbackend.Services
{
    public interface IVehicleTransferService
    {
        Task<(bool Success, string Message, TransferResponseDTO? Transfer)> InitiateTransfer(InitiateTransferDTO dto, int sellerId);
        Task<(bool Success, string Message, TransferResponseDTO? Transfer)> AcceptTransfer(int transferId, int buyerId);
        Task<(bool Success, string Message)> RejectTransfer(int transferId, int buyerId);
        Task<(bool Success, string Message)> CancelTransfer(int transferId, int sellerId);
        Task<List<TransferResponseDTO>> GetPendingTransfersForBuyer(int customerId);
        Task<List<TransferResponseDTO>> GetTransferHistory(int vehicleId);
        Task ExpirePendingTransfers();
    }

    public class VehicleTransferService : IVehicleTransferService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public VehicleTransferService(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<(bool Success, string Message, TransferResponseDTO? Transfer)> InitiateTransfer(InitiateTransferDTO dto, int sellerId)
        {
            // Verify seller owns the vehicle
            var vehicle = await _context.Vehicles
                .Include(v => v.Customer)
                .FirstOrDefaultAsync(v => v.VehicleId == dto.VehicleId);

            if (vehicle == null)
            {
                return (false, "Vehicle not found", null);
            }

            if (vehicle.CustomerId != sellerId)
            {
                return (false, "You don't own this vehicle", null);
            }

            if (vehicle.Status == "PendingTransfer")
            {
                return (false, "This vehicle already has a pending transfer", null);
            }

            if (vehicle.Status == "Sold")
            {
                return (false, "This vehicle has already been sold", null);
            }

            // Find buyer by email
            var buyer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Email.ToLower() == dto.BuyerEmail.ToLower());

            if (buyer == null)
            {
                return (false, "Buyer not found. The buyer must have an account in the system.", null);
            }

            if (buyer.CustomerId == sellerId)
            {
                return (false, "You cannot transfer a vehicle to yourself", null);
            }

            // Create transfer request
            var transfer = new VehicleTransfer
            {
                VehicleId = dto.VehicleId,
                FromOwnerId = sellerId,
                ToOwnerId = buyer.CustomerId,
                InitiatedAt = DateTime.UtcNow,
                Status = "Pending",
                MileageAtTransfer = dto.MileageAtTransfer,
                SalePrice = dto.SalePrice,
                Notes = dto.Notes,
                ExpiresAt = DateTime.UtcNow.AddDays(7), // 7 days to accept
                Vehicle = vehicle,
                FromOwner = vehicle.Customer,
                ToOwner = buyer
            };

            _context.VehicleTransfers.Add(transfer);

            // Update vehicle status
            vehicle.Status = "PendingTransfer";

            await _context.SaveChangesAsync();

            // Send notification to buyer
            await _notificationService.CreateGeneralNotificationAsync(
                buyer.CustomerId,
                "Vehicle Transfer Request",
                $"You have received a vehicle transfer request for {vehicle.Brand} {vehicle.Model} ({vehicle.RegistrationNumber})",
                "High"
            );

            var responseDto = MapToDTO(transfer, vehicle, vehicle.Customer, buyer);
            return (true, "Transfer initiated successfully. Buyer has been notified.", responseDto);
        }

        public async Task<(bool Success, string Message, TransferResponseDTO? Transfer)> AcceptTransfer(int transferId, int buyerId)
        {
            var transfer = await _context.VehicleTransfers
                .Include(t => t.Vehicle)
                .Include(t => t.FromOwner)
                .Include(t => t.ToOwner)
                .FirstOrDefaultAsync(t => t.TransferId == transferId);

            if (transfer == null)
            {
                return (false, "Transfer not found", null);
            }

            if (transfer.ToOwnerId != buyerId)
            {
                return (false, "You are not the recipient of this transfer", null);
            }

            if (transfer.Status != "Pending")
            {
                return (false, $"This transfer is already {transfer.Status.ToLower()}", null);
            }

            if (transfer.ExpiresAt < DateTime.UtcNow)
            {
                transfer.Status = "Expired";
                await _context.SaveChangesAsync();
                return (false, "This transfer has expired", null);
            }

            // Complete the transfer
            transfer.Status = "Accepted";
            transfer.CompletedAt = DateTime.UtcNow;

            // Transfer vehicle ownership
            transfer.Vehicle.CustomerId = buyerId;
            transfer.Vehicle.Status = "Active";

            // Update vehicle mileage if provided
            if (transfer.MileageAtTransfer.HasValue)
            {
                transfer.Vehicle.Mileage = transfer.MileageAtTransfer.Value;
            }

            await _context.SaveChangesAsync();

            // Notify previous owner
            await _notificationService.CreateGeneralNotificationAsync(
                transfer.FromOwnerId,
                "Vehicle Transfer Completed",
                $"Your vehicle {transfer.Vehicle.Brand} {transfer.Vehicle.Model} ({transfer.Vehicle.RegistrationNumber}) has been transferred to {transfer.ToOwner.FirstName} {transfer.ToOwner.LastName}",
                "High"
            );

            var responseDto = MapToDTO(transfer, transfer.Vehicle, transfer.FromOwner, transfer.ToOwner);
            return (true, "Transfer accepted successfully. You are now the owner of this vehicle.", responseDto);
        }

        public async Task<(bool Success, string Message)> RejectTransfer(int transferId, int buyerId)
        {
            var transfer = await _context.VehicleTransfers
                .Include(t => t.Vehicle)
                .Include(t => t.FromOwner)
                .FirstOrDefaultAsync(t => t.TransferId == transferId);

            if (transfer == null)
            {
                return (false, "Transfer not found");
            }

            if (transfer.ToOwnerId != buyerId)
            {
                return (false, "You are not the recipient of this transfer");
            }

            if (transfer.Status != "Pending")
            {
                return (false, $"This transfer is already {transfer.Status.ToLower()}");
            }

            transfer.Status = "Rejected";
            transfer.CompletedAt = DateTime.UtcNow;

            // Reset vehicle status
            transfer.Vehicle.Status = "Active";

            await _context.SaveChangesAsync();

            // Notify seller
            await _notificationService.CreateGeneralNotificationAsync(
                transfer.FromOwnerId,
                "Vehicle Transfer Rejected",
                $"Your vehicle transfer request for {transfer.Vehicle.Brand} {transfer.Vehicle.Model} ({transfer.Vehicle.RegistrationNumber}) has been rejected",
                "Medium"
            );

            return (true, "Transfer rejected successfully");
        }

        public async Task<(bool Success, string Message)> CancelTransfer(int transferId, int sellerId)
        {
            var transfer = await _context.VehicleTransfers
                .Include(t => t.Vehicle)
                .Include(t => t.ToOwner)
                .FirstOrDefaultAsync(t => t.TransferId == transferId);

            if (transfer == null)
            {
                return (false, "Transfer not found");
            }

            if (transfer.FromOwnerId != sellerId)
            {
                return (false, "You are not the initiator of this transfer");
            }

            if (transfer.Status != "Pending")
            {
                return (false, $"Cannot cancel a transfer that is {transfer.Status.ToLower()}");
            }

            transfer.Status = "Cancelled";
            transfer.CompletedAt = DateTime.UtcNow;

            // Reset vehicle status
            transfer.Vehicle.Status = "Active";

            await _context.SaveChangesAsync();

            // Notify buyer
            await _notificationService.CreateGeneralNotificationAsync(
                transfer.ToOwnerId,
                "Vehicle Transfer Cancelled",
                $"The vehicle transfer offer for {transfer.Vehicle.Brand} {transfer.Vehicle.Model} ({transfer.Vehicle.RegistrationNumber}) has been cancelled",
                "Medium"
            );

            return (true, "Transfer cancelled successfully");
        }

        public async Task<List<TransferResponseDTO>> GetPendingTransfersForBuyer(int customerId)
        {
            var transfers = await _context.VehicleTransfers
                .Include(t => t.Vehicle)
                .Include(t => t.FromOwner)
                .Include(t => t.ToOwner)
                .Where(t => t.ToOwnerId == customerId && t.Status == "Pending" && t.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(t => t.InitiatedAt)
                .ToListAsync();

            return transfers.Select(t => MapToDTO(t, t.Vehicle, t.FromOwner, t.ToOwner)).ToList();
        }

        public async Task<List<TransferResponseDTO>> GetTransferHistory(int vehicleId)
        {
            var transfers = await _context.VehicleTransfers
                .Include(t => t.Vehicle)
                .Include(t => t.FromOwner)
                .Include(t => t.ToOwner)
                .Where(t => t.VehicleId == vehicleId)
                .OrderByDescending(t => t.InitiatedAt)
                .ToListAsync();

            return transfers.Select(t => MapToDTO(t, t.Vehicle, t.FromOwner, t.ToOwner)).ToList();
        }

        public async Task ExpirePendingTransfers()
        {
            var expiredTransfers = await _context.VehicleTransfers
                .Include(t => t.Vehicle)
                .Where(t => t.Status == "Pending" && t.ExpiresAt < DateTime.UtcNow)
                .ToListAsync();

            foreach (var transfer in expiredTransfers)
            {
                transfer.Status = "Expired";
                transfer.Vehicle.Status = "Active";
            }

            if (expiredTransfers.Any())
            {
                await _context.SaveChangesAsync();
            }
        }

        private TransferResponseDTO MapToDTO(VehicleTransfer transfer, Vehicle vehicle, Customer fromOwner, Customer toOwner)
        {
            return new TransferResponseDTO
            {
                TransferId = transfer.TransferId,
                VehicleId = transfer.VehicleId,
                VehicleName = $"{vehicle.Brand} {vehicle.Model}",
                RegistrationNumber = vehicle.RegistrationNumber,
                FromOwnerId = transfer.FromOwnerId,
                FromOwnerName = $"{fromOwner.FirstName} {fromOwner.LastName}",
                FromOwnerEmail = fromOwner.Email,
                ToOwnerId = transfer.ToOwnerId,
                ToOwnerName = $"{toOwner.FirstName} {toOwner.LastName}",
                ToOwnerEmail = toOwner.Email,
                InitiatedAt = transfer.InitiatedAt,
                CompletedAt = transfer.CompletedAt,
                Status = transfer.Status,
                MileageAtTransfer = transfer.MileageAtTransfer,
                SalePrice = transfer.SalePrice,
                Notes = transfer.Notes,
                ExpiresAt = transfer.ExpiresAt
            };
        }
    }
}
