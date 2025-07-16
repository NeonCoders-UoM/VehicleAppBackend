@echo off
echo Testing notification system...
echo.

echo Starting application to generate notifications...
start /min dotnet run

echo Waiting for application to start...
timeout /t 10 /nobreak > nul

echo.
echo Application should be running now.
echo Visit: http://localhost:5000/api/notifications
echo Or use the PowerShell script: .\test_notifications.ps1
echo.
echo Press any key to continue...
pause > nul
