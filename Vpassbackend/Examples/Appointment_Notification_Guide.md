# Appointment Notification System

## Overview

The appointment system now automatically sends notifications to customers when appointments are created and completed. This ensures customers are always informed about their appointment status.

## Notification Triggers

### 1. Appointment Creation

When a customer successfully creates a new appointment, the system automatically sends a notification with:

- **Title**: "Appointment Update"
- **Message**: Confirmation message with appointment date and service center name
- **Type**: "appointment"
- **Priority**: "Medium"

### 2. Appointment Completion

When an admin marks an appointment as completed, the system sends a notification with:

- **Title**: "Appointment Update"
- **Message**: Completion confirmation message
- **Type**: "appointment"
- **Priority**: "Medium"

## Implementation Details

### Service Layer Changes

- **AppointmentService**: Added notification calls in `CreateAppointmentAsync()` and `CompleteAppointmentAsync()` methods
- **Error Handling**: Notification failures don't prevent appointment operations from succeeding
- **Logging**: Proper logging for notification errors using ILogger

### Dependencies

- **INotificationService**: Used for creating appointment notifications
- **ILogger**: Used for error logging when notifications fail

## API Endpoints

### Create Appointment (Triggers Notification)

```http
POST /api/Appointment
Content-Type: application/json

{
  "vehicleId": 1,
  "serviceIds": [1, 2],
  "station_id": 1,
  "customerId": 1,
  "appointmentDate": "2024-01-15T10:00:00Z",
  "description": "Regular maintenance service"
}
```

### Complete Appointment (Triggers Notification)

```http
POST /api/Appointment/{appointmentId}/complete
```

### View Notifications

```http
GET /api/Notifications/Customer/{customerId}
GET /api/Notifications/Customer/{customerId}/Unread
```

## Error Handling

The system is designed to be resilient:

1. **Appointment Creation**: If notification fails, the appointment is still created successfully
2. **Appointment Completion**: If notification fails, the appointment is still marked as completed
3. **Logging**: All notification errors are logged with appropriate warning levels
4. **Graceful Degradation**: The core appointment functionality continues to work even if notifications fail

## Duplicate Prevention

To prevent duplicate notifications from multiple requests (common with frontend retries or network issues):

1. **Time-based Deduplication**: The system checks for existing notifications within the last 5 minutes
2. **Appointment-specific**: For appointment notifications, duplicates are prevented based on appointment ID
3. **Content-based**: For general notifications, duplicates are prevented based on customer, type, title, and message
4. **Automatic Cleanup**: A cleanup endpoint is available to remove any existing duplicates

### Cleanup Endpoint

```http
POST /api/Notifications/CleanupDuplicates
```

This endpoint removes duplicate notifications created within the same minute.

## Testing

Use the provided `appointment-notifications.http` file to test the notification system:

1. Create an appointment and verify a notification is sent
2. Check the customer's notification list
3. Complete an appointment and verify another notification is sent
4. Test error scenarios by temporarily disabling the notification service

## Notification Message Examples

### Appointment Creation

```
"Your appointment has been successfully scheduled for Jan 15, 2024 at ABC Service Center. We look forward to serving you!"
```

### Appointment Completion

```
"Your service appointment has been completed. Thank you for choosing us!"
```

## Configuration

The notification system uses the existing notification infrastructure:

- Notifications are stored in the `Notifications` table
- They can be retrieved via the Notifications API
- Push notifications can be configured separately if needed
- Background services handle service reminder notifications

## Future Enhancements

Potential improvements to consider:

1. **Email Notifications**: Send email confirmations in addition to in-app notifications
2. **SMS Notifications**: Send text message reminders
3. **Reminder Notifications**: Send reminders before appointment date
4. **Customizable Messages**: Allow service centers to customize notification messages
5. **Notification Preferences**: Allow customers to choose notification types and frequency
