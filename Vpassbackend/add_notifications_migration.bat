@echo off
echo ============================================
echo  Notification System Database Setup
echo ============================================
echo.
echo This script will set up the notification system database tables.
echo.
echo Step 1: Adding Notifications migration...
dotnet ef migrations add AddNotifications
if %ERRORLEVEL% EQU 0 (
    echo ✓ Migration added successfully!
    echo.
    echo Step 2: Updating database...
    dotnet ef database update
    if %ERRORLEVEL% EQU 0 (
        echo ✓ Database updated successfully!
        echo.
        echo ============================================
        echo  Notification System Setup Complete!
        echo ============================================
        echo.
        echo Your notification backend is now ready!
        echo.
        echo Next steps:
        echo 1. Start your application: dotnet run
        echo 2. Test the API endpoints using Examples\notifications.http
        echo 3. Update your Flutter app to use the new endpoints
        echo.
    ) else (
        echo ❌ Failed to update database. Please check the error messages above.
        echo.
        echo Common solutions:
        echo - Check your connection string in appsettings.json
        echo - Ensure SQL Server is running
        echo - Run: dotnet ef database drop --force
        echo - Then run this script again
    )
) else (
    echo ❌ Failed to add migration. Please check the error messages above.
    echo.
    echo Make sure you're in the correct project directory.
)
echo.
pause
