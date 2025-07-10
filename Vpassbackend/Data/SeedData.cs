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
                new UserRole { UserRoleId = 1, UserRoleName = "SuperAdmin" },
                new UserRole { UserRoleId = 2, UserRoleName = "Admin" },
                new UserRole { UserRoleId = 3, UserRoleName = "ServiceCenterAdmin" },
                new UserRole { UserRoleId = 4, UserRoleName = "Cashier" },
                new UserRole { UserRoleId = 5, UserRoleName = "DataOperator" }
            );
                await context.SaveChangesAsync();
            }

            if (!context.Users.Any())
            {
                // Get the SuperAdmin role that was just created
                // var superAdminRole = context.UserRoles.First(r => r.UserRoleName == "SuperAdmin");

                var superAdmin = new User
                {
                    FirstName = "Super",
                    LastName = "Admin",
                    Email = "superadmin@example.com",
                    UserRoleId = 1 // SuperAdmin
                };
                superAdmin.Password = BCrypt.Net.BCrypt.HashPassword("SuperAdmin@123");
                context.Users.Add(superAdmin);
                await context.SaveChangesAsync();
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
        }
    }
}