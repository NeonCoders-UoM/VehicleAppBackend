# Fuel Efficiency API Documentation

## Overview
The Fuel Efficiency API provides comprehensive functionality for tracking vehicle fuel consumption, generating monthly summaries, and displaying chart data for fuel usage patterns.

## Base URL
```
/api/FuelEfficiency
```

## Endpoints

### 1. Add Fuel Record
**POST** `/api/FuelEfficiency`

Add a new fuel record for a vehicle.

**Request Body:**
```json
{
  "vehicleId": 1,
  "fuelAmount": 50.5,
  "date": "2025-04-15",
  "notes": "Gas station near home"
}
```

**Response:**
```json
{
  "fuelEfficiencyId": 123,
  "vehicleId": 1,
  "fuelAmount": 50.5,
  "date": "2025-04-15T00:00:00",
  "notes": "Gas station near home",
  "createdAt": "2025-04-15T10:30:00Z"
}
```

### 2. Get Vehicle Fuel Records
**GET** `/api/FuelEfficiency/vehicle/{vehicleId}`

Get all fuel records for a specific vehicle, ordered by date (newest first).

**Response:**
```json
[
  {
    "fuelEfficiencyId": 123,
    "vehicleId": 1,
    "fuelAmount": 50.5,
    "date": "2025-04-15T00:00:00",
    "notes": "Gas station near home",
    "createdAt": "2025-04-15T10:30:00Z"
  },
  {
    "fuelEfficiencyId": 122,
    "vehicleId": 1,
    "fuelAmount": 45.0,
    "date": "2025-04-01T00:00:00",
    "notes": "Monthly refill",
    "createdAt": "2025-04-01T09:15:00Z"
  }
]
```

### 3. Get Fuel Summary with Chart Data
**GET** `/api/FuelEfficiency/vehicle/{vehicleId}/summary?year=2025`

Get comprehensive fuel summary including monthly chart data for the specified year.

**Response:**
```json
{
  "vehicleId": 1,
  "vehicleRegistrationNumber": "ABC-1234",
  "monthlySummary": [
    {
      "year": 2025,
      "month": 1,
      "monthName": "January",
      "totalFuelAmount": 120.5,
      "recordCount": 3
    },
    {
      "year": 2025,
      "month": 2,
      "monthName": "February",
      "totalFuelAmount": 95.0,
      "recordCount": 2
    },
    ...
  ],
  "totalFuelThisYear": 1250.5,
  "averageMonthlyFuel": 104.2
}
```

### 4. Get Monthly Chart Data
**GET** `/api/FuelEfficiency/vehicle/{vehicleId}/chart/{year}`

Get chart data for all 12 months of the specified year (includes months with zero fuel).

**Response:**
```json
[
  {
    "year": 2025,
    "month": 1,
    "monthName": "January",
    "totalFuelAmount": 120.5,
    "recordCount": 3
  },
  {
    "year": 2025,
    "month": 2,
    "monthName": "February",
    "totalFuelAmount": 0,
    "recordCount": 0
  },
  ...
]
```

### 5. Get Monthly Detail Records
**GET** `/api/FuelEfficiency/vehicle/{vehicleId}/monthly/{year}/{month}`

Get all individual fuel records for a specific month.

**Response:**
```json
[
  {
    "fuelEfficiencyId": 123,
    "vehicleId": 1,
    "fuelAmount": 50.5,
    "date": "2025-04-15T00:00:00",
    "notes": "First fill of the month",
    "createdAt": "2025-04-15T10:30:00Z"
  },
  {
    "fuelEfficiencyId": 124,
    "vehicleId": 1,
    "fuelAmount": 45.0,
    "date": "2025-04-25T00:00:00",
    "notes": "Second fill of the month",
    "createdAt": "2025-04-25T11:45:00Z"
  }
]
```

### 6. Get Fuel for Date Range
**GET** `/api/FuelEfficiency/vehicle/{vehicleId}/period?startDate=2025-01-01&endDate=2025-03-31`

Get total fuel consumption for a specific date range.

**Response:**
```json
{
  "totalFuel": 315.5,
  "startDate": "2025-01-01T00:00:00",
  "endDate": "2025-03-31T00:00:00"
}
```

### 7. Get Average Monthly Fuel
**GET** `/api/FuelEfficiency/vehicle/{vehicleId}/average/{year}`

Get the average fuel consumption per month for the specified year.

**Response:**
```json
{
  "averageMonthlyFuel": 104.2,
  "year": 2025
}
```

### 8. Update Fuel Record
**PUT** `/api/FuelEfficiency/{id}`

Update an existing fuel record.

**Request Body:**
```json
{
  "vehicleId": 1,
  "fuelAmount": 55.0,
  "date": "2025-04-15",
  "notes": "Updated amount - premium fuel"
}
```

**Response:** 204 No Content

### 9. Delete Fuel Record
**DELETE** `/api/FuelEfficiency/{id}`

Delete a fuel record.

**Response:** 204 No Content

## Business Logic

### Monthly Aggregation
- Each month's bar in the chart represents the **total** fuel added during that month
- If you add 5 liters on week 1, then 10 liters on week 3, the month shows 15 liters
- If you add 2 more liters in week 4, the month total becomes 17 liters
- This cumulative approach gives you the complete fuel consumption per month

### Chart Data Structure
- Always returns 12 months of data (January to December)
- Months with no fuel records show 0 amount
- Perfect for creating consistent bar charts
- Each bar height represents the total fuel for that month

### Validation Rules
- Fuel amount must be greater than 0
- Date cannot be in the future
- Vehicle must exist in the database
- Fuel amount is stored with 2 decimal precision

### Frontend Integration Example
```javascript
// Fetch chart data for current year
fetch('/api/FuelEfficiency/vehicle/1/chart/2025')
  .then(response => response.json())
  .then(data => {
    // data contains 12 months with totalFuelAmount for each
    const chartLabels = data.map(month => month.monthName);
    const chartValues = data.map(month => month.totalFuelAmount);
    
    // Use chartLabels and chartValues to create your bar chart
    createBarChart(chartLabels, chartValues);
  });

// Add new fuel record
const newFuelRecord = {
  vehicleId: 1,
  fuelAmount: 50.5,
  date: '2025-04-15',
  notes: 'Regular refill'
};

fetch('/api/FuelEfficiency', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify(newFuelRecord)
})
.then(response => response.json())
.then(result => {
  console.log('Fuel record added:', result);
  // Refresh chart data
  refreshChart();
});
```
