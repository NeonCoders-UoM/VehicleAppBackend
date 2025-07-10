using Vpassbackend.Models;


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
                // Get the SuperAdmin role that was just created
                var superAdminRole = context.UserRoles.First(r => r.UserRoleName == "SuperAdmin");

                var superAdmin = new User
                {
                    FirstName = "Super",
                    LastName = "Admin",
                    Email = "superadmin@example.com",
                    UserRoleId = superAdminRole.UserRoleId
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
        }
    }
}
