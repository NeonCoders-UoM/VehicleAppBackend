@echo off
cd /d %~dp0
dotnet ef database update
pause
