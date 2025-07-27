using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vpassbackend.Data;
using Vpassbackend.Services;
using Vpassbackend.DTOs;

namespace Vpassbackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PdfController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IPdfService _pdfService;

        public PdfController(ApplicationDbContext context, IPdfService pdfService)
        {
            _context = context;
            _pdfService = pdfService;
        }

        /// <summary>
        /// Generate and download complete vehicle service history PDF
        /// </summary>
        /// <param name="vehicleId">Vehicle ID</param>
        /// <returns>PDF file for download</returns>
        [HttpGet("vehicle-service-history/{vehicleId}")]
        public async Task<IActionResult> GenerateVehicleServiceHistoryPdf(int vehicleId)
        {
            try
            {
                // Get vehicle with customer information
                var vehicle = await _context.Vehicles
                    .Include(v => v.Customer)
                    .FirstOrDefaultAsync(v => v.VehicleId == vehicleId);

                if (vehicle == null)
                {
                    return NotFound(new { message = "Vehicle not found" });
                }

                // Check payment status for this vehicle's latest invoice
                var invoice = await _context.Invoices
                    .Where(i => i.VehicleId == vehicleId)
                    .OrderByDescending(i => i.InvoiceDate)
                    .FirstOrDefaultAsync();
                if (invoice == null)
                {
                    return StatusCode(403, new { message = "No payment found for this vehicle. Please pay to download the PDF." });
                }
                var paymentLog = await _context.PaymentLogs
                    .Where(p => p.InvoiceId == invoice.InvoiceId)
                    .OrderByDescending(p => p.LogId)
                    .FirstOrDefaultAsync();
                if (paymentLog == null || paymentLog.Status != "Paid")
                {
                    return StatusCode(403, new { message = "Payment required. Please complete payment to download the PDF." });
                }

                // Get service history for the vehicle
                var serviceHistory = await _context.VehicleServiceHistories
                    .Include(vh => vh.ServiceCenter)
                    .Include(vh => vh.ServicedByUser)
                    .Where(vh => vh.VehicleId == vehicleId)
                    .OrderByDescending(vh => vh.ServiceDate)
                    .Select(vh => new ServiceHistoryDTO
                    {
                        ServiceHistoryId = vh.ServiceHistoryId,
                        VehicleId = vh.VehicleId,
                        ServiceType = vh.ServiceType,
                        Description = vh.Description,
                        Cost = vh.Cost,
                        ServiceDate = vh.ServiceDate,
                        Mileage = vh.Mileage,
                        IsVerified = vh.IsVerified,
                        ServiceCenterName = vh.ServiceCenter != null ? vh.ServiceCenter.Station_name : null,
                        ServicedByUserName = vh.ServicedByUser != null ? $"{vh.ServicedByUser.FirstName} {vh.ServicedByUser.LastName}" : null,
                        ExternalServiceCenterName = vh.ExternalServiceCenterName,
                        ReceiptDocumentPath = vh.ReceiptDocumentPath
                    })
                    .ToListAsync();

                // Generate PDF
                var pdfBytes = await _pdfService.GenerateVehicleServiceHistoryPdfAsync(vehicle, serviceHistory);

                // Create filename
                var fileName = $"ServiceHistory_{vehicle.RegistrationNumber}_{DateTime.Now:yyyyMMdd}.pdf";

                // Return file for download
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error generating PDF", error = ex.Message });
            }
        }

        /// <summary>
        /// Generate and download vehicle service history summary PDF
        /// </summary>
        /// <param name="vehicleId">Vehicle ID</param>
        /// <returns>PDF file for download</returns>
        [HttpGet("vehicle-service-summary/{vehicleId}")]
        public async Task<IActionResult> GenerateVehicleServiceSummaryPdf(int vehicleId)
        {
            try
            {
                // Get vehicle with customer information
                var vehicle = await _context.Vehicles
                    .Include(v => v.Customer)
                    .FirstOrDefaultAsync(v => v.VehicleId == vehicleId);

                if (vehicle == null)
                {
                    return NotFound(new { message = "Vehicle not found" });
                }

                // Check payment status for this vehicle's latest invoice
                var invoice = await _context.Invoices
                    .Where(i => i.VehicleId == vehicleId)
                    .OrderByDescending(i => i.InvoiceDate)
                    .FirstOrDefaultAsync();
                if (invoice == null)
                {
                    return StatusCode(403, new { message = "No payment found for this vehicle. Please pay to download the PDF." });
                }
                var paymentLog = await _context.PaymentLogs
                    .Where(p => p.InvoiceId == invoice.InvoiceId)
                    .OrderByDescending(p => p.LogId)
                    .FirstOrDefaultAsync();
                if (paymentLog == null || paymentLog.Status != "Paid")
                {
                    return StatusCode(403, new { message = "Payment required. Please complete payment to download the PDF." });
                }

                // Get service history for the vehicle
                var serviceHistory = await _context.VehicleServiceHistories
                    .Include(vh => vh.ServiceCenter)
                    .Include(vh => vh.ServicedByUser)
                    .Where(vh => vh.VehicleId == vehicleId)
                    .OrderByDescending(vh => vh.ServiceDate)
                    .Select(vh => new ServiceHistoryDTO
                    {
                        ServiceHistoryId = vh.ServiceHistoryId,
                        VehicleId = vh.VehicleId,
                        ServiceType = vh.ServiceType,
                        Description = vh.Description,
                        Cost = vh.Cost,
                        ServiceDate = vh.ServiceDate,
                        Mileage = vh.Mileage,
                        IsVerified = vh.IsVerified,
                        ServiceCenterName = vh.ServiceCenter != null ? vh.ServiceCenter.Station_name : null,
                        ServicedByUserName = vh.ServicedByUser != null ? $"{vh.ServicedByUser.FirstName} {vh.ServicedByUser.LastName}" : null,
                        ExternalServiceCenterName = vh.ExternalServiceCenterName,
                        ReceiptDocumentPath = vh.ReceiptDocumentPath
                    })
                    .ToListAsync();

                // Generate PDF
                var pdfBytes = await _pdfService.GenerateServiceHistorySummaryPdfAsync(vehicle, serviceHistory);

                // Create filename
                var fileName = $"ServiceSummary_{vehicle.RegistrationNumber}_{DateTime.Now:yyyyMMdd}.pdf";

                // Return file for download
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error generating PDF", error = ex.Message });
            }
        }

        /// <summary>
        /// Preview vehicle service history PDF in browser
        /// </summary>
        /// <param name="vehicleId">Vehicle ID</param>
        /// <returns>PDF file for preview</returns>
        [HttpGet("vehicle-service-history/{vehicleId}/preview")]
        public async Task<IActionResult> PreviewVehicleServiceHistoryPdf(int vehicleId)
        {
            try
            {
                // Get vehicle with customer information
                var vehicle = await _context.Vehicles
                    .Include(v => v.Customer)
                    .FirstOrDefaultAsync(v => v.VehicleId == vehicleId);

                if (vehicle == null)
                {
                    return NotFound(new { message = "Vehicle not found" });
                }

                // Check payment status for this vehicle's latest invoice
                var invoice = await _context.Invoices
                    .Where(i => i.VehicleId == vehicleId)
                    .OrderByDescending(i => i.InvoiceDate)
                    .FirstOrDefaultAsync();
                if (invoice == null)
                {
                    return StatusCode(403, new { message = "No payment found for this vehicle. Please pay to preview the PDF." });
                }
                var paymentLog = await _context.PaymentLogs
                    .Where(p => p.InvoiceId == invoice.InvoiceId)
                    .OrderByDescending(p => p.LogId)
                    .FirstOrDefaultAsync();
                if (paymentLog == null || paymentLog.Status != "Paid")
                {
                    return StatusCode(403, new { message = "Payment required. Please complete payment to preview the PDF." });
                }

                // Get service history for the vehicle
                var serviceHistory = await _context.VehicleServiceHistories
                    .Include(vh => vh.ServiceCenter)
                    .Include(vh => vh.ServicedByUser)
                    .Where(vh => vh.VehicleId == vehicleId)
                    .OrderByDescending(vh => vh.ServiceDate)
                    .Select(vh => new ServiceHistoryDTO
                    {
                        ServiceHistoryId = vh.ServiceHistoryId,
                        VehicleId = vh.VehicleId,
                        ServiceType = vh.ServiceType,
                        Description = vh.Description,
                        Cost = vh.Cost,
                        ServiceDate = vh.ServiceDate,
                        Mileage = vh.Mileage,
                        IsVerified = vh.IsVerified,
                        ServiceCenterName = vh.ServiceCenter != null ? vh.ServiceCenter.Station_name : null,
                        ServicedByUserName = vh.ServicedByUser != null ? $"{vh.ServicedByUser.FirstName} {vh.ServicedByUser.LastName}" : null,
                        ExternalServiceCenterName = vh.ExternalServiceCenterName,
                        ReceiptDocumentPath = vh.ReceiptDocumentPath
                    })
                    .ToListAsync();

                // Generate PDF
                var pdfBytes = await _pdfService.GenerateVehicleServiceHistoryPdfAsync(vehicle, serviceHistory);

                // Return file for preview (inline display)
                return File(pdfBytes, "application/pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error generating PDF", error = ex.Message });
            }
        }
    }
}
