# Loyalty Points API Guide

## Overview

The loyalty points system allows customers to earn points from completed service appointments. Points are calculated based on the service pricing and any applied package discounts.

## Features

1. **Get Customer Total Loyalty Points**: Retrieve the current total loyalty points a customer has earned
2. **Get Appointment Loyalty Points**: View loyalty points that can be earned from a specific appointment
3. **Automatic Points Addition**: Points are automatically added to customer accounts when appointments are completed

## API Endpoints

### 1. Get Customer Total Loyalty Points

**Endpoint**: `GET /api/Customers/{customerId}/loyalty-points`

**Description**: Retrieves the total loyalty points currently owned by a customer.

**Parameters**:

- `customerId` (int, required): The ID of the customer

**Response**:

```json
{
  "customerId": 1,
  "totalLoyaltyPoints": 150
}
```

**Example Request**:

```bash
GET /api/Customers/1/loyalty-points
```

**Example Response**:

```json
{
  "customerId": 1,
  "totalLoyaltyPoints": 150
}
```

### 2. Get Appointment Loyalty Points

**Endpoint**: `GET /api/Appointment/{appointmentId}/loyalty-points`

**Description**: Retrieves the loyalty points that can be earned from a specific appointment.

**Parameters**:

- `appointmentId` (int, required): The ID of the appointment

**Response**:

```json
{
  "appointmentId": 1,
  "customerId": 1,
  "vehicleId": 1,
  "stationId": 1,
  "loyaltyPoints": 25,
  "services": [
    {
      "serviceId": 1,
      "loyaltyPoints": 15
    },
    {
      "serviceId": 2,
      "loyaltyPoints": 10
    }
  ]
}
```

**Example Request**:

```bash
GET /api/Appointment/1/loyalty-points
```

**Example Response**:

```json
{
  "appointmentId": 1,
  "customerId": 1,
  "vehicleId": 1,
  "stationId": 1,
  "loyaltyPoints": 25,
  "services": [
    {
      "serviceId": 1,
      "loyaltyPoints": 15
    },
    {
      "serviceId": 2,
      "loyaltyPoints": 10
    }
  ]
}
```

## Loyalty Points Calculation

### Formula

Loyalty points are calculated using the following formula:

- **Base Points**: 1 point per 10 currency units of service price
- **With Package Discount**: Points are calculated based on the discounted price after package percentage is applied

### Example Calculations

1. **Service without Package**:

   - Service Price: $100
   - Loyalty Points: 100 ÷ 10 = 10 points

2. **Service with Platinum Package (25% discount)**:

   - Service Price: $100
   - Discounted Price: $100 - ($100 × 0.25) = $75
   - Loyalty Points: 75 ÷ 10 = 7 points

3. **Service with Gold Package (15% discount)**:
   - Service Price: $100
   - Discounted Price: $100 - ($100 × 0.15) = $85
   - Loyalty Points: 85 ÷ 10 = 8 points

## Automatic Points Addition

When an appointment is completed using the `POST /api/Appointment/{appointmentId}/complete` endpoint:

1. The system calculates the total loyalty points from all services in the appointment
2. Points are automatically added to the customer's total loyalty points
3. A notification is sent to the customer about the completed appointment
4. The appointment status is updated to "Completed"

## Error Handling

### Common Error Responses

**Customer Not Found**:

```json
{
  "message": "Customer not found."
}
```

**Appointment Not Found**:

```json
{
  "message": "Appointment not found."
}
```

**Server Error**:

```json
{
  "message": "Error retrieving loyalty points",
  "error": "Detailed error message"
}
```

## Usage Examples

### Frontend Integration

```javascript
// Get customer's total loyalty points
async function getCustomerLoyaltyPoints(customerId) {
  const response = await fetch(`/api/Customers/${customerId}/loyalty-points`);
  const data = await response.json();
  return data.totalLoyaltyPoints;
}

// Get loyalty points for an appointment
async function getAppointmentLoyaltyPoints(appointmentId) {
  const response = await fetch(
    `/api/Appointment/${appointmentId}/loyalty-points`
  );
  const data = await response.json();
  return data.loyaltyPoints;
}

// Complete appointment and earn points
async function completeAppointment(appointmentId) {
  const response = await fetch(`/api/Appointment/${appointmentId}/complete`, {
    method: "POST",
  });
  return response.ok;
}
```

### Mobile App Integration

```dart
// Flutter example
class LoyaltyPointsService {
  static Future<int> getCustomerLoyaltyPoints(int customerId) async {
    final response = await http.get(
      Uri.parse('$baseUrl/api/Customers/$customerId/loyalty-points'),
    );

    if (response.statusCode == 200) {
      final data = json.decode(response.body);
      return data['totalLoyaltyPoints'];
    }
    throw Exception('Failed to load loyalty points');
  }

  static Future<int> getAppointmentLoyaltyPoints(int appointmentId) async {
    final response = await http.get(
      Uri.parse('$baseUrl/api/Appointment/$appointmentId/loyalty-points'),
    );

    if (response.statusCode == 200) {
      final data = json.decode(response.body);
      return data['loyaltyPoints'];
    }
    throw Exception('Failed to load appointment loyalty points');
  }
}
```

## Database Schema

### Customer Table

```sql
CREATE TABLE Customers (
    CustomerId INT PRIMARY KEY IDENTITY(1,1),
    FirstName NVARCHAR(100),
    LastName NVARCHAR(100),
    Email NVARCHAR(150) NOT NULL,
    Address NVARCHAR(200),
    PhoneNumber NVARCHAR(20),
    Password NVARCHAR(MAX) NOT NULL,
    NIC NVARCHAR(20),
    LoyaltyPoints INT DEFAULT 0,  -- Total loyalty points
    -- ... other fields
);
```

### ServiceCenterService Table

```sql
CREATE TABLE ServiceCenterServices (
    ServiceCenterServiceId INT PRIMARY KEY IDENTITY(1,1),
    Station_id INT,
    ServiceId INT,
    PackageId INT NULL,
    CustomPrice DECIMAL(10,2) NULL,
    BasePrice DECIMAL(10,2) NULL,
    LoyaltyPoints INT NULL,  -- Loyalty points for this service
    IsAvailable BIT DEFAULT 1,
    Notes NVARCHAR(255),
    -- ... foreign keys
);
```

## Security Considerations

1. **Authentication**: All endpoints require proper authentication
2. **Authorization**: Customer endpoints should only be accessible by the customer themselves or authorized staff
3. **Data Validation**: All input parameters are validated before processing
4. **Error Handling**: Sensitive information is not exposed in error messages

## Best Practices

1. **Cache Loyalty Points**: Consider caching customer loyalty points for better performance
2. **Batch Updates**: For high-volume scenarios, consider batch processing for loyalty points updates
3. **Audit Trail**: Consider adding audit logs for loyalty points transactions
4. **Rate Limiting**: Implement rate limiting to prevent abuse
5. **Monitoring**: Monitor loyalty points transactions for unusual patterns

## Future Enhancements

1. **Loyalty Points Redemption**: Allow customers to redeem points for discounts
2. **Points Expiration**: Implement points expiration policies
3. **Tier System**: Implement customer tiers based on loyalty points
4. **Points History**: Track detailed history of points earned and spent
5. **Promotional Points**: Special bonus points for promotions or referrals
