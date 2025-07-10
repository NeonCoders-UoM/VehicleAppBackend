@echo off
echo Running database update script to add coordinates to ServiceCenters table...

:: Replace these variables with your SQL Server connection information
set server=.\SQLEXPRESS
set database=VehicleAppDb
set username=sa
set password=YourPassword

:: Run the SQL script using sqlcmd
sqlcmd -S %server% -d %database% -U %username% -P %password% -i AddCoordinatesToServiceCenter.sql

if %ERRORLEVEL% EQU 0 (
    echo Database updated successfully!
) else (
    echo Error updating database. Please check your SQL Server connection details.
)

pause
