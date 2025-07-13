using Vpassbackend.Models;
using Vpassbackend.DTOs;

namespace Vpassbackend.Services
{
    public interface IPdfService
    {
        Task<byte[]> GenerateVehicleServiceHistoryPdfAsync(Vehicle vehicle, List<ServiceHistoryDTO> serviceHistory);
        Task<byte[]> GenerateServiceHistorySummaryPdfAsync(Vehicle vehicle, List<ServiceHistoryDTO> serviceHistory);
    }
}
