@echo off
setlocal

echo Starting CarService.Api on https://localhost:5001 ...
start "CarService.Api" cmd /k "cd /d %~dp0src\CarService.Api && dotnet run"

timeout /t 3 >nul

echo Starting CarService.Mvc on https://localhost:5003 ...
start "CarService.Mvc" cmd /k "cd /d %~dp0src\CarService.Mvc && dotnet run"

echo.
echo Done. Open:
echo  - API Swagger: https://localhost:5001/swagger
echo  - MVC:        https://localhost:5003
endlocal
