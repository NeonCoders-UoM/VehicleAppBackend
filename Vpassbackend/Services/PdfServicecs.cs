using iTextSharp.text;
using iTextSharp.text.pdf;
using Vpassbackend.Models;
using Vpassbackend.DTOs;
using System.IO;
using Document = iTextSharp.text.Document;

namespace Vpassbackend.Services
{
    public class PdfService : IPdfService
    {
        public async Task<byte[]> GenerateVehicleServiceHistoryPdfAsync(Vehicle vehicle, List<ServiceHistoryDTO> serviceHistory)
        {
            return await Task.Run(() =>
            {
                using (var ms = new MemoryStream())
                {
                    var document = new Document(PageSize.A4, 36, 36, 54, 54);
                    var writer = PdfWriter.GetInstance(document, ms);

                    document.Open();

                    // Add header
                    AddHeader(document, "Vehicle Service History Report");

                    // Add vehicle information
                    AddVehicleInfo(document, vehicle);

                    // Add service history summary
                    AddServiceHistorySummary(document, serviceHistory);

                    // Add detailed service history
                    AddDetailedServiceHistory(document, serviceHistory);

                    // Add footer
                    AddFooter(document);

                    document.Close();

                    return ms.ToArray();
                }
            });
        }

        public async Task<byte[]> GenerateServiceHistorySummaryPdfAsync(Vehicle vehicle, List<ServiceHistoryDTO> serviceHistory)
        {
            return await Task.Run(() =>
            {
                using (var ms = new MemoryStream())
                {
                    var document = new Document(PageSize.A4, 36, 36, 54, 54);
                    var writer = PdfWriter.GetInstance(document, ms);

                    document.Open();

                    // Add header
                    AddHeader(document, "Vehicle Service History Summary");

                    // Add vehicle information
                    AddVehicleInfo(document, vehicle);

                    // Add service history summary only
                    AddServiceHistorySummary(document, serviceHistory);

                    // Add footer
                    AddFooter(document);

                    document.Close();

                    return ms.ToArray();
                }
            });
        }

        private void AddHeader(Document document, string title)
        {
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, new BaseColor(64, 64, 64));
            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16, new BaseColor(0, 0, 0));
            var dateFont = FontFactory.GetFont(FontFactory.HELVETICA, 10, new BaseColor(128, 128, 128));

            // Company header
            var companyParagraph = new Paragraph("Vehicle Passport System", headerFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 10
            };
            document.Add(companyParagraph);

            // Document title
            var titleParagraph = new Paragraph(title, titleFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 5
            };
            document.Add(titleParagraph);

            // Generation date
            var dateParagraph = new Paragraph($"Generated on: {DateTime.Now:dd/MM/yyyy HH:mm}", dateFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 20
            };
            document.Add(dateParagraph);

            // Add a line separator
            var line = new Paragraph("_____________________________________________________________________________")
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 20
            };
            document.Add(line);
        }

        private void AddVehicleInfo(Document document, Vehicle vehicle)
        {
            var sectionFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, new BaseColor(0, 0, 0));
            var labelFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11, new BaseColor(0, 0, 0));
            var valueFont = FontFactory.GetFont(FontFactory.HELVETICA, 11, new BaseColor(0, 0, 0));

            // Vehicle Information Section
            var vehicleSection = new Paragraph("Vehicle Information", sectionFont)
            {
                SpacingBefore = 10,
                SpacingAfter = 10
            };
            document.Add(vehicleSection);

            // Create a table for vehicle information
            var vehicleTable = new PdfPTable(4) { WidthPercentage = 100 };
            vehicleTable.SetWidths(new float[] { 25f, 25f, 25f, 25f });

            // Row 1
            AddTableCell(vehicleTable, "Registration Number:", labelFont, new BaseColor(211, 211, 211));
            AddTableCell(vehicleTable, vehicle.RegistrationNumber, valueFont, new BaseColor(255, 255, 255));
            AddTableCell(vehicleTable, "Owner:", labelFont, new BaseColor(211, 211, 211));
            AddTableCell(vehicleTable, $"{vehicle.Customer.FirstName} {vehicle.Customer.LastName}", valueFont, new BaseColor(255, 255, 255));

            // Row 2
            AddTableCell(vehicleTable, "Brand:", labelFont, new BaseColor(211, 211, 211));
            AddTableCell(vehicleTable, vehicle.Brand ?? "N/A", valueFont, new BaseColor(255, 255, 255));
            AddTableCell(vehicleTable, "Model:", labelFont, new BaseColor(211, 211, 211));
            AddTableCell(vehicleTable, vehicle.Model ?? "N/A", valueFont, new BaseColor(255, 255, 255));

            // Row 3
            AddTableCell(vehicleTable, "Year:", labelFont, new BaseColor(211, 211, 211));
            AddTableCell(vehicleTable, vehicle.Year?.ToString() ?? "N/A", valueFont, new BaseColor(255, 255, 255));
            AddTableCell(vehicleTable, "Fuel Type:", labelFont, new BaseColor(211, 211, 211));
            AddTableCell(vehicleTable, vehicle.Fuel ?? "N/A", valueFont, new BaseColor(255, 255, 255));

            // Row 4
            AddTableCell(vehicleTable, "Chassis Number:", labelFont, new BaseColor(211, 211, 211));
            AddTableCell(vehicleTable, vehicle.ChassisNumber ?? "N/A", valueFont, new BaseColor(255, 255, 255));
            AddTableCell(vehicleTable, "Current Mileage:", labelFont, new BaseColor(211, 211, 211));
            AddTableCell(vehicleTable, vehicle.Mileage?.ToString("N0") ?? "N/A", valueFont, new BaseColor(255, 255, 255));

            document.Add(vehicleTable);
        }

        private void AddServiceHistorySummary(Document document, List<ServiceHistoryDTO> serviceHistory)
        {
            var sectionFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, new BaseColor(0, 0, 0));
            var labelFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11, new BaseColor(0, 0, 0));
            var valueFont = FontFactory.GetFont(FontFactory.HELVETICA, 11, new BaseColor(0, 0, 0));

            // Service History Summary Section
            var summarySection = new Paragraph("Service History Summary", sectionFont)
            {
                SpacingBefore = 20,
                SpacingAfter = 10
            };
            document.Add(summarySection);

            if (!serviceHistory.Any())
            {
                var noDataParagraph = new Paragraph("No service history available for this vehicle.", valueFont)
                {
                    SpacingAfter = 20,
                    Alignment = Element.ALIGN_CENTER
                };
                document.Add(noDataParagraph);
                return;
            }

            // Calculate statistics
            var totalServices = serviceHistory.Count;
            var verifiedServices = serviceHistory.Count(s => s.IsVerified);
            var unverifiedServices = serviceHistory.Count(s => !s.IsVerified);
            var totalCost = serviceHistory.Sum(s => s.Cost);
            var averageCost = totalServices > 0 ? totalCost / totalServices : 0;
            var lastServiceDate = serviceHistory.MaxBy(s => s.ServiceDate)?.ServiceDate;

            // Create summary table
            var summaryTable = new PdfPTable(4) { WidthPercentage = 100 };
            summaryTable.SetWidths(new float[] { 25f, 25f, 25f, 25f });

            // Row 1
            AddTableCell(summaryTable, "Total Services:", labelFont, new BaseColor(211, 211, 211));
            AddTableCell(summaryTable, totalServices.ToString(), valueFont, new BaseColor(255, 255, 255));
            AddTableCell(summaryTable, "Total Cost:", labelFont, new BaseColor(211, 211, 211));
            AddTableCell(summaryTable, $"LKR {totalCost:N2}", valueFont, new BaseColor(255, 255, 255));

            // Row 2
            AddTableCell(summaryTable, "Verified Services:", labelFont, new BaseColor(200, 255, 200)); // Light green
            AddTableCell(summaryTable, verifiedServices.ToString(), valueFont, new BaseColor(255, 255, 255));
            AddTableCell(summaryTable, "Unverified Services:", labelFont, new BaseColor(255, 200, 200)); // Light red
            AddTableCell(summaryTable, unverifiedServices.ToString(), valueFont, new BaseColor(255, 255, 255));

            // Row 3
            AddTableCell(summaryTable, "Average Cost:", labelFont, new BaseColor(211, 211, 211));
            AddTableCell(summaryTable, $"LKR {averageCost:N2}", valueFont, new BaseColor(255, 255, 255));
            AddTableCell(summaryTable, "Last Service:", labelFont, new BaseColor(211, 211, 211));
            AddTableCell(summaryTable, lastServiceDate?.ToString("dd/MM/yyyy") ?? "N/A", valueFont, new BaseColor(255, 255, 255));

            document.Add(summaryTable);
        }

        private void AddDetailedServiceHistory(Document document, List<ServiceHistoryDTO> serviceHistory)
        {
            var sectionFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, new BaseColor(0, 0, 0));
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, new BaseColor(255, 255, 255));
            var verifiedFont = FontFactory.GetFont(FontFactory.HELVETICA, 9, new BaseColor(0, 0, 0));
            var unverifiedFont = FontFactory.GetFont(FontFactory.HELVETICA, 9, new BaseColor(64, 64, 64));

            // Detailed Service History Section
            var detailSection = new Paragraph("Detailed Service History", sectionFont)
            {
                SpacingBefore = 20,
                SpacingAfter = 10
            };
            document.Add(detailSection);

            if (!serviceHistory.Any())
            {
                return;
            }

            // Create detailed table
            var detailTable = new PdfPTable(6) { WidthPercentage = 100 };
            detailTable.SetWidths(new float[] { 15f, 20f, 25f, 20f, 10f, 10f });

            // Add headers
            AddTableCell(detailTable, "Date", headerFont, new BaseColor(64, 64, 64));
            AddTableCell(detailTable, "Service Type", headerFont, new BaseColor(64, 64, 64));
            AddTableCell(detailTable, "Description", headerFont, new BaseColor(64, 64, 64));
            AddTableCell(detailTable, "Service Provider", headerFont, new BaseColor(64, 64, 64));
            AddTableCell(detailTable, "Cost (LKR)", headerFont, new BaseColor(64, 64, 64));
            AddTableCell(detailTable, "Status", headerFont, new BaseColor(64, 64, 64));

            // Sort by date (newest first)
            var sortedHistory = serviceHistory.OrderByDescending(s => s.ServiceDate).ToList();

            foreach (var service in sortedHistory)
            {
                var font = service.IsVerified ? verifiedFont : unverifiedFont;
                var rowColor = service.IsVerified ? new BaseColor(255, 255, 255) : new BaseColor(248, 248, 248);

                AddTableCell(detailTable, service.ServiceDate.ToString("dd/MM/yyyy"), font, rowColor);
                AddTableCell(detailTable, service.ServiceType, font, rowColor);
                AddTableCell(detailTable, service.Description ?? "No description provided", font, rowColor);
                
                var serviceProvider = service.IsVerified 
                    ? service.ServiceCenterName ?? "Unknown Service Center"
                    : service.ExternalServiceCenterName ?? "Self-Reported";
                AddTableCell(detailTable, serviceProvider, font, rowColor);
                
                AddTableCell(detailTable, service.Cost.ToString("N2"), font, rowColor);
                
                var statusColor = service.IsVerified ? new BaseColor(76, 175, 80) : new BaseColor(255, 152, 0);
                var statusFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8, new BaseColor(255, 255, 255));
                AddTableCell(detailTable, service.IsVerified ? "VERIFIED" : "UNVERIFIED", statusFont, statusColor);
            }

            document.Add(detailTable);

            // Add legend
            AddLegend(document);
        }

        private void AddLegend(Document document)
        {
            var legendFont = FontFactory.GetFont(FontFactory.HELVETICA, 9, new BaseColor(0, 0, 0));
            var legendTitle = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, new BaseColor(0, 0, 0));

            var legendSection = new Paragraph("Legend:", legendTitle)
            {
                SpacingBefore = 15,
                SpacingAfter = 5
            };
            document.Add(legendSection);

            var verifiedParagraph = new Paragraph("• VERIFIED: Services performed at registered service centers with official records", legendFont)
            {
                SpacingAfter = 3
            };
            document.Add(verifiedParagraph);

            var unverifiedParagraph = new Paragraph("• UNVERIFIED: Self-reported services or services performed at non-registered centers", legendFont)
            {
                SpacingAfter = 10
            };
            document.Add(unverifiedParagraph);
        }

        private void AddFooter(Document document)
        {
            var footerFont = FontFactory.GetFont(FontFactory.HELVETICA, 8, new BaseColor(128, 128, 128));
            
            var footerParagraph = new Paragraph($"This document was generated electronically by Vehicle Passport System on {DateTime.Now:dd/MM/yyyy HH:mm}. " +
                "For verification of service records, please contact the respective service centers.", footerFont)
            {
                SpacingBefore = 30,
                Alignment = Element.ALIGN_CENTER
            };
            document.Add(footerParagraph);

            var disclaimerParagraph = new Paragraph("Note: Unverified service records are self-reported and may not reflect actual service performed.", footerFont)
            {
                SpacingBefore = 10,
                Alignment = Element.ALIGN_CENTER
            };
            document.Add(disclaimerParagraph);
        }

        private void AddTableCell(PdfPTable table, string text, Font font, BaseColor backgroundColor)
        {
            var cell = new PdfPCell(new Phrase(text, font))
            {
                BackgroundColor = backgroundColor,
                Padding = 8,
                Border = Rectangle.BOX,
                BorderColor = new BaseColor(211, 211, 211),
                BorderWidth = 0.5f
            };
            table.AddCell(cell);
        }
    }
}
