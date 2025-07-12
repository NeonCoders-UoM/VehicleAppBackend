# Flutter Connection Guide for Vehicle App Backend

This guide will help you connect your Flutter application to the Vehicle App Backend API.

## Testing the Connection

Before implementing the full API integration, test the connection using these endpoints:

### 1. Basic Connection Test

```dart
Future<bool> testBackendConnection() async {
  try {
    final response = await http.get(
      Uri.parse('http://localhost:5039/api/FlutterTest'),
      headers: {'Content-Type': 'application/json'},
    );
    print('Response status: ${response.statusCode}');
    print('Response body: ${response.body}');

    return response.statusCode == 200;
  } catch (e) {
    print('Connection test error: $e');
    return false;
  }
}
```

### 2. CORS Test

```dart
Future<bool> testCorsConnection() async {
  try {
    final response = await http.get(
      Uri.parse('http://localhost:5039/api/FlutterTest/cors'),
      headers: {'Content-Type': 'application/json'},
    );
    print('CORS test status: ${response.statusCode}');
    print('CORS test body: ${response.body}');

    return response.statusCode == 200;
  } catch (e) {
    print('CORS test error: $e');
    return false;
  }
}
```

### 3. Test Reminders Endpoint

Use this to test fetching reminders without hitting the actual database:

```dart
Future<List<dynamic>> getTestReminders(int vehicleId) async {
  try {
    final response = await http.get(
      Uri.parse('http://localhost:5039/api/FlutterTest/reminders/$vehicleId'),
      headers: {'Content-Type': 'application/json'},
    );

    if (response.statusCode == 200) {
      return json.decode(response.body);
    } else {
      throw Exception('Failed to load test reminders');
    }
  } catch (e) {
    print('Error loading test reminders: $e');
    throw Exception('Failed to fetch test reminders: $e');
  }
}
```

## Updating Your Flutter App

Update your Flutter app to use the modified endpoints:

### 1. Service Reminders

```dart
// In your API service class
Future<List<dynamic>> getVehicleReminders(int vehicleId) async {
  try {
    // Note the capital "V" in "Vehicle" - this matches the API route
    final response = await http.get(
      Uri.parse('http://localhost:5039/api/ServiceReminders/Vehicle/$vehicleId'),
      headers: {'Content-Type': 'application/json'},
    );

    if (response.statusCode == 200) {
      return json.decode(response.body);
    } else {
      throw Exception('Failed to load vehicle reminders');
    }
  } catch (e) {
    print('Network error in getVehicleReminders: $e');
    throw Exception('Failed to fetch vehicle reminders: $e');
  }
}
```

### 2. Adding Authentication

```dart
class ApiService {
  final String baseUrl = 'http://localhost:5039/api';
  String? _authToken;

  // Login method to get authentication token
  Future<bool> login(String email, String password) async {
    try {
      final response = await http.post(
        Uri.parse('$baseUrl/Auth/login'),
        headers: {'Content-Type': 'application/json'},
        body: json.encode({
          'email': email,
          'password': password
        }),
      );

      if (response.statusCode == 200) {
        final data = json.decode(response.body);
        _authToken = data['token'];
        return true;
      } else {
        return false;
      }
    } catch (e) {
      print('Login error: $e');
      return false;
    }
  }

  // Method to get vehicle reminders with auth token
  Future<List<dynamic>> getVehicleReminders(int vehicleId) async {
    try {
      final headers = {
        'Content-Type': 'application/json',
      };

      // Add auth token if available
      if (_authToken != null) {
        headers['Authorization'] = 'Bearer $_authToken';
      }

      final response = await http.get(
        Uri.parse('$baseUrl/ServiceReminders/Vehicle/$vehicleId'),
        headers: headers,
      );

      if (response.statusCode == 200) {
        return json.decode(response.body);
      } else {
        throw Exception('Failed to load vehicle reminders: ${response.statusCode}');
      }
    } catch (e) {
      print('Network error in getVehicleReminders: $e');
      throw Exception('Failed to fetch vehicle reminders: $e');
    }
  }
}
```

## Troubleshooting

### CORS Issues

If you're still experiencing CORS issues:

1. **Use a CORS Proxy for Development**

   ```dart
   final String baseUrl = 'https://cors-anywhere.herokuapp.com/http://localhost:5039/api';
   ```

2. **Try with HTTP Instead of HTTPS**

   Make sure you're using `http://` and not `https://` when connecting to localhost.

3. **Use the FlutterTest Controller**

   Use the special FlutterTest controller endpoints we created for testing.

4. **Check Network Settings**

   Make sure your Flutter app has network permissions in the app manifest.

### Authentication Issues

If you're having trouble with authentication:

```dart
// Add this debugging code to your login method
try {
  final response = await http.post(
    Uri.parse('$baseUrl/Auth/login'),
    headers: {'Content-Type': 'application/json'},
    body: json.encode({
      'email': email,
      'password': password
    }),
  );

  print('Login response status: ${response.statusCode}');
  print('Login response body: ${response.body}');

  // Rest of the method...
} catch (e) {
  print('Login error: $e');
  return false;
}
```

## Next Steps

After successfully connecting your Flutter app to the backend:

1. **Implement Full API Service**
   Create a comprehensive API service class that handles all endpoints.

2. **Add Error Handling**
   Implement proper error handling and user feedback.

3. **State Management**
   Use a state management solution (Provider, Bloc, Riverpod) to manage the API data.

4. **Caching**
   Add caching mechanisms for improved performance.
