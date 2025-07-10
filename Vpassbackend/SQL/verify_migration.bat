@echo off
echo Running database schema verification script...
sqlcmd -S .\SQLEXPRESS -E -i verify_migration.sql -o migration_verification_results.txt
if %ERRORLEVEL% EQU 0 (
    echo Script executed successfully. Results saved to migration_verification_results.txt
    type migration_verification_results.txt
) else (
    echo Error executing SQL script
)
pause
