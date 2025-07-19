# Frontend Integration Guide for UserRoleId

This guide explains how to properly extract and use UserRoleId from the backend authentication system.

## Backend Changes Made

### 1. AuthService Updated

The JWT token now includes these claims:

```csharp
new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
new Claim(ClaimTypes.Email, user.Email),
new Claim(ClaimTypes.Role, roleName),
new Claim("UserRoleId", user.UserRoleId.ToString()), // ✅ Added UserRoleId
new Claim("UserId", user.UserId.ToString())          // ✅ Added UserId for convenience
```

### 2. Login Response Enhanced

The login endpoint now returns:

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "userId": 123,
  "userRole": "SuperAdmin",
  "userRoleId": 1 // ✅ Added UserRoleId to response
}
```

## Frontend Implementation Options

### Option 1: Use Login Response (Recommended)

Extract UserRoleId directly from the login response:

```typescript
// login-form.tsx
const handleLogin = async (email: string, password: string) => {
  try {
    const response = await fetch("/api/auth/login", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ email, password }),
    });

    const data = await response.json();

    if (response.ok) {
      // Store token
      localStorage.setItem("token", data.token);

      // Store user info including UserRoleId
      localStorage.setItem("userId", data.userId.toString());
      localStorage.setItem("userRole", data.userRole);
      localStorage.setItem("userRoleId", data.userRoleId.toString()); // ✅ Store UserRoleId

      // Use UserRoleId for dashboard redirect
      redirectToDashboard(data.userRoleId);
    }
  } catch (error) {
    console.error("Login failed:", error);
  }
};

const redirectToDashboard = (userRoleId: number) => {
  switch (userRoleId) {
    case 1: // SuperAdmin
      router.push("/superadmin-dashboard");
      break;
    case 2: // Admin
      router.push("/admin-dashboard");
      break;
    case 3: // ServiceCenterAdmin
      router.push("/servicecenter-dashboard");
      break;
    case 4: // Cashier
      router.push("/cashier-dashboard");
      break;
    case 5: // DataOperator
      router.push("/dataoperator-dashboard");
      break;
    default:
      throw new Error(`Invalid role ID: ${userRoleId}`);
  }
};
```

### Option 2: Decode JWT Token

Extract UserRoleId from JWT token claims:

```typescript
import jwt_decode from "jwt-decode";

interface JWTPayload {
  [key: string]: any;
  UserRoleId: string;
  UserId: string;
  role: string;
  email: string;
}

const decodeTokenAndGetRoleId = (token: string): number => {
  try {
    const decoded = jwt_decode<JWTPayload>(token);
    console.log("Decoded token:", decoded);

    const userRoleId = parseInt(decoded.UserRoleId);
    console.log("Decoded role ID:", userRoleId);

    if (isNaN(userRoleId)) {
      throw new Error("UserRoleId not found in token");
    }

    return userRoleId;
  } catch (error) {
    console.error("Token decode error:", error);
    throw error;
  }
};

// Usage in login handler
const handleLogin = async (email: string, password: string) => {
  const response = await fetch("/api/auth/login", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ email, password }),
  });

  const data = await response.json();

  if (response.ok) {
    localStorage.setItem("token", data.token);

    // Extract UserRoleId from token
    const userRoleId = decodeTokenAndGetRoleId(data.token);
    redirectToDashboard(userRoleId);
  }
};
```

## User Role Constants (for Frontend)

Create a constants file for role mapping:

```typescript
// constants/userRoles.ts
export const USER_ROLES = {
  SUPER_ADMIN: 1,
  ADMIN: 2,
  SERVICE_CENTER_ADMIN: 3,
  CASHIER: 4,
  DATA_OPERATOR: 5,
} as const;

export const ROLE_NAMES = {
  1: "SuperAdmin",
  2: "Admin",
  3: "ServiceCenterAdmin",
  4: "Cashier",
  5: "DataOperator",
} as const;

export type UserRoleId = (typeof USER_ROLES)[keyof typeof USER_ROLES];
```

## Updated Login Form Implementation

```typescript
// login-form.tsx
import { USER_ROLES, ROLE_NAMES } from "../constants/userRoles";

const handleLogin = async (email: string, password: string) => {
  try {
    const response = await fetch("https://localhost:7000/api/auth/login", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ email, password }),
    });

    const data = await response.json();

    if (response.ok) {
      console.log("Login successful:", data);

      // Store authentication data
      localStorage.setItem("token", data.token);
      localStorage.setItem("userId", data.userId.toString());
      localStorage.setItem("userRole", data.userRole);
      localStorage.setItem("userRoleId", data.userRoleId.toString());

      // Validate role ID
      const userRoleId = data.userRoleId;
      console.log("User Role ID:", userRoleId);

      if (!Object.values(USER_ROLES).includes(userRoleId)) {
        throw new Error(`Invalid role ID: ${userRoleId}`);
      }

      // Redirect based on role
      redirectToDashboard(userRoleId);
    } else {
      throw new Error(data.message || "Login failed");
    }
  } catch (error) {
    console.error("Login failed:", error);
    // Handle error appropriately
  }
};

const redirectToDashboard = (userRoleId: number) => {
  switch (userRoleId) {
    case USER_ROLES.SUPER_ADMIN:
      router.push("/superadmin-dashboard");
      break;
    case USER_ROLES.ADMIN:
      router.push("/admin-dashboard");
      break;
    case USER_ROLES.SERVICE_CENTER_ADMIN:
      router.push("/servicecenter-dashboard");
      break;
    case USER_ROLES.CASHIER:
      router.push("/cashier-dashboard");
      break;
    case USER_ROLES.DATA_OPERATOR:
      router.push("/dataoperator-dashboard");
      break;
    default:
      throw new Error(`Invalid role ID: ${userRoleId}`);
  }
};
```

## Testing the Fix

### 1. Test Login Response

```bash
curl -X POST https://localhost:7000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"superadmin@example.com","password":"SuperAdmin@123"}'
```

Expected response:

```json
{
  "token": "eyJ...",
  "userId": 1,
  "userRole": "SuperAdmin",
  "userRoleId": 1
}
```

### 2. Test JWT Token Content

```bash
curl -X POST https://localhost:7000/api/auth/debug-token \
  -H "Content-Type: application/json" \
  -d '"YOUR_JWT_TOKEN_HERE"'
```

Expected response should include:

```json
{
  "userRoleId": "1",
  "userId": "1",
  "role": "SuperAdmin"
}
```

## Common Issues & Solutions

### Issue 1: UserRoleId is undefined

**Solution**: Make sure you're accessing `data.userRoleId` from the login response.

### Issue 2: JWT decode fails

**Solution**: Use the login response userRoleId instead of decoding JWT.

### Issue 3: Role ID is string instead of number

**Solution**: Convert to number: `parseInt(data.userRoleId)`

### Issue 4: Invalid role ID error

**Solution**: Check that backend has seeded user roles properly.

## Debugging Steps

1. **Check Network Tab**: Verify login response includes `userRoleId`
2. **Console Log**: Log the entire login response to see all fields
3. **Test with PowerShell**: Use the provided `test_jwt_userrole.ps1` script
4. **Backend Logs**: Check console for user creation/login logs

## Summary

The backend now provides UserRoleId in two ways:

1. **Login Response** (recommended): `data.userRoleId`
2. **JWT Token Claim**: Decode token and extract `UserRoleId` claim

Use the login response approach for simplicity and reliability.
