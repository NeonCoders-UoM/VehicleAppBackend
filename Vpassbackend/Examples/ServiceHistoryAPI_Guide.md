# Vehicle Service History API Guide

## Endpoints

### 1. Get All Service History for a Vehicle

- **URL**: `/api/Vehicles/{vehicleId}/ServiceHistory`
- **Method**: `GET`
- **Description**: Retrieves all service history records for a specific vehicle
- **Example Response**:

```json
[
  {
    "serviceHistoryId": 1,
    "vehicleId": 1,
    "serviceType": "Oil Change",
    "description": "Regular oil change and filter replacement",
    "cost": 45.99,
    "serviceCenterId": 1,
    "servicedByUserId": 1,
    "serviceCenterName": "City Center Auto Services",
    "servicedByUserName": "John Smith",
    "serviceDate": "2025-07-09T10:30:00",
    "mileage": 25000,
    "isVerified": true,
    "externalServiceCenterName": null,
    "receiptDocumentPath": null
  },
  {
    "serviceHistoryId": 2,
    "vehicleId": 1,
    "serviceType": "Oil Change",
    "description": "Oil change at external service provider",
    "cost": 39.99,
    "serviceCenterId": null,
    "servicedByUserId": null,
    "serviceCenterName": null,
    "servicedByUserName": null,
    "serviceDate": "2025-07-01T14:00:00",
    "mileage": 24500,
    "isVerified": false,
    "externalServiceCenterName": "QuickLube Auto Service",
    "receiptDocumentPath": null
  }
]
```

### 2. Get Specific Service History Record

- **URL**: `/api/Vehicles/{vehicleId}/ServiceHistory/{serviceHistoryId}`
- **Method**: `GET`
- **Description**: Retrieves a specific service history record for a vehicle
- **Example Response**:

```json
{
  "serviceHistoryId": 1,
  "vehicleId": 1,
  "serviceType": "Oil Change",
  "description": "Regular oil change and filter replacement",
  "cost": 45.99,
  "serviceCenterId": 1,
  "servicedByUserId": 1,
  "serviceCenterName": "City Center Auto Services",
  "servicedByUserName": "John Smith",
  "serviceDate": "2025-07-09T10:30:00",
  "mileage": 25000,
  "isVerified": true,
  "externalServiceCenterName": null,
  "receiptDocumentPath": null
}
```

### 3. Add New Service History

- **URL**: `/api/Vehicles/{vehicleId}/ServiceHistory`
- **Method**: `POST`
- **Description**: Adds a new service history record for a vehicle
- **Example Request Body** (Verified Service):

```json
{
  "serviceType": "Oil Change",
  "description": "Regular oil change and filter replacement",
  "cost": 45.99,
  "serviceCenterId": 1,
  "servicedByUserId": 1,
  "serviceDate": "2025-07-09T10:30:00",
  "mileage": 25000
}
```

- **Example Request Body** (Unverified Service):

```json
{
  "serviceType": "Oil Change",
  "description": "Oil change at external service provider",
  "cost": 39.99,
  "serviceDate": "2025-07-01T14:00:00",
  "mileage": 24500,
  "externalServiceCenterName": "QuickLube Auto Service"
}
```

### 4. Update Service History

- **URL**: `/api/Vehicles/{vehicleId}/ServiceHistory/{serviceHistoryId}`
- **Method**: `PUT`
- **Description**: Updates an existing service history record
- **Example Request Body**:

```json
{
  "serviceHistoryId": 1,
  "serviceType": "Oil Change",
  "description": "Regular oil change and filter replacement with synthetic oil",
  "cost": 49.99,
  "serviceCenterId": 1,
  "servicedByUserId": 1,
  "serviceDate": "2025-07-09T10:30:00",
  "mileage": 25000
}
```

### 5. Delete Service History

- **URL**: `/api/Vehicles/{vehicleId}/ServiceHistory/{serviceHistoryId}`
- **Method**: `DELETE`
- **Description**: Deletes a service history record

## Verified vs Unverified Services

- **Verified Services**: Services performed at partnered service centers. These are automatically marked as verified when a valid `serviceCenterId` is provided.
- **Unverified Services**: Services performed at non-partnered service centers. These are identified by the absence of a `serviceCenterId` and can include an `externalServiceCenterName`.

## Service Types

The service type is now a free-text field that can contain any service name (like "Oil Change", "Brake Replacement", etc.) instead of referencing a service ID. This gives more flexibility for recording services that might not be in the standard service catalog.

## Service Provider

You can optionally specify who performed the service using the `servicedByUserId` field. This would typically be a technician or service center employee who is registered in the system.

## Receipt Documents

You can upload receipt documents by including a Base64-encoded string in the `receiptDocument` field of request bodies.
