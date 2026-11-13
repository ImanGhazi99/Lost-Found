@echo off
setlocal
cd /d "%~dp0"
where dotnet >nul 2>nul
if errorlevel 1 goto missing
dotnet --list-sdks | findstr /r "." >nul
if errorlevel 1 goto missing
echo Building and opening the Windows Forms application...
dotnet run --project "CampusLostAndFound.Desktop\CampusLostAndFound.Desktop.csproj" --configuration Release
if errorlevel 1 (
 echo Build or launch failed. Read the error above.
 pause
)
exit /b
:missing
echo Install the .NET 8 SDK for Windows, then run this file again.
echo Alternatively open the solution in Visual Studio with .NET desktop development installed.
pause
