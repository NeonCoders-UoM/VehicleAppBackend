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
        }
    }
}
