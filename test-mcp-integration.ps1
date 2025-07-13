# Test MCP Integration Script for Windows
# This script tests the complete MCP integration

Write-Host "Testing DbChatPro MCP Integration..." -ForegroundColor Green

# Function to test endpoint
function Test-Endpoint {
    param(
        [string]$Method,
        [string]$Endpoint,
        [string]$Data = "",
        [string]$Description
    )
    
    Write-Host "Testing: $Description" -ForegroundColor Yellow
    
    try {
        if ($Data) {
            $response = Invoke-RestMethod -Uri "http://localhost:5000$Endpoint" -Method $Method -ContentType "application/json" -Body $Data
        } else {
            $response = Invoke-RestMethod -Uri "http://localhost:5000$Endpoint" -Method $Method
        }
        
        Write-Host "✓ Success" -ForegroundColor Green
        $responseJson = $response | ConvertTo-Json -Depth 3
        Write-Host "Response: $($responseJson.Substring(0, [Math]::Min(200, $responseJson.Length)))..." -ForegroundColor Gray
    }
    catch {
        Write-Host "✗ Failed" -ForegroundColor Red
        Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    }
    
    Write-Host ""
}

# Wait for services to be ready
Write-Host "Waiting for services to start..." -ForegroundColor Yellow
Start-Sleep -Seconds 5

# Test MCP status
Test-Endpoint -Method "GET" -Endpoint "/api/mcp/status" -Description "MCP Status Check"

# Test schema endpoint
Test-Endpoint -Method "GET" -Endpoint "/api/mcp/schema" -Description "Database Schema"

# Test SQL generation
$sqlGenerationData = @{
    prompt = "Show me all users"
    aiModel = "gpt-4"
    aiPlatform = "AzureOpenAI"
    databaseType = "SqlServer"
    databaseConnectionString = "Server=localhost;Database=master;Trusted_Connection=true;"
} | ConvertTo-Json

Test-Endpoint -Method "POST" -Endpoint "/api/mcp/generate-sql" -Data $sqlGenerationData -Description "SQL Generation"

# Test query execution
$queryExecutionData = @{
    prompt = "Show me all users"
    aiModel = "gpt-4"
    aiPlatform = "AzureOpenAI"
    databaseType = "SqlServer"
    databaseConnectionString = "Server=localhost;Database=master;Trusted_Connection=true;"
} | ConvertTo-Json

Test-Endpoint -Method "POST" -Endpoint "/api/mcp/execute" -Data $queryExecutionData -Description "Query Execution"

# Test enterprise MCP endpoints
Test-Endpoint -Method "GET" -Endpoint "/api/enterprise/mcp/status" -Description "Enterprise MCP Status"

Write-Host "MCP Integration Test Complete!" -ForegroundColor Green
Write-Host ""
Write-Host "If all tests passed, the MCP integration is working correctly." -ForegroundColor Cyan
Write-Host "Check the responses above for any error messages." -ForegroundColor Cyan