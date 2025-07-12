using Vpassbackend.Models;
using Microsoft.EntityFrameworkCore;


namespace Vpassbackend.Data
{
    public static class SeedData
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (!context.UserRoles.Any())
            {
                context.UserRoles.AddRange(
                new UserRole { UserRoleName = "SuperAdmin" },
                new UserRole { UserRoleName = "Admin" },
                new UserRole { UserRoleName = "ServiceCenterAdmin" },
                new UserRole { UserRoleName = "Cashier" },
                new UserRole { UserRoleName = "DataOperator" }
            );
                await context.SaveChangesAsync();
            }

            if (!context.Users.Any())
            {
                var superAdmin = new User
                {
                    FirstName = "Super",
                    LastName = "Admin",
                    Email = "superadmin@example.com",
                    UserRoleId = 1 // SuperAdmin
                };

                // Explicitly hash the password with BCrypt for login
                var plainPassword = "SuperAdmin@123";
                superAdmin.Password = BCrypt.Net.BCrypt.HashPassword(plainPassword);

                context.Users.Add(superAdmin);
                await context.SaveChangesAsync();

                // Log the user creation for debugging
                Console.WriteLine($"SuperAdmin user created with email: {superAdmin.Email}");
            }
            else
            {
                // Ensure SuperAdmin password is correct for frontend testing
                var existingSuperAdmin = await context.Users
                    .FirstOrDefaultAsync(u => u.Email == "superadmin@example.com");

                if (existingSuperAdmin != null)
                {
                    bool isPasswordValid = BCrypt.Net.BCrypt.Verify("SuperAdmin@123", existingSuperAdmin.Password);

                    if (!isPasswordValid)
                    {
                        Console.WriteLine("Updating SuperAdmin password hash...");
                        existingSuperAdmin.Password = BCrypt.Net.BCrypt.HashPassword("SuperAdmin@123");
                        await context.SaveChangesAsync();
                        Console.WriteLine("SuperAdmin password updated successfully.");
                    }
                }
            }

            // Add sample service centers if none exist
            if (!context.ServiceCenters.Any())
            {
                context.ServiceCenters.AddRange(
                    new ServiceCenter
                    {
                        OwnerName = "John Smith",
                        VATNumber = "VAT123456",
                        RegisterationNumber = "REG001",
                        Station_name = "City Center Auto Services",
                        Email = "city.center@example.com",
                        Telephone = "123-456-7890",
                        Address = "123 Main St, City Center",
                        Station_status = "Active"
                    },
                    new ServiceCenter
                    {
                        OwnerName = "Alice Johnson",
                        VATNumber = "VAT789012",
                        RegisterationNumber = "REG002",
                        Station_name = "West Side Vehicle Repairs",
                        Email = "westside@example.com",
                        Telephone = "987-654-3210",
                        Address = "456 West Ave, Westside",
                        Station_status = "Active"
                    },
                    new ServiceCenter
                    {
                        OwnerName = "Robert Davis",
                        VATNumber = "VAT345678",
                        RegisterationNumber = "REG003",
                        Station_name = "Highway Auto Center",
                        Email = "highway@example.com",
                        Telephone = "555-123-4567",
                        Address = "789 Highway Blvd, North District",
                        Station_status = "Inactive"
                    }
                );
                await context.SaveChangesAsync();
            }

            // Add sample services if none exist
            if (!context.Services.Any())
            {
                // Add generic services available to potentially be offered by any service center
                var services = new List<Service>
                {
                    new Service
                    {
                        ServiceName = "Oil Change",
                        Description = "Standard oil change service with filter replacement",
                        BasePrice = 49.99m,
                        LoyaltyPoints = 5,
                        Category = "Maintenance"
                    },
                    new Service
                    {
                        ServiceName = "Tire Rotation",
                        Description = "Complete tire rotation service",
                        BasePrice = 29.99m,
                        LoyaltyPoints = 3,
                        Category = "Maintenance"
                    },
                    new Service
                    {
                        ServiceName = "Full Inspection",
                        Description = "Comprehensive vehicle inspection service",
                        BasePrice = 89.99m,
                        LoyaltyPoints = 10,
                        Category = "Inspection"
                    },
                    new Service
                    {
                        ServiceName = "Brake Replacement",
                        Description = "Front and rear brake pad replacement",
                        BasePrice = 199.99m,
                        LoyaltyPoints = 20,
                        Category = "Repair"
                    },
                    new Service
                    {
                        ServiceName = "Engine Tune-Up",
                        Description = "Complete engine tune-up and optimization",
                        BasePrice = 149.99m,
                        LoyaltyPoints = 15,
                        Category = "Performance"
                    },
                    new Service
                    {
                        ServiceName = "AC Service",
                        Description = "Air conditioning system check and recharge",
                        BasePrice = 79.99m,
                        LoyaltyPoints = 8,
                        Category = "Comfort"
                    },
                    new Service
                    {
                        ServiceName = "Transmission Flush",
                        Description = "Complete transmission fluid flush and replacement",
                        BasePrice = 129.99m,
                        LoyaltyPoints = 12,
                        Category = "Maintenance"
                    }
                };

                context.Services.AddRange(services);
                await context.SaveChangesAsync();
            }

            // Add ServiceCenterService relationships if none exist
            if (!context.ServiceCenterServices.Any())
            {
                var services = context.Services.ToList();
                var serviceCenters = context.ServiceCenters.ToList();

                if (services.Count > 0 && serviceCenters.Count > 0)
                {
                    // Get service centers
                    var cityCenter = serviceCenters.FirstOrDefault(sc => sc.Station_name == "City Center Auto Services");
                    var westSide = serviceCenters.FirstOrDefault(sc => sc.Station_name == "West Side Vehicle Repairs");
                    var highway = serviceCenters.FirstOrDefault(sc => sc.Station_name == "Highway Auto Center");

                    // Get services by name
                    var oilChange = services.FirstOrDefault(s => s.ServiceName == "Oil Change");
                    var tireRotation = services.FirstOrDefault(s => s.ServiceName == "Tire Rotation");
                    var inspection = services.FirstOrDefault(s => s.ServiceName == "Full Inspection");
                    var brakeReplacement = services.FirstOrDefault(s => s.ServiceName == "Brake Replacement");
                    var engineTuneUp = services.FirstOrDefault(s => s.ServiceName == "Engine Tune-Up");
                    var acService = services.FirstOrDefault(s => s.ServiceName == "AC Service");
                    var transmissionFlush = services.FirstOrDefault(s => s.ServiceName == "Transmission Flush");

                    // Assign services to City Center
                    if (cityCenter != null)
                    {
                        if (oilChange != null)
                            context.ServiceCenterServices.Add(new ServiceCenterService
                            {
                                ServiceCenter = cityCenter,
                                Service = oilChange,
                                IsAvailable = true,
                                CustomPrice = 45.99m,
                                Notes = "Our most popular service"
                            });

                        if (tireRotation != null)
                            context.ServiceCenterServices.Add(new ServiceCenterService
                            {
                                ServiceCenter = cityCenter,
                                Service = tireRotation,
                                IsAvailable = true
                            });

                        if (inspection != null)
                            context.ServiceCenterServices.Add(new ServiceCenterService
                            {
                                ServiceCenter = cityCenter,
                                Service = inspection,
                                IsAvailable = true,
                                CustomPrice = 79.99m
                            });
                    }

                    // Assign services to West Side
                    if (westSide != null)
                    {
                        if (brakeReplacement != null)
                            context.ServiceCenterServices.Add(new ServiceCenterService
                            {
                                ServiceCenter = westSide,
                                Service = brakeReplacement,
                                IsAvailable = true
                            });

                        if (engineTuneUp != null)
                            context.ServiceCenterServices.Add(new ServiceCenterService
                            {
                                ServiceCenter = westSide,
                                Service = engineTuneUp,
                                IsAvailable = true,
                                Notes = "Speciality service with performance guarantee"
                            });

                        if (oilChange != null)
                            context.ServiceCenterServices.Add(new ServiceCenterService
                            {
                                ServiceCenter = westSide,
                                Service = oilChange,
                                IsAvailable = true,
                                CustomPrice = 54.99m
                            });
                    }

                    // Assign services to Highway
                    if (highway != null)
                    {
                        if (acService != null)
                            context.ServiceCenterServices.Add(new ServiceCenterService
                            {
                                ServiceCenter = highway,
                                Service = acService,
                                IsAvailable = true
                            });

                        if (transmissionFlush != null)
                            context.ServiceCenterServices.Add(new ServiceCenterService
                            {
                                ServiceCenter = highway,
                                Service = transmissionFlush,
                                IsAvailable = true
                            });

                        if (inspection != null)
                            context.ServiceCenterServices.Add(new ServiceCenterService
                            {
                                ServiceCenter = highway,
                                Service = inspection,
                                IsAvailable = false,
                                Notes = "Temporarily unavailable due to equipment maintenance"
                            });
                    }

                    await context.SaveChangesAsync();
                }
            }

            // Add sample vehicles if none exist
            if (!context.Vehicles.Any())
            {
                // First, we need to find a customer to associate with these vehicles
                var customer = await context.Customers.FirstOrDefaultAsync();

                // If no customers exist, create a test customer
                if (customer == null)
                {
                    customer = new Customer
                    {
                        FirstName = "Test",
                        LastName = "Customer",
                        Email = "testcustomer@example.com",
                        Address = "123 Test Street",
                        PhoneNumber = "555-123-4567",
                        Password = BCrypt.Net.BCrypt.HashPassword("Customer@123"),
                        NIC = "TEST-123456789"
                    };
                    context.Customers.Add(customer);
                    await context.SaveChangesAsync();
                }

                var sampleVehicle1 = new Vehicle
                {
                    CustomerId = customer.CustomerId,
                    Brand = "Toyota",
                    Model = "Corolla",
                    Year = 2022,
                    ChassisNumber = "1HGCM82633A123456",
                    RegistrationNumber = "ABC-1234",
                    Fuel = "Gasoline",
                    Mileage = 15000,
                    Customer = customer
                };

                var sampleVehicle2 = new Vehicle
                {
                    CustomerId = customer.CustomerId,
                    Brand = "Honda",
                    Model = "Civic",
                    Year = 2021,
                    ChassisNumber = "2HGES16533H567890",
                    RegistrationNumber = "XYZ-5678",
                    Fuel = "Hybrid",
                    Mileage = 22000,
                    Customer = customer
                };

                context.Vehicles.AddRange(sampleVehicle1, sampleVehicle2);
                await context.SaveChangesAsync();
            }

            // Add sample service reminders if none exist
            if (!context.ServiceReminders.Any())
            {
                try
                {
                    var vehicles = await context.Vehicles.ToListAsync();
                    var services = await context.Services.ToListAsync();

                    if (vehicles.Count > 0 && services.Count > 0)
                    {
                        var vehicle1 = vehicles.FirstOrDefault();
                        var vehicle2 = vehicles.Count > 1 ? vehicles[1] : vehicle1;

                        var oilChangeService = services.FirstOrDefault(s => s.ServiceName == "Oil Change");
                        var tireRotationService = services.FirstOrDefault(s => s.ServiceName == "Tire Rotation");
                        var inspectionService = services.FirstOrDefault(s => s.ServiceName == "Full Inspection");

                        if (vehicle1 != null && oilChangeService != null)
                        {
                            context.ServiceReminders.Add(new ServiceReminder
                            {
                                VehicleId = vehicle1.VehicleId,
                                ServiceId = oilChangeService.ServiceId,
                                ReminderDate = DateTime.UtcNow.AddMonths(3),
                                IntervalMonths = 6,
                                NotifyBeforeDays = 14,
                                Notes = "Regular oil change reminder",
                                IsActive = true,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            });
                        }

                        if (vehicle1 != null && tireRotationService != null)
                        {
                            context.ServiceReminders.Add(new ServiceReminder
                            {
                                VehicleId = vehicle1.VehicleId,
                                ServiceId = tireRotationService.ServiceId,
                                ReminderDate = DateTime.UtcNow.AddMonths(2),
                                IntervalMonths = 6,
                                NotifyBeforeDays = 7,
                                Notes = "Tire rotation every 10,000 miles",
                                IsActive = true,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            });
                        }

                        if (vehicle2 != null && inspectionService != null)
                        {
                            context.ServiceReminders.Add(new ServiceReminder
                            {
                                VehicleId = vehicle2.VehicleId,
                                ServiceId = inspectionService.ServiceId,
                                ReminderDate = DateTime.UtcNow.AddDays(10),
                                IntervalMonths = 12,
                                NotifyBeforeDays = 30,
                                Notes = "Annual vehicle inspection",
                                IsActive = true,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            });
                        }

                        await context.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception, but don't throw it to allow the app to continue
                    Console.WriteLine($"Error seeding service reminders: {ex.Message}");
                }
            }
        }
    }
}