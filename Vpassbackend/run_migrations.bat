@echo off
echo Running database migrations...
dotnet ef database update
echo Migration complete
pause
