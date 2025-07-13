# Vehicle Service History PDF Generation API Guide

## Overview
This API provides endpoints to generate PDF reports of vehicle service history, clearly distinguishing between verified and unverified services.

## Features
- **Verified Services**: Services performed at registered service centers with official records
- **Unverified Services**: Self-reported services or services from non-registered centers
- **Visual Distinction**: Different styling and status indicators for verified vs unverified services
- **Comprehensive Reports**: Full service history with vehicle information and service summary
- **Summary Reports**: Condensed version with key statistics only

## API Endpoints

### 1. Generate Full Service History PDF
**Endpoint**: `GET /api/Pdf/vehicle-service-history/{vehicleId}`
**Description**: Generates a complete PDF report with vehicle info, summary, and detailed service history.

**Example Request**:
```
GET /api/Pdf/vehicle-service-history/1
```

**Response**: PDF file download with filename format: `ServiceHistory_ABC-1234_20250713.pdf`

### 2. Generate Service Summary PDF
**Endpoint**: `GET /api/Pdf/vehicle-service-summary/{vehicleId}`
**Description**: Generates a summary PDF with vehicle info and service statistics only.

**Example Request**:
```
GET /api/Pdf/vehicle-service-summary/1
```

**Response**: PDF file download with filename format: `ServiceSummary_ABC-1234_20250713.pdf`

### 3. Preview Service History PDF
**Endpoint**: `GET /api/Pdf/vehicle-service-history/{vehicleId}/preview`
**Description**: Returns PDF for inline preview in browser instead of download.

**Example Request**:
```
GET /api/Pdf/vehicle-service-history/1/preview
```

**Response**: PDF file for inline display in browser

## Test Endpoints (Mock Data)

### 1. Test with Mixed Services
**Endpoint**: `GET /api/TestPdf/test-vehicle-service-history`
**Description**: Test PDF with both verified and unverified services

### 2. Test with Empty History
**Endpoint**: `GET /api/TestPdf/test-empty-service-history`
**Description**: Test PDF with no service history

### 3. Test with Verified Only
**Endpoint**: `GET /api/TestPdf/test-verified-only`
**Description**: Test PDF with only verified services

### 4. Test with Unverified Only
**Endpoint**: `GET /api/TestPdf/test-unverified-only`
**Description**: Test PDF with only unverified services

## PDF Content Structure

### 1. Header Section
- Company name: "Vehicle Passport System"
- Document title
- Generation date and time

### 2. Vehicle Information Section
- Registration number
- Owner information
- Brand, model, year
- Fuel type
- Chassis number
- Current mileage

### 3. Service History Summary Section
- Total number of services
- Number of verified services (highlighted in green)
- Number of unverified services (highlighted in red)
- Total cost of all services
- Average cost per service
- Date of last service

### 4. Detailed Service History Section
- Table with all services sorted by date (newest first)
- Columns: Date, Service Type, Description, Service Provider, Cost, Status
- **Verified services** display:
  - Service center name from registered centers
  - Technician name who performed the service
  - "VERIFIED" status in green
  - Normal text styling
- **Unverified services** display:
  - External service center name (if provided) or "Self-Reported"
  - No technician information
  - "UNVERIFIED" status in orange
  - Slightly grayed out text
  - Light gray background

### 5. Legend Section
- Explanation of verified vs unverified services
- Clear distinction between the two types

### 6. Footer Section
- Generation timestamp
- System identification
- Disclaimer about unverified services

## Visual Indicators

### Verified Services
- ✅ Green "VERIFIED" status badge
- Normal black text
- White background
- Service center and technician information included

### Unverified Services
- ⚠️ Orange "UNVERIFIED" status badge
- Grayed out text
- Light gray background
- External service center name or "Self-Reported"

## Error Handling
- **404**: Vehicle not found
- **500**: Internal server error with detailed error message

## Example cURL Commands

### Download Full Service History
```bash
curl -X GET "http://localhost:5039/api/Pdf/vehicle-service-history/1" \
  -H "accept: application/pdf" \
  --output "service_history.pdf"
```

### Download Service Summary
```bash
curl -X GET "http://localhost:5039/api/Pdf/vehicle-service-summary/1" \
  -H "accept: application/pdf" \
  --output "service_summary.pdf"
```

### Test with Mock Data
```bash
curl -X GET "http://localhost:5039/api/TestPdf/test-vehicle-service-history" \
  -H "accept: application/pdf" \
  --output "test_service_history.pdf"
```

## Integration Notes

### Frontend Integration
1. **Download Button**: Direct link to PDF endpoint
2. **Preview**: Use iframe or embed with preview endpoint
3. **Loading States**: Show loading indicator during PDF generation
4. **Error Handling**: Display user-friendly error messages

### Mobile App Integration
1. **Download**: Use file download API
2. **Preview**: Open in system PDF viewer
3. **Sharing**: Use platform-specific sharing APIs

### Security Considerations
- Implement proper authentication for production use
- Validate vehicle ownership before generating PDFs
- Add rate limiting to prevent abuse
- Consider adding watermarks for official documents

## Database Requirements
The PDF generation relies on the following database relationships:
- `Vehicle` → `Customer` (owner information)
- `VehicleServiceHistory` → `Vehicle` (service records)
- `VehicleServiceHistory` → `ServiceCenter` (for verified services)
- `VehicleServiceHistory` → `User` (technician information)

## Performance Considerations
- PDF generation is CPU-intensive; consider async processing for large reports
- Implement caching for frequently requested reports
- Use pagination for very large service histories
- Consider background processing for bulk PDF generation
