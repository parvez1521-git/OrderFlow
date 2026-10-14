@echo off
setlocal

REM ================================================================
REM OrderFlow full local startup script
REM ------------------------------------------------
REM Run this file from PowerShell or Command Prompt:
REM
REM   cd C:\Training2026\OrderFlow
REM   .\run-orderflow.bat
REM
REM This starts:
REM   1. Backend projects on fixed localhost ports
REM      - OrderFlow.Api on http://localhost:5078 for admin login/auth
REM      - CustomerService on http://localhost:6101
REM      - ProductService on http://localhost:6102
REM      - InventoryService on http://localhost:6103
REM      - OrderService on http://localhost:6104
REM      - PaymentService on http://localhost:6105
REM      - NotificationService on http://localhost:6106
REM      - Gateway on http://localhost:6200
REM
REM   2. React Customer Portal on http://localhost:5173
REM   3. Angular Admin Portal on http://localhost:4200
REM ================================================================

set "ORDERFLOW_ROOT=C:\Training2026\OrderFlow"
set "CUSTOMER_PORTAL=%ORDERFLOW_ROOT%\CustomerPortal"
set "ADMIN_PORTAL=%ORDERFLOW_ROOT%\AdminPortal"
set "API_PROJECT=%ORDERFLOW_ROOT%\OrderFlow.Api\OrderFlow.Api.csproj"
set "CUSTOMER_SERVICE_PROJECT=%ORDERFLOW_ROOT%\Services\CustomerService\OrderFlow.CustomerService.csproj"
set "PRODUCT_SERVICE_PROJECT=%ORDERFLOW_ROOT%\Services\ProductService\OrderFlow.ProductService.csproj"
set "INVENTORY_SERVICE_PROJECT=%ORDERFLOW_ROOT%\Services\InventoryService\OrderFlow.InventoryService.csproj"
set "ORDER_SERVICE_PROJECT=%ORDERFLOW_ROOT%\Services\OrderService\OrderFlow.OrderService.csproj"
set "PAYMENT_SERVICE_PROJECT=%ORDERFLOW_ROOT%\Services\PaymentService\OrderFlow.PaymentService.csproj"
set "NOTIFICATION_SERVICE_PROJECT=%ORDERFLOW_ROOT%\Services\NotificationService\OrderFlow.NotificationService.csproj"
set "GATEWAY_PROJECT=%ORDERFLOW_ROOT%\Gateway\OrderFlow.Gateway.csproj"
set "API_DLL=%ORDERFLOW_ROOT%\OrderFlow.Api\bin\Debug\net10.0\OrderFlow.Api.dll"
set "CUSTOMER_SERVICE_DLL=%ORDERFLOW_ROOT%\Services\CustomerService\bin\Debug\net10.0\OrderFlow.CustomerService.dll"
set "PRODUCT_SERVICE_DLL=%ORDERFLOW_ROOT%\Services\ProductService\bin\Debug\net10.0\OrderFlow.ProductService.dll"
set "INVENTORY_SERVICE_DLL=%ORDERFLOW_ROOT%\Services\InventoryService\bin\Debug\net10.0\OrderFlow.InventoryService.dll"
set "ORDER_SERVICE_DLL=%ORDERFLOW_ROOT%\Services\OrderService\bin\Debug\net10.0\OrderFlow.OrderService.dll"
set "PAYMENT_SERVICE_DLL=%ORDERFLOW_ROOT%\Services\PaymentService\bin\Debug\net10.0\OrderFlow.PaymentService.dll"
set "NOTIFICATION_SERVICE_DLL=%ORDERFLOW_ROOT%\Services\NotificationService\bin\Debug\net10.0\OrderFlow.NotificationService.dll"
set "AI_SERVICE_DLL=%ORDERFLOW_ROOT%\Services\AiService\bin\Debug\net10.0\OrderFlow.AiService.dll"
set "GATEWAY_DLL=%ORDERFLOW_ROOT%\Gateway\bin\Debug\net10.0\OrderFlow.Gateway.dll"

if not exist "%ORDERFLOW_ROOT%" (
    echo OrderFlow root folder was not found:
    echo %ORDERFLOW_ROOT%
    pause
    exit /b 1
)

if not exist "%GATEWAY_PROJECT%" (
    echo Gateway project was not found:
    echo %GATEWAY_PROJECT%
    pause
    exit /b 1
)

where dotnet >nul 2>nul
if errorlevel 1 (
    echo dotnet command was not found. Please install the .NET SDK or open a Developer PowerShell.
    pause
    exit /b 1
)

where npm >nul 2>nul
if errorlevel 1 (
    echo npm command was not found. Please install Node.js or make sure npm is available on PATH.
    pause
    exit /b 1
)

echo Starting OrderFlow API for admin login...
start "OrderFlow API - Auth" powershell -NoExit -ExecutionPolicy Bypass -Command "$env:ASPNETCORE_ENVIRONMENT='Development'; $env:ASPNETCORE_URLS='http://localhost:5078'; Set-Location -LiteralPath '%ORDERFLOW_ROOT%\OrderFlow.Api'; dotnet '%API_DLL%'"

echo Starting CustomerService...
start "OrderFlow CustomerService" powershell -NoExit -ExecutionPolicy Bypass -Command "$env:ASPNETCORE_ENVIRONMENT='Development'; $env:ASPNETCORE_URLS='http://localhost:6101'; Set-Location -LiteralPath '%ORDERFLOW_ROOT%\Services\CustomerService'; dotnet '%CUSTOMER_SERVICE_DLL%'"

echo Starting ProductService...
start "OrderFlow ProductService" powershell -NoExit -ExecutionPolicy Bypass -Command "$env:ASPNETCORE_ENVIRONMENT='Development'; $env:ASPNETCORE_URLS='http://localhost:6102'; Set-Location -LiteralPath '%ORDERFLOW_ROOT%\Services\ProductService'; dotnet '%PRODUCT_SERVICE_DLL%'"

echo Starting InventoryService...
start "OrderFlow InventoryService" powershell -NoExit -ExecutionPolicy Bypass -Command "$env:ASPNETCORE_ENVIRONMENT='Development'; $env:ASPNETCORE_URLS='http://localhost:6103'; Set-Location -LiteralPath '%ORDERFLOW_ROOT%\Services\InventoryService'; dotnet '%INVENTORY_SERVICE_DLL%'"

echo Starting OrderService...
start "OrderFlow OrderService" powershell -NoExit -ExecutionPolicy Bypass -Command "$env:ASPNETCORE_ENVIRONMENT='Development'; $env:ASPNETCORE_URLS='http://localhost:6104'; Set-Location -LiteralPath '%ORDERFLOW_ROOT%\Services\OrderService'; dotnet '%ORDER_SERVICE_DLL%'"

echo Starting PaymentService...
start "OrderFlow PaymentService" powershell -NoExit -ExecutionPolicy Bypass -Command "$env:ASPNETCORE_ENVIRONMENT='Development'; $env:ASPNETCORE_URLS='http://localhost:6105'; Set-Location -LiteralPath '%ORDERFLOW_ROOT%\Services\PaymentService'; dotnet '%PAYMENT_SERVICE_DLL%'"

echo Starting NotificationService...
start "OrderFlow NotificationService" powershell -NoExit -ExecutionPolicy Bypass -Command "$env:ASPNETCORE_ENVIRONMENT='Development'; $env:ASPNETCORE_URLS='http://localhost:6106'; Set-Location -LiteralPath '%ORDERFLOW_ROOT%\Services\NotificationService'; dotnet '%NOTIFICATION_SERVICE_DLL%'"

echo Starting AiService (Ollama Assistant)...
start "OrderFlow AiService" powershell -NoExit -ExecutionPolicy Bypass -Command "$env:ASPNETCORE_ENVIRONMENT='Development'; $env:ASPNETCORE_URLS='http://localhost:6107'; Set-Location -LiteralPath '%ORDERFLOW_ROOT%\Services\AiService'; dotnet '%AI_SERVICE_DLL%'"

echo Starting KnowledgeService (Python RAG)...
start "OrderFlow KnowledgeService - Python RAG" powershell -NoExit -ExecutionPolicy Bypass -Command "Set-Location -LiteralPath '%ORDERFLOW_ROOT%\Services\KnowledgeService'; python -m uvicorn main:app --host 127.0.0.1 --port 8000"

echo Starting McpServer (Model Context Protocol)...
start "OrderFlow MCP Server - SSE" powershell -NoExit -ExecutionPolicy Bypass -Command "Set-Location -LiteralPath '%ORDERFLOW_ROOT%\Services\McpServer'; python server.py --sse --port 8001"

echo Waiting a few seconds before starting gateway...
powershell -NoProfile -Command "Start-Sleep -Seconds 8"

echo Starting YARP Gateway...
start "OrderFlow Gateway" powershell -NoExit -ExecutionPolicy Bypass -Command "$env:ASPNETCORE_ENVIRONMENT='Development'; $env:ASPNETCORE_URLS='http://localhost:6200'; Set-Location -LiteralPath '%ORDERFLOW_ROOT%\Gateway'; dotnet '%GATEWAY_DLL%'"

echo Waiting a few seconds before starting portals...
powershell -NoProfile -Command "Start-Sleep -Seconds 5"

echo Starting React Customer Portal...
start "OrderFlow Customer Portal - React" powershell -NoExit -ExecutionPolicy Bypass -Command "Set-Location -LiteralPath '%CUSTOMER_PORTAL%'; if (-not (Test-Path -LiteralPath 'node_modules')) { npm install }; npm run dev"

echo Starting Angular Admin Portal...
start "OrderFlow Admin Portal - Angular" powershell -NoExit -ExecutionPolicy Bypass -Command "Set-Location -LiteralPath '%ADMIN_PORTAL%'; if (-not (Test-Path -LiteralPath 'node_modules')) { npm install }; npm start"

echo.
echo OrderFlow startup commands have been launched.
echo.
echo Open these URLs after the terminal windows finish starting:
echo   Customer Portal:  http://localhost:5173
echo   Admin Portal:     http://localhost:4200
echo   Gateway health:   http://localhost:6200/health
echo   AI Assistant:     Embedded in Customer Portal (:5173)
echo   RAG Knowledge:    http://127.0.0.1:8000/docs
echo   MCP Server (SSE): http://127.0.0.1:8001/sse (stdio: python Services\McpServer\server.py)
echo.
echo Admin login:
echo   Email:    admin@oms.local
echo   Password: ChangeMe123!
echo.
echo Redis and RabbitMQ are optional for this direct local startup.
echo If you want Redis/RabbitMQ orchestration, start Docker Desktop and run the Aspire AppHost manually.
echo To stop everything, press Ctrl+C in each opened terminal window.
echo.
pause

endlocal
