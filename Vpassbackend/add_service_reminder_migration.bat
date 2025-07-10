@echo off
cd /d %~dp0
dotnet ef migrations add AddServiceReminders
pause
