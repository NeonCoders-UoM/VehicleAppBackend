# 🔐 Forgot Password Functionality Guide

## Overview
The forgot password functionality has been implemented for both **Customers** and **Users** (staff/admin). The system uses OTP (One-Time Password) sent via email to securely reset passwords.

## 🔧 Backend Implementation

### Database Changes
- Added `ForgotPasswordOtp` (string) and `ForgotPasswordOtpExpiry` (DateTime) fields to both `Customers` and `Users` tables
- Migration: `20250727141229_AddForgotPasswordFields`

### API Endpoints

#### 1. Request Password Reset
```http
POST /api/Auth/forgot-password
Content-Type: application/json

{
  "email": "user@example.com"
}
```

**Response:**
```json
{
  "message": "Password reset OTP sent to your email address."
}
```

#### 2. Reset Password
```http
POST /api/Auth/reset-password
Content-Type: application/json

{
  "email": "user@example.com",
  "otp": "123456",
  "newPassword": "NewPassword123",
  "confirmPassword": "NewPassword123"
}
```

**Response:**
```json
{
  "message": "Password reset successfully. You can now login with your new password."
}
```

#### 3. Resend OTP
```http
POST /api/Auth/resend-forgot-password-otp
Content-Type: application/json

"user@example.com"
```

**Response:**
```json
{
  "message": "New password reset OTP sent to your email address."
}
```

## 🔄 Workflow

### For Customers:
1. **Request Reset**: Customer enters email on forgot password page
2. **Email Verification**: System checks if email exists in Customers table
3. **OTP Generation**: 6-digit OTP generated and sent via email
4. **OTP Verification**: Customer enters OTP on reset page
5. **Password Reset**: Customer enters new password twice for confirmation
6. **Login**: Customer can now login with new password

### For Users (Staff/Admin):
1. **Request Reset**: User enters email on forgot password page
2. **Email Verification**: System checks if email exists in Users table
3. **OTP Generation**: 6-digit OTP generated and sent via email
4. **OTP Verification**: User enters OTP on reset page
5. **Password Reset**: User enters new password twice for confirmation
6. **Login**: User can now login with new password

## 📧 Email Templates

### Password Reset Request Email
```html
<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
    <h2 style='color: #333;'>Password Reset Request</h2>
    <p>Hello {FirstName},</p>
    <p>We received a request to reset your password for your {UserType} account. Use the following OTP to reset your password:</p>
    <div style='background-color: #f5f5f5; padding: 15px; text-align: center; margin: 20px 0;'>
        <h1 style='color: #007bff; margin: 0; font-size: 32px;'>{OTP}</h1>
    </div>
    <p><strong>This OTP will expire in 10 minutes.</strong></p>
    <p>If you didn't request this password reset, please ignore this email.</p>
    <p>Best regards,<br>Vehicle Service Team</p>
</div>
```

### New OTP Email
```html
<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
    <h2 style='color: #333;'>New Password Reset OTP</h2>
    <p>Hello {FirstName},</p>
    <p>Here's your new password reset OTP for your {UserType} account:</p>
    <div style='background-color: #f5f5f5; padding: 15px; text-align: center; margin: 20px 0;'>
        <h1 style='color: #007bff; margin: 0; font-size: 32px;'>{OTP}</h1>
    </div>
    <p><strong>This OTP will expire in 10 minutes.</strong></p>
    <p>If you didn't request this password reset, please ignore this email.</p>
    <p>Best regards,<br>Vehicle Service Team</p>
</div>
```

## 🔒 Security Features

1. **OTP Expiration**: OTPs expire after 10 minutes
2. **Password Validation**: Minimum 6 characters required
3. **Password Confirmation**: Must enter password twice to prevent typos
4. **Email Verification**: System checks if email exists before sending OTP
5. **Secure Hashing**: Passwords are hashed using BCrypt
6. **OTP Cleanup**: OTP fields are cleared after successful password reset

## 🚨 Error Handling

### Common Error Responses:

**Email Not Found:**
```json
{
  "message": "No account found with this email address."
}
```

**Invalid OTP:**
```json
{
  "message": "Invalid OTP."
}
```

**Expired OTP:**
```json
{
  "message": "OTP has expired."
}
```

**Password Validation:**
```json
{
  "message": "Password must be at least 6 characters long"
}
```

**Password Mismatch:**
```json
{
  "message": "Passwords do not match"
}
```

## 🎯 Frontend Integration

### React/JavaScript Example:
```javascript
// Request password reset
const requestPasswordReset = async (email) => {
  try {
    const response = await fetch('/api/Auth/forgot-password', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ email })
    });
    const data = await response.json();
    return data;
  } catch (error) {
    throw error;
  }
};

// Reset password
const resetPassword = async (email, otp, newPassword, confirmPassword) => {
  try {
    const response = await fetch('/api/Auth/reset-password', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        email,
        otp,
        newPassword,
        confirmPassword
      })
    });
    const data = await response.json();
    return data;
  } catch (error) {
    throw error;
  }
};

// Resend OTP
const resendOtp = async (email) => {
  try {
    const response = await fetch('/api/Auth/resend-forgot-password-otp', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(email)
    });
    const data = await response.json();
    return data;
  } catch (error) {
    throw error;
  }
};
```

## 📱 User Experience Flow

1. **Forgot Password Page**: User enters email address
2. **Email Sent**: Confirmation message shown
3. **OTP Entry Page**: User enters 6-digit OTP from email
4. **New Password Page**: User enters new password twice
5. **Success Page**: Confirmation message and redirect to login
6. **Login**: User can now login with new password

## 🔧 Configuration

### Email Settings (appsettings.json):
```json
{
  "Smtp": {
    "Host": "smtp.gmail.com",
    "Port": "587",
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "FromEmail": "your-email@gmail.com"
  }
}
```

## ✅ Testing

### Test Cases:
1. **Valid Email**: Should send OTP
2. **Invalid Email**: Should return "No account found"
3. **Valid OTP**: Should reset password
4. **Invalid OTP**: Should return "Invalid OTP"
5. **Expired OTP**: Should return "OTP has expired"
6. **Password Mismatch**: Should return "Passwords do not match"
7. **Short Password**: Should return validation error
8. **Resend OTP**: Should generate new OTP

## 🚀 Deployment Notes

1. Ensure SMTP settings are configured correctly
2. Test email delivery in production environment
3. Monitor OTP expiration times
4. Consider rate limiting for OTP requests
5. Log password reset attempts for security

## 📞 Support

For any issues with the forgot password functionality:
1. Check SMTP configuration
2. Verify email templates
3. Monitor database for OTP fields
4. Test with both customer and user accounts 