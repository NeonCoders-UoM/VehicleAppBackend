# User Roles Implementation Guide

## Overview

This guide explains how user roles are implemented and managed in the Vehicle App Backend system using `UserRoleId`.

## User Role Constants

The system defines the following user roles with explicit IDs for consistency:

```csharp
public const int SUPER_ADMIN_ROLE_ID = 1;        // SuperAdmin
public const int ADMIN_ROLE_ID = 2;              // Admin
public const int SERVICE_CENTER_ADMIN_ROLE_ID = 3; // ServiceCenterAdmin
public const int CASHIER_ROLE_ID = 4;            // Cashier
public const int DATA_OPERATOR_ROLE_ID = 5;      // DataOperator
```

## Role Descriptions

### 1. SuperAdmin (ID: 1)

- **Highest level access**
- Can manage all system settings and users
- Default credentials: `superadmin@example.com` / `SuperAdmin@123`

### 2. Admin (ID: 2)

- **System administration access**
- Can manage users and system configurations
- Default credentials: `admin@example.com` / `Admin@123`

### 3. ServiceCenterAdmin (ID: 3)

- **Service center management access**
- Can manage service center operations and staff
- Default credentials: `serviceadmin@example.com` / `ServiceAdmin@123`

### 4. Cashier (ID: 4)

- **Transaction handling access**
- Can process payments and transactions
- Default credentials: `cashier@example.com` / `Cashier@123`

### 5. DataOperator (ID: 5)

- **Data entry and basic operations**
- Can input and modify data within assigned scope
- Default credentials: `dataoperator@example.com` / `DataOp@123`

## Usage Examples

### Creating a User with a Specific Role

```csharp
// Using the helper method
var newUser = SeedData.CreateUserWithRole(context, "John", "Doe", "john@example.com",
    "password123", SeedData.SERVICE_CENTER_ADMIN_ROLE_ID);

// Manual creation
var user = new User
{
    FirstName = "Jane",
    LastName = "Smith",
    Email = "jane@example.com",
    UserRoleId = SeedData.CASHIER_ROLE_ID,
    Password = BCrypt.Net.BCrypt.HashPassword("password123")
};
```

### Getting Users by Role

```csharp
// Get all service center admins
var serviceCenterAdmins = await SeedData.GetUsersByRoleAsync(context, SeedData.SERVICE_CENTER_ADMIN_ROLE_ID);

// Get all cashiers
var cashiers = await SeedData.GetUsersByRoleAsync(context, SeedData.CASHIER_ROLE_ID);
```

### Getting Role by Name

```csharp
var adminRole = await SeedData.GetUserRoleByNameAsync(context, "Admin");
if (adminRole != null)
{
    var newAdmin = SeedData.CreateUserWithRole(context, "New", "Admin",
        "newadmin@example.com", "password123", adminRole.UserRoleId);
}
```

## Database Seeding

The `SeedData.SeedAsync()` method automatically:

1. Creates all user roles with explicit IDs
2. Creates sample users for each role
3. Ensures referential integrity between users and roles

## Best Practices

### 1. Use Constants

Always use the predefined constants instead of hardcoding role IDs:

```csharp
// ✅ Good
user.UserRoleId = SeedData.SUPER_ADMIN_ROLE_ID;

// ❌ Bad
user.UserRoleId = 1;
```

### 2. Validate Role Existence

Always check if a role exists before assigning it:

```csharp
var role = await SeedData.GetUserRoleByNameAsync(context, "Admin");
if (role != null)
{
    user.UserRoleId = role.UserRoleId;
}
else
{
    throw new InvalidOperationException("Admin role not found");
}
```

### 3. Use Helper Methods

Utilize the provided helper methods for consistent user creation:

```csharp
var user = SeedData.CreateUserWithRole(context, firstName, lastName, email, password, roleId);
```

## Authorization Implementation

When implementing authorization in controllers, use the role constants:

```csharp
[Authorize(Roles = "SuperAdmin,Admin")]
public async Task<IActionResult> AdminOnly()
{
    // Only SuperAdmin and Admin can access
}

// In code, check roles like this:
if (user.UserRoleId == SeedData.SUPER_ADMIN_ROLE_ID ||
    user.UserRoleId == SeedData.ADMIN_ROLE_ID)
{
    // Allow access
}
```

## Migration Considerations

If you need to add new roles:

1. Add a new constant in `SeedData.cs`
2. Update the seeding logic to include the new role
3. Create a database migration to add the new role
4. Update any authorization logic as needed

## Security Notes

- All passwords are hashed using BCrypt
- Role IDs are explicitly defined to prevent insertion order dependencies
- Default users are created for testing purposes only
- Change default passwords in production environments
