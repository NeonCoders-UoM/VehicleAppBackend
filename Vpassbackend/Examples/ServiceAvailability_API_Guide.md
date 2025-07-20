# Service Availability and Closure Schedule API Guide

## Overview
This guide covers the integration between closure schedules and service availability. When a service center is marked as closed on a specific day, all services at that service center become unavailable for that day.

## Endpoints

### 1. Toggle Service Availability
**Endpoint:** `PATCH /api/ServiceCenterServices/{id}/toggle-availability`

**Description:** Toggles the availability of a specific service at a service center.

**Authorization:** Requires SuperAdmin, Admin, or ServiceCenterAdmin role.

**Response:**
```json
{
  "serviceCenterServiceId": 1,
  "station_id": 1,
  "serviceId": 1,
  "packageId": null,
  "customPrice": 150.00,
  "serviceCenterBasePrice": 150.00,
  "serviceCenterLoyaltyPoints": 15,
  "isAvailable": false,
  "notes": null,
  "serviceName": "Oil Change",
  "serviceDescription": "Complete oil change service",
  "serviceBasePrice": 100.00,
  "category": "Maintenance",
  "stationName": "Downtown Service Center",
  "packageName": null,
  "packagePercentage": null,
  "packageDescription": null
}
```

### 2. Update Availability from Closure Schedule
**Endpoint:** `POST /api/ServiceCenterServices/update-availability-from-closure`

**Description:** Manually triggers service availability updates based on closure schedule changes.

**Authorization:** Requires SuperAdmin, Admin, or ServiceCenterAdmin role.

**Request Body:**
```json
{
  "serviceCenterId": 1,
  "weekNumber": 1,
  "day": "Monday"
}
```

**Response:**
```json
{
  "message": "Updated availability for 5 services",
  "isClosed": true,
  "serviceCenterId": 1,
  "weekNumber": 1,
  "day": "Monday"
}
```

### 3. Add Closure Schedule
**Endpoint:** `POST /api/ClosureSchedule`

**Description:** Adds a closure schedule and automatically updates service availability.

**Request Body:**
```json
{
  "serviceCenterId": 1,
  "weekNumber": 1,
  "day": "Monday"
}
```

**Response:**
```json
{
  "id": 1,
  "serviceCenterId": 1,
  "weekNumber": 1,
  "day": "Monday"
}
```

### 4. Update Closure Schedule
**Endpoint:** `PUT /api/ClosureSchedule/{id}`

**Description:** Updates a closure schedule and automatically updates service availability for both old and new schedules.

**Request Body:**
```json
{
  "serviceCenterId": 1,
  "weekNumber": 1,
  "day": "Tuesday"
}
```

### 5. Delete Closure Schedule
**Endpoint:** `DELETE /api/ClosureSchedule/{id}`

**Description:** Deletes a closure schedule and automatically restores service availability.

### 6. Get Services with Availability
**Endpoint:** `GET /api/Service/{serviceCenterId}?weekNumber=1&day=Monday`

**Description:** Gets all services for a service center with their availability status, considering closure schedules.

**Response:**
```json
[
  {
    "serviceId": 1,
    "serviceName": "Oil Change",
    "serviceCenterServiceId": 1,
    "isAvailable": false
  },
  {
    "serviceId": 2,
    "serviceName": "Tire Rotation",
    "serviceCenterServiceId": 2,
    "isAvailable": false
  }
]
```

### 7. Get Available Services
**Endpoint:** `GET /api/Service/{serviceCenterId}/available?weekNumber=1&day=Monday`

**Description:** Gets only the available services for a specific day, considering closure schedules.

**Response:**
```json
[]
```

## Integration Behavior

### Automatic Updates
When a closure schedule is added, updated, or deleted, the system automatically:
1. Updates the `IsAvailable` property of all `ServiceCenterService` records for that service center
2. Sets availability to `false` when the service center is closed
3. Sets availability to `true` when the service center is open (or closure is removed)

### Availability Hierarchy
The system checks availability in the following order:
1. **Closure Schedule:** If the service center is closed, all services are unavailable
2. **ServiceCenterService.IsAvailable:** Individual service availability at the service center level
3. **ServiceAvailability:** Specific availability settings for week/day combinations

### Example Scenarios

#### Scenario 1: Service Center Closed
- Add closure: `POST /api/ClosureSchedule` with Monday closure
- Result: All services become unavailable for Monday
- Get services: `GET /api/Service/1?weekNumber=1&day=Monday` returns all services with `isAvailable: false`

#### Scenario 2: Service Center Reopened
- Delete closure: `DELETE /api/ClosureSchedule/1`
- Result: All services become available again (unless individually set to unavailable)
- Get services: `GET /api/Service/1?weekNumber=1&day=Monday` returns services with their individual availability status

#### Scenario 3: Individual Service Toggle
- Toggle service: `PATCH /api/ServiceCenterServices/1/toggle-availability`
- Result: Only that specific service availability is toggled
- Other services remain unaffected

## Error Handling

### Common Errors
- **404 Not Found:** Service center service not found
- **400 Bad Request:** Duplicate closure entry
- **401 Unauthorized:** Missing or invalid authorization
- **403 Forbidden:** Insufficient permissions

### Validation
- Closure schedules cannot have duplicate entries for the same service center, week, and day
- Service center and service must exist before creating relationships
- All required fields must be provided

## Testing Examples

### Test Closure Schedule Creation
```bash
curl -X POST "https://localhost:7001/api/ClosureSchedule" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
    "serviceCenterId": 1,
    "weekNumber": 1,
    "day": "Monday"
  }'
```

### Test Service Availability Toggle
```bash
curl -X PATCH "https://localhost:7001/api/ServiceCenterServices/1/toggle-availability" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### Test Get Services with Availability
```bash
curl -X GET "https://localhost:7001/api/Service/1?weekNumber=1&day=Monday" \
  -H "Authorization: Bearer YOUR_TOKEN"
``` 