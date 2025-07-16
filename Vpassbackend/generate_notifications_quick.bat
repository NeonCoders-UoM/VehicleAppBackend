@echo off
echo ============================================
echo  Generate Notifications from Service Reminders
echo ============================================
echo.
echo This script will generate notifications from your existing service reminders.
echo.
echo Prerequisites:
echo 1. Your application must be running (dotnet run)
echo 2. Service reminders must exist in the database
echo.
echo Starting notification generation...
echo.

curl -X POST "https://localhost:7038/api/Notifications/GenerateFromServiceReminders" ^
     -H "Content-Type: application/json" ^
     -k --silent --show-error

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ✓ Notification generation completed successfully!
    echo.
    echo Check your Flutter app or use the API to view the new notifications:
    echo GET https://localhost:7038/api/Notifications/Customer/{customerId}
) else (
    echo.
    echo ❌ Failed to generate notifications.
    echo.
    echo Possible issues:
    echo - Application is not running (run: dotnet run)
    echo - SSL certificate issues (run: dotnet dev-certs https --trust)
    echo - No service reminders due for notification
    echo.
    echo For detailed testing, run: .\generate_notifications_from_reminders.ps1
)

echo.
pause
