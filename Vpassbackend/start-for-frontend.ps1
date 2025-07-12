Write-Host "Starting Vehicle Passport Backend API for Frontend Connection..." -ForegroundColor Cyan

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
Write-Host "Starting the backend server..." -ForegroundColor Green
Write-Host "The API will be available at:" -ForegroundColor Cyan
Write-Host "  - HTTP:  http://localhost:5039" -ForegroundColor White
Write-Host "  - HTTPS: https://localhost:7039" -ForegroundColor White
Write-Host ""
Write-Host "Use Ctrl+C to stop the server" -ForegroundColor Yellow
Write-Host "Swagger UI: http://localhost:5039/swagger" -ForegroundColor Magenta
Write-Host ""
Write-Host "Frontend connection instructions can be found in Frontend_Connection_Guide.md" -ForegroundColor Green
Write-Host ""

# Run the application with specific URLs
dotnet run --urls="http://localhost:5039;https://localhost:7039"
