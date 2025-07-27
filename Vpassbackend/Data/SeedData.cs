using Vpassbackend.Models;
using Microsoft.EntityFrameworkCore;


namespace Vpassbackend.Data
{
    public static class SeedData
    {
        // User Role ID Constants for consistency
        public const int SUPER_ADMIN_ROLE_ID = 1;
        public const int ADMIN_ROLE_ID = 2;
        public const int SERVICE_CENTER_ADMIN_ROLE_ID = 3;
        public const int CASHIER_ROLE_ID = 4;
        public const int DATA_OPERATOR_ROLE_ID = 5;

        public static async Task SeedAsync(ApplicationDbContext context)
        {
            // Define user roles with explicit IDs for consistency
            if (!context.UserRoles.Any())
{
    await context.Database.OpenConnectionAsync();
    await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT [UserRoles] ON");

    context.UserRoles.AddRange(
        new UserRole { UserRoleId = SUPER_ADMIN_ROLE_ID, UserRoleName = "SuperAdmin" },
        new UserRole { UserRoleId = ADMIN_ROLE_ID, UserRoleName = "Admin" },
        new UserRole { UserRoleId = SERVICE_CENTER_ADMIN_ROLE_ID, UserRoleName = "ServiceCenterAdmin" },
        new UserRole { UserRoleId = CASHIER_ROLE_ID, UserRoleName = "Cashier" },
        new UserRole { UserRoleId = DATA_OPERATOR_ROLE_ID, UserRoleName = "DataOperator" }
    );

    await context.SaveChangesAsync();

    await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT [UserRoles] OFF");
    await context.Database.CloseConnectionAsync();
}

            // Add sample service centers if none exist (MUST be before Users to avoid FK constraint issues)
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

            if (!context.Users.Any())
            {
                // Get the SuperAdmin role to ensure we're using the correct UserRoleId
                var superAdminRole = await context.UserRoles
                    .FirstOrDefaultAsync(r => r.UserRoleName == "SuperAdmin");

                if (superAdminRole == null)
                {
                    throw new InvalidOperationException("SuperAdmin role not found. Please ensure user roles are seeded first.");
                }

                var superAdmin = new User
                {
                    FirstName = "Super",
                    LastName = "Admin",
                    Email = "superadmin@example.com",
                    UserRoleId = superAdminRole.UserRoleId // Use the actual SuperAdmin role ID
                };

                // Explicitly hash the password with BCrypt for login
                var plainPassword = "SuperAdmin@123";
                superAdmin.Password = BCrypt.Net.BCrypt.HashPassword(plainPassword);

                context.Users.Add(superAdmin);

                // Add sample users for other roles
                var adminRole = await context.UserRoles
                    .FirstOrDefaultAsync(r => r.UserRoleName == "Admin");
                var serviceCenterAdminRole = await context.UserRoles
                    .FirstOrDefaultAsync(r => r.UserRoleName == "ServiceCenterAdmin");
                var cashierRole = await context.UserRoles
                    .FirstOrDefaultAsync(r => r.UserRoleName == "Cashier");
                var dataOperatorRole = await context.UserRoles
                    .FirstOrDefaultAsync(r => r.UserRoleName == "DataOperator");

                if (adminRole != null)
                {
                    var admin = new User
                    {
                        FirstName = "System",
                        LastName = "Admin",
                        Email = "admin@example.com",
                        UserRoleId = adminRole.UserRoleId,
                        Password = BCrypt.Net.BCrypt.HashPassword("Admin@123")
                    };
                    context.Users.Add(admin);
                }

                if (serviceCenterAdminRole != null)
                {
                    var serviceCenterAdmin = new User
                    {
                        FirstName = "Service Center",
                        LastName = "Manager",
                        Email = "serviceadmin@example.com",
                        UserRoleId = serviceCenterAdminRole.UserRoleId,
                        Password = BCrypt.Net.BCrypt.HashPassword("ServiceAdmin@123")
                    };
                    context.Users.Add(serviceCenterAdmin);
                }

                if (cashierRole != null)
                {
                    var cashier = new User
                    {
                        FirstName = "John",
                        LastName = "Cashier",
                        Email = "cashier@example.com",
                        UserRoleId = cashierRole.UserRoleId,
                        Password = BCrypt.Net.BCrypt.HashPassword("Cashier@123")
                    };
                    context.Users.Add(cashier);
                }

                if (dataOperatorRole != null)
                {
                    var dataOperator = new User
                    {
                        FirstName = "Data",
                        LastName = "Operator",
                        Email = "dataoperator@example.com",
                        UserRoleId = dataOperatorRole.UserRoleId,
                        Password = BCrypt.Net.BCrypt.HashPassword("DataOp@123")
                    };
                    context.Users.Add(dataOperator);
                }

                await context.SaveChangesAsync();

                // Log the user creation for debugging
                Console.WriteLine($"SuperAdmin user created with email: {superAdmin.Email}");
                Console.WriteLine($"Sample users created for all user roles.");
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
                        Category = "Maintenance"
                    },
                    new Service
                    {
                        ServiceName = "Tire Rotation",
                        Description = "Complete tire rotation service",
                        BasePrice = 29.99m,
                        Category = "Maintenance"
                    },
                    new Service
                    {
                        ServiceName = "Full Inspection",
                        Description = "Comprehensive vehicle inspection service",
                        BasePrice = 89.99m,
                        Category = "Inspection"
                    },
                    new Service
                    {
                        ServiceName = "Brake Replacement",
                        Description = "Front and rear brake pad replacement",
                        BasePrice = 199.99m,
                        Category = "Repair"
                    },
                    new Service
                    {
                        ServiceName = "Engine Tune-Up",
                        Description = "Complete engine tune-up and optimization",
                        BasePrice = 149.99m,
                        Category = "Performance"
                    },
                    new Service
                    {
                        ServiceName = "AC Service",
                        Description = "Air conditioning system check and recharge",
                        BasePrice = 79.99m,
                        Category = "Comfort"
                    },
                    new Service
                    {
                        ServiceName = "Transmission Flush",
                        Description = "Complete transmission fluid flush and replacement",
                        BasePrice = 129.99m,
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
                                ReminderDate = DateTime.UtcNow.AddDays(5), // Due in 5 days
                                IntervalMonths = 6,
                                NotifyBeforeDays = 14, // Will trigger notification now
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
                                ReminderDate = DateTime.UtcNow.AddDays(-2), // Overdue by 2 days
                                IntervalMonths = 6,
                                NotifyBeforeDays = 7, // Will trigger notification now
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
                                ReminderDate = DateTime.UtcNow.AddDays(25), // Due in 25 days
                                IntervalMonths = 12,
                                NotifyBeforeDays = 30, // Will trigger notification now
                                Notes = "Annual vehicle inspection",
                                IsActive = true,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            });
                        }

                        // Add a brake service reminder that's overdue
                        var brakeService = services.FirstOrDefault(s => s.ServiceName == "Brake Replacement");
                        if (vehicle2 != null && brakeService != null)
                        {
                            context.ServiceReminders.Add(new ServiceReminder
                            {
                                VehicleId = vehicle2.VehicleId,
                                ServiceId = brakeService.ServiceId,
                                ReminderDate = DateTime.UtcNow.AddDays(-10), // Overdue by 10 days
                                IntervalMonths = 12,
                                NotifyBeforeDays = 7, // Will trigger notification now
                                Notes = "Brake safety check - important!",
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

            // Add sample notifications based on service reminders if none exist
            if (!context.Notifications.Any())
            {
                try
                {
                    var serviceReminders = await context.ServiceReminders
                        .Include(sr => sr.Vehicle)
                            .ThenInclude(v => v.Customer)
                        .Include(sr => sr.Service)
                        .Where(sr => sr.IsActive)
                        .ToListAsync();

                    if (serviceReminders.Count > 0)
                    {
                        var today = DateTime.UtcNow.Date;

                        foreach (var reminder in serviceReminders)
                        {
                            if (reminder.Vehicle?.Customer != null)
                            {
                                var daysUntilDue = (reminder.ReminderDate.Date - today).Days;

                                // Create notifications for reminders that are due or coming up
                                if (daysUntilDue <= reminder.NotifyBeforeDays)
                                {
                                    var priorityLevel = daysUntilDue <= 0 ? "Critical" :
                                                      daysUntilDue <= 3 ? "High" : "Medium";

                                    var title = daysUntilDue <= 0
                                        ? $"{reminder.Service?.ServiceName ?? "Service"} Overdue"
                                        : $"{reminder.Service?.ServiceName ?? "Service"} Due Soon";

                                    var message = daysUntilDue <= 0
                                        ? $"Your {reminder.Vehicle.RegistrationNumber} is overdue for {reminder.Service?.ServiceName ?? "service"}. Please schedule an appointment immediately."
                                        : $"Your {reminder.Vehicle.RegistrationNumber} needs {reminder.Service?.ServiceName ?? "service"} in {daysUntilDue} day{(daysUntilDue == 1 ? "" : "s")}. Please schedule an appointment.";

                                    var notification = new Notification
                                    {
                                        CustomerId = reminder.Vehicle.CustomerId,
                                        Title = title,
                                        Message = message,
                                        Type = "service_reminder",
                                        Priority = priorityLevel,
                                        PriorityColor = priorityLevel switch
                                        {
                                            "Critical" => "#DC2626", // Red
                                            "High" => "#EA580C",     // Orange
                                            "Medium" => "#3B82F6",   // Blue
                                            _ => "#3B82F6"           // Default blue
                                        },
                                        ServiceReminderId = reminder.ServiceReminderId,
                                        VehicleId = reminder.VehicleId,
                                        VehicleRegistrationNumber = reminder.Vehicle.RegistrationNumber,
                                        VehicleBrand = reminder.Vehicle.Brand,
                                        VehicleModel = reminder.Vehicle.Model,
                                        ServiceName = reminder.Service?.ServiceName,
                                        CustomerName = $"{reminder.Vehicle.Customer.FirstName} {reminder.Vehicle.Customer.LastName}",
                                        CreatedAt = DateTime.UtcNow,
                                        SentAt = DateTime.UtcNow,
                                        IsRead = false
                                    };

                                    context.Notifications.Add(notification);
                                }
                            }
                        }

                        // Also create some general notifications for demonstration
                        var testCustomer = await context.Customers.FirstOrDefaultAsync();
                        if (testCustomer != null)
                        {
                            // Welcome notification
                            context.Notifications.Add(new Notification
                            {
                                CustomerId = testCustomer.CustomerId,
                                Title = "Welcome to Vehicle Service System",
                                Message = "Welcome! Your account has been set up successfully. You'll receive notifications about your vehicle service requirements here.",
                                Type = "general",
                                Priority = "Low",
                                PriorityColor = "#10B981",
                                CustomerName = $"{testCustomer.FirstName} {testCustomer.LastName}",
                                CreatedAt = DateTime.UtcNow.AddDays(-1),
                                SentAt = DateTime.UtcNow.AddDays(-1),
                                IsRead = true,
                                ReadAt = DateTime.UtcNow.AddHours(-2)
                            });

                            // System update notification
                            context.Notifications.Add(new Notification
                            {
                                CustomerId = testCustomer.CustomerId,
                                Title = "New Feature: Automatic Service Reminders",
                                Message = "We've added automatic service reminders to help you stay on top of your vehicle maintenance. You'll receive notifications when services are due.",
                                Type = "general",
                                Priority = "Medium",
                                PriorityColor = "#3B82F6",
                                CustomerName = $"{testCustomer.FirstName} {testCustomer.LastName}",
                                CreatedAt = DateTime.UtcNow.AddHours(-6),
                                SentAt = DateTime.UtcNow.AddHours(-6),
                                IsRead = false
                            });
                        }

                        await context.SaveChangesAsync();
                        Console.WriteLine($"Sample notifications created from service reminders");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error seeding notifications: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Helper method to create a user with a specific role
        /// </summary>
        /// <param name="context">Database context</param>
        /// <param name="firstName">User's first name</param>
        /// <param name="lastName">User's last name</param>
        /// <param name="email">User's email</param>
        /// <param name="password">User's password (will be hashed)</param>
        /// <param name="userRoleId">The ID of the user role</param>
        /// <returns>The created user</returns>
        public static User CreateUserWithRole(ApplicationDbContext context, string firstName, string lastName,
            string email, string password, int userRoleId)
        {
            var user = new User
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                UserRoleId = userRoleId,
                Password = BCrypt.Net.BCrypt.HashPassword(password)
            };

            context.Users.Add(user);
            return user;
        }

        /// <summary>
        /// Helper method to get a user role by name
        /// </summary>
        /// <param name="context">Database context</param>
        /// <param name="roleName">Name of the role</param>
        /// <returns>UserRole if found, null otherwise</returns>
        public static async Task<UserRole?> GetUserRoleByNameAsync(ApplicationDbContext context, string roleName)
        {
            return await context.UserRoles.FirstOrDefaultAsync(r => r.UserRoleName == roleName);
        }

        /// <summary>
        /// Helper method to get users by role
        /// </summary>
        /// <param name="context">Database context</param>
        /// <param name="userRoleId">The user role ID</param>
        /// <returns>List of users with the specified role</returns>
        public static async Task<List<User>> GetUsersByRoleAsync(ApplicationDbContext context, int userRoleId)
        {
            return await context.Users
                .Include(u => u.UserRole)
                .Where(u => u.UserRoleId == userRoleId)
                .ToListAsync();
        }
    }
}