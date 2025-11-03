# Notification System API Documentation

## Overview

The Notification System provides a complete backend solution for managing customer notifications related to vehicle service reminders, appointments, and other events. This system is designed to work with your Flutter frontend to provide real-time notifications to users.

## Features

### Core Functionality

- ✅ Create notifications manually or automatically from service reminders
- ✅ Mark notifications as read/unread
- ✅ Delete individual or all notifications
- ✅ Filter notifications by customer and read status
- ✅ Get notification counts (total and unread)
- ✅ Automatic priority color assignment based on urgency
- ✅ Background service for automatic notification generation

### Notification Types

- `service_reminder` - Generated from service reminders
- `appointment` - Related to scheduled appointments
- `general` - General announcements or updates

### Priority Levels

- `Critical` (#DC2626 - Red) - Overdue services, urgent issues
- `High` (#EA580C - Orange) - Due soon services, important updates
- `Medium` (#3B82F6 - Blue) - Standard reminders
- `Low` (#10B981 - Green) - Optional notifications

## API Endpoints

### Get Customer Notifications

#### Get All Notifications for a Customer

```http
GET /api/Notifications/Customer/{customerId}
```

**Response:**

```json
[
  {
    "notificationId": 1,
    "customerId": 1,
    "title": "Oil Change Reminder",
    "message": "Your vehicle is due for an oil change service...",
    "type": "service_reminder",
    "priority": "High",
    "priorityColor": "#EA580C",
    "isRead": false,
    "readAt": null,
    "sentAt": "2025-07-16T10:30:00Z",
    "scheduledFor": null,
    "createdAt": "2025-07-16T10:30:00Z",
    "updatedAt": null,
    "serviceReminderId": 1,
    "vehicleId": 1,
    "vehicleRegistrationNumber": "ABC-1234",
    "vehicleBrand": "Toyota",
    "vehicleModel": "Corolla",
    "serviceName": "Oil Change",
    "customerName": "John Doe"
  }
]
```

#### Get Unread Notifications Only

```http
GET /api/Notifications/Customer/{customerId}/Unread
```

#### Get Notification Counts

```http
GET /api/Notifications/Customer/{customerId}/Count
```

**Response:**

```json
{
  "totalCount": 5,
  "unreadCount": 3
}
```

### Create Notifications

#### Create a New Notification

```http
POST /api/Notifications
Content-Type: application/json

{
  "customerId": 1,
  "title": "Service Reminder",
  "message": "Your vehicle needs attention",
  "type": "service_reminder",
  "priority": "High",
  "serviceReminderId": 1,
  "vehicleId": 1,
  "vehicleRegistrationNumber": "ABC-1234",
  "vehicleBrand": "Toyota",
  "vehicleModel": "Corolla",
  "serviceName": "Oil Change",
  "customerName": "John Doe"
}
```

### Update Notifications

#### Mark Single Notification as Read

```http
PUT /api/Notifications/{notificationId}/MarkAsRead
```

#### Mark All Customer Notifications as Read

```http
PUT /api/Notifications/Customer/{customerId}/MarkAllAsRead
```

#### Update Notification Details

```http
PUT /api/Notifications/{notificationId}
Content-Type: application/json

{
  "title": "Updated Title",
  "priority": "Critical",
  "isRead": true
}
```

### Delete Notifications

#### Delete Single Notification

```http
DELETE /api/Notifications/{notificationId}
```

#### Delete All Customer Notifications

```http
DELETE /api/Notifications/Customer/{customerId}/DeleteAll
```

### Automatic Generation

#### Generate Notifications from Service Reminders

```http
POST /api/Notifications/GenerateFromServiceReminders
```

This endpoint scans all active service reminders and creates notifications for services that are due or overdue.

## Background Service

The `NotificationBackgroundService` automatically runs every 6 hours to:

1. Check all active service reminders
2. Generate notifications for services due within the specified notification period
3. Avoid creating duplicate notifications for the same reminder

## Database Schema

### Notification Table Fields

| Field                     | Type         | Description                   |
| ------------------------- | ------------ | ----------------------------- |
| NotificationId            | int          | Primary key                   |
| CustomerId                | int          | Foreign key to Customer       |
| Title                     | string(200)  | Notification title            |
| Message                   | string(1000) | Notification message          |
| Type                      | string(50)   | Notification type             |
| Priority                  | string(20)   | Priority level                |
| PriorityColor             | string(7)    | Hex color code                |
| IsRead                    | bool         | Read status                   |
| ReadAt                    | DateTime?    | When marked as read           |
| SentAt                    | DateTime?    | When notification was sent    |
| ScheduledFor              | DateTime?    | When to send (future feature) |
| CreatedAt                 | DateTime     | Creation timestamp            |
| UpdatedAt                 | DateTime?    | Last update timestamp         |
| ServiceReminderId         | int?         | Related service reminder      |
| VehicleId                 | int?         | Related vehicle               |
| AppointmentId             | int?         | Related appointment           |
| VehicleRegistrationNumber | string(20)   | Vehicle reg number            |
| VehicleBrand              | string(100)  | Vehicle brand                 |
| VehicleModel              | string(100)  | Vehicle model                 |
| ServiceName               | string(200)  | Service name                  |
| CustomerName              | string(150)  | Customer name                 |

## Frontend Integration

### Flutter Model Mapping

The backend DTOs map directly to your Flutter `NotificationModel`:

```dart
class NotificationModel {
  final int? notificationId;
  final int customerId;
  final String title;
  final String message;
  final String type;
  final String priority;
  final String priorityColor;
  final bool isRead;
  final DateTime? readAt;
  final DateTime? sentAt;
  final DateTime? scheduledFor;
  final DateTime createdAt;
  final DateTime? updatedAt;
  // ... other fields
}
```

### API Integration Examples

Based on your frontend code, here are the main API calls:

#### Get Notifications with Refresh

```dart
Future<List<NotificationModel>> getNotificationsWithRefresh(int customerId) async {
  final response = await http.get(
    Uri.parse('$baseUrl/api/Notifications/Customer/$customerId'),
    headers: {'Content-Type': 'application/json'},
  );

  if (response.statusCode == 200) {
    final List<dynamic> data = jsonDecode(response.body);
    return data.map((json) => NotificationModel.fromJson(json)).toList();
  }
  throw Exception('Failed to load notifications');
}
```

#### Mark as Read

```dart
Future<void> markNotificationAsRead(int notificationId) async {
  final response = await http.put(
    Uri.parse('$baseUrl/api/Notifications/$notificationId/MarkAsRead'),
    headers: {'Content-Type': 'application/json'},
  );

  if (response.statusCode != 204) {
    throw Exception('Failed to mark notification as read');
  }
}
```

#### Delete Notification

```dart
Future<void> deleteNotification(int notificationId) async {
  final response = await http.delete(
    Uri.parse('$baseUrl/api/Notifications/$notificationId'),
    headers: {'Content-Type': 'application/json'},
  );

  if (response.statusCode != 204) {
    throw Exception('Failed to delete notification');
  }
}
```

## Setup Instructions

### 1. Database Migration

Run the migration to create the notifications table:

```bash
# Add the migration
./add_notifications_migration.bat

# Update the database
dotnet ef database update
```

### 2. Start the Application

The notification background service will start automatically when you run the application.

### 3. Test the API

Use the provided `notifications.http` file to test all endpoints:

```bash
# Test basic functionality
GET https://localhost:7038/api/Notifications/Customer/1

# Create test notifications
POST https://localhost:7038/api/Notifications
```

## Error Handling

All endpoints include comprehensive error handling:

- **404 Not Found** - Customer or notification not found
- **500 Internal Server Error** - Database or server errors
- **400 Bad Request** - Invalid input data

Errors are logged for debugging and monitoring.

## Security Considerations

- All endpoints validate customer existence before operations
- Soft deletes for related entities (ServiceReminder, Vehicle, Appointment)
- No sensitive data in notification messages
- Proper foreign key constraints with appropriate cascade behavior

## Performance Notes

- Notifications are ordered by creation date (newest first)
- Background service runs every 6 hours to avoid excessive database load
- Duplicate notification prevention for service reminders
- Efficient database queries with proper indexing

## Future Enhancements

- Push notifications to mobile devices
- Email notification integration
- Scheduled notifications (using ScheduledFor field)
- Notification templates
- User notification preferences
- Rich content notifications (images, actions)

This notification system provides a solid foundation for your vehicle service application and can be easily extended as your requirements grow.
