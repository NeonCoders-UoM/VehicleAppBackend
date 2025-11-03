# User Role Management API Examples

This file demonstrates how to use the updated AuthController endpoints with UserRoleId.

## Available User Roles

Based on the SeedData constants:

- SuperAdmin: UserRoleId = 1
- Admin: UserRoleId = 2
- ServiceCenterAdmin: UserRoleId = 3
- Cashier: UserRoleId = 4
- DataOperator: UserRoleId = 5

## API Endpoints

### 1. Get Available User Roles

```http
GET /api/auth/user-roles
```

**Response:**

```json
[
  { "userRoleId": 1, "userRoleName": "SuperAdmin" },
  { "userRoleId": 2, "userRoleName": "Admin" },
  { "userRoleId": 3, "userRoleName": "ServiceCenterAdmin" },
  { "userRoleId": 4, "userRoleName": "Cashier" },
  { "userRoleId": 5, "userRoleName": "DataOperator" }
]
```

### 2. Register User with Specific Role

```http
POST /api/auth/register
Content-Type: application/json

{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "password": "SecurePassword123!",
  "userRoleId": 3
}
```

**Response:**

```json
{
  "message": "User registered successfully",
  "userId": 123,
  "email": "john.doe@example.com",
  "userRoleId": 3,
  "userRoleName": "ServiceCenterAdmin"
}
```

### 3. Login User

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "john.doe@example.com",
  "password": "SecurePassword123!"
}
```

**Response:**

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "userId": 123,
  "userRole": "ServiceCenterAdmin"
}
```

### 4. Get User Information

```http
GET /api/auth/user-info/123
```

**Response:**

```json
{
  "userId": 123,
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "userRoleId": 3,
  "userRoleName": "ServiceCenterAdmin"
}
```

### 5. Update User Role (Admin/SuperAdmin only)

```http
PUT /api/auth/update-user-role/123
Content-Type: application/json
Authorization: Bearer {admin_token}

{
  "userRoleId": 2
}
```

**Response:**

```json
{
  "message": "User role updated successfully",
  "userId": 123,
  "email": "john.doe@example.com",
  "previousRole": {
    "userRoleId": 3,
    "userRoleName": "ServiceCenterAdmin"
  },
  "newRole": {
    "userRoleId": 2,
    "userRoleName": "Admin"
  }
}
```

## PowerShell Test Script

```powershell
# Set base URL
$baseUrl = "https://localhost:7000/api/auth"

# 1. Get available roles
Write-Host "Getting available user roles..."
$roles = Invoke-RestMethod -Uri "$baseUrl/user-roles" -Method Get
$roles | ConvertTo-Json

# 2. Register a new Service Center Admin
Write-Host "`nRegistering new Service Center Admin..."
$registerData = @{
    firstName = "Alice"
    lastName = "Manager"
    email = "alice.manager@example.com"
    password = "ServiceAdmin123!"
    userRoleId = 3
} | ConvertTo-Json

$newUser = Invoke-RestMethod -Uri "$baseUrl/register" -Method Post -Body $registerData -ContentType "application/json"
$newUser | ConvertTo-Json

# 3. Login with the new user
Write-Host "`nLogging in with new user..."
$loginData = @{
    email = "alice.manager@example.com"
    password = "ServiceAdmin123!"
} | ConvertTo-Json

$loginResult = Invoke-RestMethod -Uri "$baseUrl/login" -Method Post -Body $loginData -ContentType "application/json"
$loginResult | ConvertTo-Json

# 4. Get user info
Write-Host "`nGetting user information..."
$userId = $newUser.userId
$userInfo = Invoke-RestMethod -Uri "$baseUrl/user-info/$userId" -Method Get
$userInfo | ConvertTo-Json

# 5. Update user role (requires admin token)
Write-Host "`nNote: To update user role, you need to login as SuperAdmin or Admin first"
```

## C# Code Examples

### Register User with Role

```csharp
var registerDto = new UserRegisterDto
{
    FirstName = "Jane",
    LastName = "Smith",
    Email = "jane.smith@example.com",
    Password = "SecurePassword123!",
    UserRoleId = SeedData.CASHIER_ROLE_ID // Using constant
};

var response = await httpClient.PostAsJsonAsync("/api/auth/register", registerDto);
```

### Check User Role in Controller

```csharp
[HttpGet("admin-only")]
[Authorize]
public async Task<IActionResult> AdminOnlyEndpoint()
{
    var userIdClaim = User.FindFirst("UserId")?.Value;
    if (int.TryParse(userIdClaim, out int userId))
    {
        var user = await _context.Users
            .Include(u => u.UserRole)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user?.UserRoleId == SeedData.SUPER_ADMIN_ROLE_ID ||
            user?.UserRoleId == SeedData.ADMIN_ROLE_ID)
        {
            // Allow access
            return Ok("Admin access granted");
        }
    }

    return Forbid("Admin access required");
}
```

## Logging Output Examples

When users are registered or roles are updated, the system logs:

```
User created: alice.manager@example.com with UserRoleId: 3 (ServiceCenterAdmin)
User role updated: jane.smith@example.com changed from UserRoleId 4 (Cashier) to UserRoleId 2 (Admin)
```

## Error Handling

### Invalid UserRoleId

```json
{
  "error": "Invalid UserRoleId: 99. Role does not exist."
}
```

### Email Already Registered

```json
{
  "error": "Email already registered."
}
```

### Unauthorized Role Update

```json
{
  "error": "Access denied. Admin privileges required."
}
```
