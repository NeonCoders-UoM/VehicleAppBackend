# Package System API Guide

## Overview

The package system allows service centers to offer services with different discount packages (Platinum, Gold, Silver) and set custom pricing for each service.

## Default Packages

- **Platinum**: 25% discount
- **Gold**: 15% discount
- **Silver**: 10% discount

## Package Management APIs

### Get All Packages

```
GET /api/Packages
Authorization: Bearer {token}
```

### Get Active Packages Only

```
GET /api/Packages/Active
Authorization: Bearer {token}
```

### Get Specific Package

```
GET /api/Packages/{id}
Authorization: Bearer {token}
```

### Create New Package

```
POST /api/Packages
Authorization: Bearer {token} (SuperAdmin, Admin)
Content-Type: application/json

{
    "packageName": "Premium",
    "percentage": 30.00,
    "description": "Premium package with 30% discount",
    "isActive": true
}
```

### Update Package

```
PUT /api/Packages/{id}
Authorization: Bearer {token} (SuperAdmin, Admin)
Content-Type: application/json

{
    "packageName": "Updated Premium",
    "percentage": 35.00,
    "description": "Updated premium package",
    "isActive": true
}
```

### Delete Package

```
DELETE /api/Packages/{id}
Authorization: Bearer {token} (SuperAdmin, Admin)
```

## Service Center Service Management with Packages

### Add Service to Service Center with Package

```
POST /api/ServiceCenterServices
Authorization: Bearer {token} (SuperAdmin, Admin, ServiceCenterAdmin)
Content-Type: application/json

{
    "station_id": 1,
    "serviceId": 1,
    "packageId": 1,  // Optional: Package ID (Platinum = 1, Gold = 2, Silver = 3)
    "customPrice": 150.00,  // Optional: Custom price for this service center
    "isAvailable": true,
    "notes": "Premium service with platinum package"
}
```

### Update Service Center Service Package

```
PUT /api/ServiceCenterServices/{id}
Authorization: Bearer {token} (SuperAdmin, Admin, ServiceCenterAdmin)
Content-Type: application/json

{
    "packageId": 2,  // Change to Gold package
    "customPrice": 140.00,
    "isAvailable": true,
    "notes": "Updated to gold package"
}
```

### Get Service Center Services (includes package info)

```
GET /api/ServiceCenterServices
GET /api/ServiceCenterServices/ByServiceCenter/{stationId}
GET /api/ServiceCenterServices/ByService/{serviceId}
Authorization: Bearer {token}
```

## Response Format

Service center services now include package information:

```json
{
  "serviceCenterServiceId": 1,
  "station_id": 1,
  "serviceId": 1,
  "packageId": 1,
  "customPrice": 150.0,
  "isAvailable": true,
  "notes": "Premium service",
  "serviceName": "Oil Change",
  "serviceDescription": "Full synthetic oil change",
  "basePrice": 100.0,
  "loyaltyPoints": 10,
  "category": "Maintenance",
  "stationName": "ABC Service Center",
  "packageName": "Platinum",
  "packagePercentage": 25.0,
  "packageDescription": "Premium package with 25% discount on services"
}
```

## Business Logic

1. **Package Selection**: Service centers can choose a package for each service they offer
2. **Custom Pricing**: Service centers can set custom prices regardless of package
3. **Package Validation**: Only active packages can be assigned to services
4. **Package Deletion**: Packages cannot be deleted if they are currently in use

## Setup Instructions

1. Run the migration: `dotnet ef database update`
2. Seed default packages: Execute `add_default_packages.sql`
3. Or use the batch file: `setup_package_system.bat`

## Notes

- Package percentages represent discount percentages
- Custom prices override base service prices
- Service centers can offer the same service with different packages
- Package assignment is optional - services can be offered without packages
