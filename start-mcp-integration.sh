#!/bin/bash

# Start MCP Integration Script
# This script starts both the DbChatPro API and the MCP server

echo "Starting DbChatPro MCP Integration..." 

# Set environment variables for MCP server
export DATABASETYPE="SqlServer"
export DATABASECONNECTIONSTRING="Server=localhost;Database=master;Trusted_Connection=true;"

# Start the MCP server in the background
echo "Starting MCP Server..."
dotnet run --project DbChatPro.MCPServer &
MCP_PID=$!

# Wait a moment for the MCP server to start
sleep 3

# Start the API
echo "Starting DbChatPro API..."
dotnet run --project DbChatPro.API &
API_PID=$!

# Function to cleanup on exit
cleanup() {
    echo "Shutting down services..."
    kill $MCP_PID 2>/dev/null
    kill $API_PID 2>/dev/null
    exit 0
}

# Set trap to cleanup on script exit
trap cleanup SIGINT SIGTERM

echo "MCP Integration started successfully!"
echo "API is running on: http://localhost:5000"
echo "MCP Server is running on: http://localhost:5001"
echo "Swagger UI: http://localhost:5000"
echo "Press Ctrl+C to stop all services"

# Wait for background processes
wait