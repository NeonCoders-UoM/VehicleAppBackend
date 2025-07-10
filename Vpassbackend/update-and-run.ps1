Write-Host "Applying database migrations..." -ForegroundColor Cyan
dotnet ef database update
if ($LASTEXITCODE -eq 0) {
    Write-Host "Migrations applied successfully!" -ForegroundColor Green
    Write-Host "Starting the application..." -ForegroundColor Cyan
    dotnet run
} else {
    Write-Host "Error applying migrations. Please check the logs." -ForegroundColor Red
}
