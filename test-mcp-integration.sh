#!/bin/bash

# Test MCP Integration Script
# This script tests the complete MCP integration

echo "Testing DbChatPro MCP Integration..."

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Function to test endpoint
test_endpoint() {
    local method=$1
    local endpoint=$2
    local data=$3
    local description=$4
    
    echo -e "${YELLOW}Testing: $description${NC}"
    
    if [ -n "$data" ]; then
        response=$(curl -s -X $method "http://localhost:5000$endpoint" \
            -H "Content-Type: application/json" \
            -d "$data")
    else
        response=$(curl -s -X $method "http://localhost:5000$endpoint")
    fi
    
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✓ Success${NC}"
        echo "Response: $response" | head -c 200
        echo "..."
    else
        echo -e "${RED}✗ Failed${NC}"
    fi
    echo ""
}

# Wait for services to be ready
echo "Waiting for services to start..."
sleep 5

# Test MCP status
test_endpoint "GET" "/api/mcp/status" "" "MCP Status Check"

# Test schema endpoint
test_endpoint "GET" "/api/mcp/schema" "" "Database Schema"

# Test SQL generation
test_endpoint "POST" "/api/mcp/generate-sql" '{
  "prompt": "Show me all users",
  "aiModel": "gpt-4",
  "aiPlatform": "AzureOpenAI",
  "databaseType": "SqlServer",
  "databaseConnectionString": "Server=localhost;Database=master;Trusted_Connection=true;"
}' "SQL Generation"

# Test query execution
test_endpoint "POST" "/api/mcp/execute" '{
  "prompt": "Show me all users",
  "aiModel": "gpt-4",
  "aiPlatform": "AzureOpenAI",
  "databaseType": "SqlServer",
  "databaseConnectionString": "Server=localhost;Database=master;Trusted_Connection=true;"
}' "Query Execution"

# Test enterprise MCP endpoints
test_endpoint "GET" "/api/enterprise/mcp/status" "" "Enterprise MCP Status"

echo -e "${GREEN}MCP Integration Test Complete!${NC}"
echo ""
echo "If all tests passed, the MCP integration is working correctly."
echo "Check the responses above for any error messages."