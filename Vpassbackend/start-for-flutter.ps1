Write-Host "Starting Vehicle Passport Backend API for Flutter Connection..." -ForegroundColor Cyan

# Get the current directory
$currentDir = Get-Location
Write-Host "Working directory: $currentDir" -ForegroundColor Gray

# Apply any pending database migrations
Write-Host "Applying database migrations..." -ForegroundColor Yellow
dotnet ef database update
if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to apply migrations. Continuing anyway..." -ForegroundColor Red
}

# Start the backend with HTTPS and specific ports for frontend access
Write-Host "Starting the backend server for Flutter..." -ForegroundColor Green
Write-Host "The API will be available at:" -ForegroundColor Cyan
Write-Host "  - HTTP:  http://localhost:5039" -ForegroundColor White
Write-Host ""
Write-Host "Use Ctrl+C to stop the server" -ForegroundColor Yellow
Write-Host "Swagger UI: http://localhost:5039/swagger" -ForegroundColor Magenta
Write-Host ""
Write-Host "Flutter connection test endpoints:" -ForegroundColor Green
Write-Host "  - Basic test: http://localhost:5039/api/FlutterTest" -ForegroundColor White
Write-Host "  - CORS test:  http://localhost:5039/api/FlutterTest/cors" -ForegroundColor White
Write-Host "  - Sample reminders: http://localhost:5039/api/FlutterTest/reminders/1" -ForegroundColor White
Write-Host ""
Write-Host "Flutter connection instructions can be found in Flutter_Connection_Guide.md" -ForegroundColor Green
Write-Host ""

# Run the application with specific URLs and without HTTPS to avoid certificate issues
dotnet run --urls="http://localhost:5039" --no-https
