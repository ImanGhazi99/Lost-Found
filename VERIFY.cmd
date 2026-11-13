@echo off
setlocal
cd /d "%~dp0"
dotnet build CampusLostAndFound.sln -c Release
if errorlevel 1 goto failed
dotnet run --project CampusLostAndFound.Tests -c Release --no-build
if errorlevel 1 goto failed
echo Build and domain tests passed. Complete the manual GUI checks in README.md.
pause
exit /b 0
:failed
echo Verification failed. Read the error above.
pause
exit /b 1
