@echo off
echo Running SQL script to add missing columns...
sqlcmd -S .\SQLEXPRESS -E -d VehiclePassportAppNew -i add_missing_columns.sql
if %ERRORLEVEL% EQU 0 (
    echo Successfully added missing columns
) else (
    echo Error executing SQL script
)
pause
