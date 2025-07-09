# VehicleAppVpassbackend

## Appointment Booking System

The Vehicle App now includes a comprehensive appointment booking system for service centers. This system allows customers to:

1. Select an appointment date
2. Choose services for their vehicle
3. View nearby service centers based on their location
4. See estimated costs for each service center
5. Book appointments with a 10% advance payment

### Key Features:

- Location-based service center recommendations
- Service selection and cost estimation
- Appointment booking with advance payment
- Payment processing and tracking

### API Endpoints:

#### Get Nearby Service Centers

```
POST /api/Appointments/NearbyServiceCenters
```

Returns service centers near the customer's location that offer the requested services.

#### Book Appointment

```
POST /api/Appointments/Book
```

Books an appointment at the selected service center.

#### Process Payment

```
POST /api/Appointments/ProcessPayment
```

Processes the advance payment for an appointment.

#### Get Appointment Details

```
GET /api/Appointments/{id}
```

Retrieves the details of an appointment.

See the `appointment-booking.http` file for example API requests.
