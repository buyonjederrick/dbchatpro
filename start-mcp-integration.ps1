# Start MCP Integration Script
# This script starts both the DbChatPro API and the MCP server

Write-Host "Starting DbChatPro MCP Integration..." -ForegroundColor Green

# Set environment variables for MCP server
$env:DATABASETYPE = "SqlServer"
$env:DATABASECONNECTIONSTRING = "Server=localhost;Database=master;Trusted_Connection=true;"

# Start the MCP server in the background
Write-Host "Starting MCP Server..." -ForegroundColor Yellow
Start-Process -FilePath "dotnet" -ArgumentList "run", "--project", "DbChatPro.MCPServer" -WindowStyle Hidden

# Wait a moment for the MCP server to start
Start-Sleep -Seconds 3

# Start the API
Write-Host "Starting DbChatPro API..." -ForegroundColor Yellow
dotnet run --project DbChatPro.API

Write-Host "MCP Integration started successfully!" -ForegroundColor Green
Write-Host "API is running on: http://localhost:5000" -ForegroundColor Cyan
Write-Host "MCP Server is running on: http://localhost:5001" -ForegroundColor Cyan
Write-Host "Swagger UI: http://localhost:5000" -ForegroundColor Cyan