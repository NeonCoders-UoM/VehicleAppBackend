# ServiceReminder API Changes - July 2025

## Important Update: ServiceId Removed

We've made a significant change to our API by removing the `serviceId` field from all ServiceReminder DTOs. This change affects how you interact with the API endpoints.

### Updated Model Structure

The `ServiceReminderDTO` no longer contains the `serviceId` field but still includes `serviceName`:

```json
{
  "serviceReminderId": 1,
  "vehicleId": 2,
  // serviceId removed
  "reminderDate": "2025-08-15T10:00:00Z",
  "intervalMonths": 6,
  "notifyBeforeDays": 14,
  "notes": "Regular maintenance",
  "isActive": true,
  "createdAt": "2025-06-10T08:30:00Z",
  "updatedAt": "2025-07-12T14:22:00Z",
  "serviceName": "Oil Change",
  "vehicleRegistrationNumber": "ABC-123",
  "vehicleBrand": "Toyota",
  "vehicleModel": "Corolla"
}
```

### API Endpoint Changes

#### GET Endpoints

No changes to how you call the endpoints, but the response will no longer include `serviceId`.

#### POST Endpoint

When creating a new reminder, you no longer need to send `serviceId`. The system will automatically use a default service.

```json
// OLD REQUEST BODY
{
  "vehicleId": 1,
  "serviceId": 2,           // No longer required
  "reminderDate": "2025-09-01T00:00:00Z",
  "intervalMonths": 6,
  "notifyBeforeDays": 14,
  "notes": "Regular maintenance"
}

// NEW REQUEST BODY
{
  "vehicleId": 1,
  "reminderDate": "2025-09-01T00:00:00Z",
  "intervalMonths": 6,
  "notifyBeforeDays": 14,
  "notes": "Regular maintenance"
}
```

#### PUT Endpoint

Similarly, when updating a reminder, you no longer need to send `serviceId`.

```json
// OLD REQUEST BODY
{
  "serviceId": 2,           // No longer required
  "reminderDate": "2025-09-01T00:00:00Z",
  "intervalMonths": 6,
  "notifyBeforeDays": 14,
  "notes": "Updated notes",
  "isActive": true
}

// NEW REQUEST BODY
{
  "reminderDate": "2025-09-01T00:00:00Z",
  "intervalMonths": 6,
  "notifyBeforeDays": 14,
  "notes": "Updated notes",
  "isActive": true
}
```

### Flutter Code Updates

#### Fetching Reminders

Your model classes should be updated to no longer expect the `serviceId` field:

```dart
class ServiceReminder {
  final int serviceReminderId;
  final int vehicleId;
  // serviceId removed
  final DateTime reminderDate;
  final int intervalMonths;
  final int notifyBeforeDays;
  final String? notes;
  final bool isActive;
  final DateTime createdAt;
  final DateTime? updatedAt;
  final String serviceName;
  final String vehicleRegistrationNumber;
  final String? vehicleBrand;
  final String? vehicleModel;

  ServiceReminder({
    required this.serviceReminderId,
    required this.vehicleId,
    // serviceId removed
    required this.reminderDate,
    required this.intervalMonths,
    required this.notifyBeforeDays,
    this.notes,
    required this.isActive,
    required this.createdAt,
    this.updatedAt,
    required this.serviceName,
    required this.vehicleRegistrationNumber,
    this.vehicleBrand,
    this.vehicleModel,
  });

  factory ServiceReminder.fromJson(Map<String, dynamic> json) {
    return ServiceReminder(
      serviceReminderId: json['serviceReminderId'],
      vehicleId: json['vehicleId'],
      // serviceId removed
      reminderDate: DateTime.parse(json['reminderDate']),
      intervalMonths: json['intervalMonths'],
      notifyBeforeDays: json['notifyBeforeDays'],
      notes: json['notes'],
      isActive: json['isActive'],
      createdAt: DateTime.parse(json['createdAt']),
      updatedAt: json['updatedAt'] != null ? DateTime.parse(json['updatedAt']) : null,
      serviceName: json['serviceName'],
      vehicleRegistrationNumber: json['vehicleRegistrationNumber'],
      vehicleBrand: json['vehicleBrand'],
      vehicleModel: json['vehicleModel'],
    );
  }
}
```

#### Creating a Reminder

When creating a new reminder, don't include the `serviceId` field:

```dart
Future<ServiceReminder?> createServiceReminder({
  required int vehicleId,
  // serviceId removed
  required DateTime reminderDate,
  required int intervalMonths,
  required int notifyBeforeDays,
  String? notes,
}) async {
  try {
    final response = await http.post(
      Uri.parse('$baseUrl/ServiceReminders'),
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer $_authToken',
      },
      body: jsonEncode({
        'vehicleId': vehicleId,
        // serviceId removed
        'reminderDate': reminderDate.toIso8601String(),
        'intervalMonths': intervalMonths,
        'notifyBeforeDays': notifyBeforeDays,
        'notes': notes,
      }),
    );

    if (response.statusCode == 201) {
      return ServiceReminder.fromJson(jsonDecode(response.body));
    } else {
      throw Exception('Failed to create reminder: ${response.statusCode}');
    }
  } catch (e) {
    print('Error creating service reminder: $e');
    return null;
  }
}
```

#### Updating a Reminder

Similarly, don't include the `serviceId` field when updating:

```dart
Future<bool> updateServiceReminder({
  required int reminderId,
  // serviceId removed
  required DateTime reminderDate,
  required int intervalMonths,
  required int notifyBeforeDays,
  String? notes,
  required bool isActive,
}) async {
  try {
    final response = await http.put(
      Uri.parse('$baseUrl/ServiceReminders/$reminderId'),
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer $_authToken',
      },
      body: jsonEncode({
        // serviceId removed
        'reminderDate': reminderDate.toIso8601String(),
        'intervalMonths': intervalMonths,
        'notifyBeforeDays': notifyBeforeDays,
        'notes': notes,
        'isActive': isActive,
      }),
    );

    return response.statusCode == 204;
  } catch (e) {
    print('Error updating service reminder: $e');
    return false;
  }
}
```

## Migration Plan

1. Update your backend to the latest version
2. Update your Flutter models to remove the `serviceId` field
3. Update your API service classes to no longer send `serviceId`
4. Test with the test endpoints to verify everything works
5. Deploy your updated Flutter app

If you have any questions about these changes, please contact the development team.
