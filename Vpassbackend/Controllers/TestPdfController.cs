using Microsoft.AspNetCore.Mvc;
using Vpassbackend.Services;
using Vpassbackend.Models;
using Vpassbackend.DTOs;

namespace Vpassbackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestPdfController : ControllerBase
    {
        private readonly IPdfService _pdfService;

        public TestPdfController(IPdfService pdfService)
        {
            _pdfService = pdfService;
        }

        /// <summary>
        /// Test PDF generation with mock data showing both verified and unverified services
        /// </summary>
        /// <returns>PDF file for download</returns>
        [HttpGet("test-vehicle-service-history")]
        public async Task<IActionResult> TestVehicleServiceHistoryPdf()
        {
            try
            {
                // Create mock vehicle data
                var mockVehicle = new Vehicle
                {
                    VehicleId = 1,
                    RegistrationNumber = "ABC-1234",
                    Brand = "Toyota",
                    Model = "Camry",
                    Year = 2020,
                    Fuel = "Petrol",
                    Mileage = 25000,
                    ChassisNumber = "1234567890123456",
                    CustomerId = 1,
                    Customer = new Customer
                    {
                        CustomerId = 1,
                        FirstName = "John",
                        LastName = "Doe",
                        Email = "john.doe@example.com",
                        Address = "123 Main Street, Colombo 03",
                        PhoneNumber = "+94771234567",
                        NIC = "123456789V",
                        Password = "hashedpassword",
                        LoyaltyPoints = 150,
                        IsEmailVerified = true
                    }
                };

                // Create mock service history with both verified and unverified services
                var mockServiceHistory = new List<ServiceHistoryDTO>
                {
                    // Verified service from registered service center
                    new ServiceHistoryDTO
                    {
                        ServiceHistoryId = 1,
                        VehicleId = 1,
                        ServiceType = "Full Service",
                        Description = "Complete engine service including oil change, filter replacement, and general inspection",
                        Cost = 12500,
                        ServiceDate = DateTime.Now.AddDays(-30),
                        Mileage = 24500,
                        IsVerified = true,
                        ServiceCenterName = "Toyota Service Center - Colombo",
                        ServicedByUserName = "Michael Johnson"
                    },
                    
                    // Unverified service (self-reported)
                    new ServiceHistoryDTO
                    {
                        ServiceHistoryId = 2,
                        VehicleId = 1,
                        ServiceType = "Tire Rotation",
                        Description = "Rotated all four tires and checked tire pressure",
                        Cost = 1500,
                        ServiceDate = DateTime.Now.AddDays(-45),
                        Mileage = 24200,
                        IsVerified = false,
                        ExternalServiceCenterName = "Quick Fix Auto - Kandy"
                    },
                    
                    // Verified service from registered service center
                    new ServiceHistoryDTO
                    {
                        ServiceHistoryId = 3,
                        VehicleId = 1,
                        ServiceType = "Brake Service",
                        Description = "Brake pad replacement and brake fluid change",
                        Cost = 18000,
                        ServiceDate = DateTime.Now.AddDays(-60),
                        Mileage = 23800,
                        IsVerified = true,
                        ServiceCenterName = "AutoCare Lanka - Galle Road",
                        ServicedByUserName = "Sarah Williams"
                    },
                    
                    // Unverified service (self-reported)
                    new ServiceHistoryDTO
                    {
                        ServiceHistoryId = 4,
                        VehicleId = 1,
                        ServiceType = "Oil Change",
                        Description = "Self-performed oil change with premium oil",
                        Cost = 3500,
                        ServiceDate = DateTime.Now.AddDays(-90),
                        Mileage = 23200,
                        IsVerified = false,
                        ExternalServiceCenterName = "Self Service"
                    },
                    
                    // Verified service from registered service center
                    new ServiceHistoryDTO
                    {
                        ServiceHistoryId = 5,
                        VehicleId = 1,
                        ServiceType = "Air Conditioning Service",
                        Description = "A/C system cleaning and refrigerant refill",
                        Cost = 8500,
                        ServiceDate = DateTime.Now.AddDays(-120),
                        Mileage = 22800,
                        IsVerified = true,
                        ServiceCenterName = "Cool Air Service Station",
                        ServicedByUserName = "David Chen"
                    },
                    
                    // Unverified service (external service center)
                    new ServiceHistoryDTO
                    {
                        ServiceHistoryId = 6,
                        VehicleId = 1,
                        ServiceType = "Battery Replacement",
                        Description = "Replaced old battery with new Exide battery",
                        Cost = 15000,
                        ServiceDate = DateTime.Now.AddDays(-180),
                        Mileage = 22000,
                        IsVerified = false,
                        ExternalServiceCenterName = "City Battery Center"
                    }
                };

                // Generate PDF
                var pdfBytes = await _pdfService.GenerateVehicleServiceHistoryPdfAsync(mockVehicle, mockServiceHistory);

                // Create filename
                var fileName = $"TestServiceHistory_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

                // Return file for download
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error generating test PDF", error = ex.Message });
            }
        }

        /// <summary>
        /// Test PDF generation with no service history data
        /// </summary>
        /// <returns>PDF file for download</returns>
        [HttpGet("test-empty-service-history")]
        public async Task<IActionResult> TestEmptyServiceHistoryPdf()
        {
            try
            {
                // Create mock vehicle data
                var mockVehicle = new Vehicle
                {
                    VehicleId = 2,
                    RegistrationNumber = "XYZ-9876",
                    Brand = "Honda",
                    Model = "Civic",
                    Year = 2018,
                    Fuel = "Petrol",
                    Mileage = 45000,
                    ChassisNumber = "0987654321098765",
                    CustomerId = 2,
                    Customer = new Customer
                    {
                        CustomerId = 2,
                        FirstName = "Jane",
                        LastName = "Smith",
                        Email = "jane.smith@example.com",
                        Address = "456 Oak Avenue, Kandy",
                        PhoneNumber = "+94779876543",
                        NIC = "987654321V",
                        Password = "hashedpassword",
                        LoyaltyPoints = 75,
                        IsEmailVerified = true
                    }
                };

                // Empty service history
                var emptyServiceHistory = new List<ServiceHistoryDTO>();

                // Generate PDF
                var pdfBytes = await _pdfService.GenerateVehicleServiceHistoryPdfAsync(mockVehicle, emptyServiceHistory);

                // Create filename
                var fileName = $"TestEmptyServiceHistory_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

                // Return file for download
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error generating test PDF", error = ex.Message });
            }
        }

        /// <summary>
        /// Test PDF generation with only verified services
        /// </summary>
        /// <returns>PDF file for download</returns>
        [HttpGet("test-verified-only")]
        public async Task<IActionResult> TestVerifiedOnlyPdf()
        {
            try
            {
                // Create mock vehicle data
                var mockVehicle = new Vehicle
                {
                    VehicleId = 3,
                    RegistrationNumber = "DEF-5678",
                    Brand = "Nissan",
                    Model = "Altima",
                    Year = 2019,
                    Fuel = "Petrol",
                    Mileage = 35000,
                    ChassisNumber = "1357924680135792",
                    CustomerId = 3,
                    Customer = new Customer
                    {
                        CustomerId = 3,
                        FirstName = "Robert",
                        LastName = "Johnson",
                        Email = "robert.johnson@example.com",
                        Address = "789 Pine Road, Galle",
                        PhoneNumber = "+94765432109",
                        NIC = "654321987V",
                        Password = "hashedpassword",
                        LoyaltyPoints = 200,
                        IsEmailVerified = true
                    }
                };

                // Only verified services
                var verifiedOnlyHistory = new List<ServiceHistoryDTO>
                {
                    new ServiceHistoryDTO
                    {
                        ServiceHistoryId = 7,
                        VehicleId = 3,
                        ServiceType = "Scheduled Maintenance",
                        Description = "60,000 km scheduled maintenance service",
                        Cost = 25000,
                        ServiceDate = DateTime.Now.AddDays(-15),
                        Mileage = 34800,
                        IsVerified = true,
                        ServiceCenterName = "Nissan Authorized Service Center",
                        ServicedByUserName = "Emily Davis"
                    },
                    new ServiceHistoryDTO
                    {
                        ServiceHistoryId = 8,
                        VehicleId = 3,
                        ServiceType = "Transmission Service",
                        Description = "Transmission fluid change and filter replacement",
                        Cost = 22000,
                        ServiceDate = DateTime.Now.AddDays(-90),
                        Mileage = 34000,
                        IsVerified = true,
                        ServiceCenterName = "Premium Auto Service",
                        ServicedByUserName = "Mark Wilson"
                    }
                };

                // Generate PDF
                var pdfBytes = await _pdfService.GenerateVehicleServiceHistoryPdfAsync(mockVehicle, verifiedOnlyHistory);

                // Create filename
                var fileName = $"TestVerifiedOnly_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

                // Return file for download
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error generating test PDF", error = ex.Message });
            }
        }

        /// <summary>
        /// Test PDF generation with only unverified services
        /// </summary>
        /// <returns>PDF file for download</returns>
        [HttpGet("test-unverified-only")]
        public async Task<IActionResult> TestUnverifiedOnlyPdf()
        {
            try
            {
                // Create mock vehicle data
                var mockVehicle = new Vehicle
                {
                    VehicleId = 4,
                    RegistrationNumber = "GHI-2468",
                    Brand = "Mazda",
                    Model = "CX-5",
                    Year = 2021,
                    Fuel = "Petrol",
                    Mileage = 15000,
                    ChassisNumber = "2468135790246813",
                    CustomerId = 4,
                    Customer = new Customer
                    {
                        CustomerId = 4,
                        FirstName = "Lisa",
                        LastName = "Anderson",
                        Email = "lisa.anderson@example.com",
                        Address = "321 Cedar Street, Negombo",
                        PhoneNumber = "+94712345678",
                        NIC = "321654987V",
                        Password = "hashedpassword",
                        LoyaltyPoints = 50,
                        IsEmailVerified = true
                    }
                };

                // Only unverified services
                var unverifiedOnlyHistory = new List<ServiceHistoryDTO>
                {
                    new ServiceHistoryDTO
                    {
                        ServiceHistoryId = 9,
                        VehicleId = 4,
                        ServiceType = "Car Wash & Wax",
                        Description = "Professional car wash and wax service",
                        Cost = 2500,
                        ServiceDate = DateTime.Now.AddDays(-7),
                        Mileage = 14950,
                        IsVerified = false,
                        ExternalServiceCenterName = "Shine Auto Detailing"
                    },
                    new ServiceHistoryDTO
                    {
                        ServiceHistoryId = 10,
                        VehicleId = 4,
                        ServiceType = "Interior Cleaning",
                        Description = "Deep interior cleaning and sanitization",
                        Cost = 3000,
                        ServiceDate = DateTime.Now.AddDays(-30),
                        Mileage = 14500,
                        IsVerified = false,
                        ExternalServiceCenterName = "Clean Car Solutions"
                    }
                };

                // Generate PDF
                var pdfBytes = await _pdfService.GenerateVehicleServiceHistoryPdfAsync(mockVehicle, unverifiedOnlyHistory);

                // Create filename
                var fileName = $"TestUnverifiedOnly_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

                // Return file for download
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error generating test PDF", error = ex.Message });
            }
        }
    }
}
