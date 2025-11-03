@echo off
echo Setting up Package System...
echo.

echo Running migration...
dotnet ef database update
echo.

echo Seeding default packages...
sqlcmd -S "DESKTOP-F8C2NUA\SQLEXPRESS" -d VehiclePassportAppNew21 -i add_default_packages.sql
echo.

echo Package system setup complete!
pause 