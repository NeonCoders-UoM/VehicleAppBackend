# Authentication Troubleshooting Guide

## Current Issue: 401 Unauthorized - "Invalid credentials"

You're trying to login with Service Center Admin credentials but getting "Invalid credentials" error.

## Step-by-Step Troubleshooting

### 1. Verify API is Running

Check if your backend API is accessible:

```powershell
# Test on default ports
Invoke-RestMethod -Uri "https://localhost:7000/api/auth/user-roles" -Method Get -SkipCertificateCheck
# or
Invoke-RestMethod -Uri "https://localhost:5001/api/auth/user-roles" -Method Get -SkipCertificateCheck
```

**Expected Result**: List of user roles (1-5)
**If this fails**: Backend is not running or wrong port

### 2. Check Seeded Users

Use the debug endpoint to see what users exist:

```powershell
Invoke-RestMethod -Uri "https://localhost:7000/api/auth/debug-users" -Method Get -SkipCertificateCheck
```

**Expected Result**: List showing users including serviceadmin@example.com
**If user missing**: Database not seeded properly

### 3. Test Known Working Credentials

Try SuperAdmin first (always created):

```powershell
$data = @{
    email = "superadmin@example.com"
    password = "SuperAdmin@123"
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:7000/api/auth/login" -Method Post -Body $data -ContentType "application/json" -SkipCertificateCheck
```

**If this works**: Authentication system is working, issue is with Service Center Admin user
**If this fails**: Broader authentication problem

### 4. Test Service Center Admin Credentials

```powershell
$data = @{
    email = "serviceadmin@example.com"
    password = "ServiceAdmin@123"
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:7000/api/auth/login" -Method Post -Body $data -ContentType "application/json" -SkipCertificateCheck
```

### 5. Test Password Verification

Use the debug endpoint to check password:

```powershell
$data = @{
    email = "serviceadmin@example.com"
    password = "ServiceAdmin@123"
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:7000/api/auth/test-password" -Method Post -Body $data -ContentType "application/json" -SkipCertificateCheck
```

## All Seeded User Credentials

Based on your SeedData.cs, these users should exist:

| Role               | Email                    | Password         | Role ID |
| ------------------ | ------------------------ | ---------------- | ------- |
| SuperAdmin         | superadmin@example.com   | SuperAdmin@123   | 1       |
| Admin              | admin@example.com        | Admin@123        | 2       |
| ServiceCenterAdmin | serviceadmin@example.com | ServiceAdmin@123 | 3       |
| Cashier            | cashier@example.com      | Cashier@123      | 4       |
| DataOperator       | dataoperator@example.com | DataOp@123       | 5       |

## Common Issues & Solutions

### Issue 1: Database Not Seeded

**Symptoms**: debug-users returns empty list or missing users
**Solution**:

1. Stop the backend
2. Delete database file (if using SQLite)
3. Restart backend to trigger seeding
4. Check console logs for seeding messages

### Issue 2: Wrong API Port

**Symptoms**: Connection refused or timeout
**Solution**:

1. Check launchSettings.json for correct ports
2. Try both 7000 and 5001
3. Verify HTTPS vs HTTP

### Issue 3: Password Hash Issue

**Symptoms**: User exists but password doesn't match
**Solution**:

1. Check test-password endpoint results
2. Verify BCrypt is working correctly
3. Check if password is null in database

### Issue 4: Role Not Created

**Symptoms**: User exists but UserRoleId is null/invalid
**Solution**:

1. Check that UserRoles table has records
2. Verify ServiceCenterAdmin role exists with ID 3
3. Check foreign key constraints

## Quick Fixes

### Fix 1: Reset Database

```bash
# If using Entity Framework
dotnet ef database drop
dotnet ef database update
# Then restart the application
```

### Fix 2: Manual User Creation

If seeding fails, use the register endpoint:

```powershell
$data = @{
    firstName = "Service Center"
    lastName = "Manager"
    email = "serviceadmin@example.com"
    password = "ServiceAdmin@123"
    userRoleId = 3
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:7000/api/auth/register" -Method Post -Body $data -ContentType "application/json" -SkipCertificateCheck
```

### Fix 3: Check Backend Console

Look for these log messages when backend starts:

- "SuperAdmin user created with email: superadmin@example.com"
- "Sample users created for all user roles."

## Frontend Integration

Once authentication works, your frontend should:

1. **Use the login response** (recommended):

```typescript
const response = await fetch("/api/auth/login", {
  /* ... */
});
const data = await response.json();
const userRoleId = data.userRoleId; // Use this directly
```

2. **Handle different role IDs**:

```typescript
switch (userRoleId) {
  case 1: // SuperAdmin
  case 2: // Admin
  case 3: // ServiceCenterAdmin
  case 4: // Cashier
  case 5: // DataOperator
}
```

## Next Steps

1. ✅ Run the quick_login_test.ps1 script
2. ✅ Verify which credentials work
3. ✅ Check database seeding if none work
4. ✅ Test specific Service Center Admin login
5. ✅ Update frontend to use working credentials
