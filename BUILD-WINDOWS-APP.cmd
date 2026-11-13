@echo off
setlocal
cd /d "%~dp0"
dotnet publish "CampusLostAndFound.Desktop\CampusLostAndFound.Desktop.csproj" -c Release -r win-x64 --self-contained true -o WindowsApp
if errorlevel 1 goto failed
echo Done. Open WindowsApp\CampusLostAndFound.exe.
pause
exit /b 0
:failed
echo Publish failed. Install the .NET 8 SDK and check your internet connection.
pause
exit /b 1
