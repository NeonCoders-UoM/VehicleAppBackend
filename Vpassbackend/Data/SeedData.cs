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
        }
    }
}
