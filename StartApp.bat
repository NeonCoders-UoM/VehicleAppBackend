@echo off
echo Starting Vehicle App Backend...
echo.

cd /d D:\NeonCoders\VehicleAppBackend\VehicleAppBackend\Vpassbackend

REM Check if the directory exists
if not exist . (
  echo Error: Directory not found.
  exit /b 1
)

echo Building the application...
dotnet build

if %ERRORLEVEL% NEQ 0 (
  echo Error: Build failed.
  pause
  exit /b %ERRORLEVEL%
)

echo.
echo Starting the application...
echo.
echo Application will be available at:
echo HTTP: http://localhost:5039
echo HTTPS: https://localhost:7005
echo.
echo Press Ctrl+C to stop the application.

dotnet run --urls="http://localhost:5039;https://localhost:7005"

pause
