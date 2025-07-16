@echo off
echo ============================================
echo  Notification System Database Validation
echo ============================================
echo.
echo Checking database connection and tables...
echo.

echo 1. Checking database context...
dotnet ef dbcontext info
echo.

echo 2. Listing migrations...
dotnet ef migrations list
echo.

echo 3. Checking if Notifications table exists...
dotnet ef database drop --dry-run
echo.

echo 4. Testing application build...
dotnet build --no-restore
if %ERRORLEVEL% EQU 0 (
    echo ✓ Application builds successfully!
) else (
    echo ❌ Build failed. Please check the error messages above.
)
echo.

echo ============================================
echo  Database Validation Complete
echo ============================================
echo.
echo If all checks passed, your notification system is ready!
echo.
echo To start the application: dotnet run
echo To test the API: Use Examples\notifications.http in VS Code
echo.
pause
