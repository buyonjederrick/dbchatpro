# DbChatPro MCP Integration

This document describes the complete Model Context Protocol (MCP) integration for DbChatPro, which enables AI-powered database querying through a standardized protocol.

## Architecture Overview

The MCP integration consists of three main components:

1. **MCP Server** (`DbChatPro.MCPServer`) - A standalone server that implements MCP tools
2. **API Integration** (`DbChatPro.API`) - HTTP API that communicates with the MCP server
3. **Client Integration** (`react-client-advanced`) - React frontend that uses MCP capabilities

## Components

### 1. MCP Server (`DbChatPro.MCPServer`)

The MCP server provides the following tools:

- **GetSqlDataForUserPrompt** - Translates user prompts to SQL and executes them
- **GetDatabaseSchema** - Retrieves database schema information
- **GetAIGeneratedSQLQuery** - Generates SQL queries from natural language

**Key Files:**
- `DbChatProServer.cs` - Main server implementation with MCP tools
- `Program.cs` - Server startup and configuration

### 2. API Integration (`DbChatPro.API`)

The API provides HTTP endpoints that communicate with the MCP server:

- `POST /api/mcp/execute` - Execute queries through MCP
- `POST /api/mcp/generate-sql` - Generate SQL from prompts
- `GET /api/mcp/schema` - Get database schema
- `GET /api/mcp/status` - Check MCP server status

**Key Files:**
- `MCPController.cs` - HTTP endpoints for MCP operations
- `MCPService.cs` - Service layer that communicates with MCP server
- `Program.cs` - HttpClient configuration for MCP communication

### 3. Client Integration (`react-client-advanced`)

The React client includes:

- MCP-related TypeScript types
- API service methods for MCP operations
- UI components for MCP functionality

**Key Files:**
- `api.ts` - API service with MCP endpoints
- `api.ts` (types) - TypeScript interfaces for MCP operations

## Implementation Details

### MCP Server Communication

The API communicates with the MCP server using HTTP requests:

```csharp
// Example: Execute query through MCP
var mcpRequest = new
{
    prompt = "Show me all users",
    aiModel = "gpt-4",
    aiPlatform = "AzureOpenAI",
    databaseType = "SqlServer",
    databaseConnectionString = connectionString
};

var response = await _httpClient.PostAsync("/mcp/execute", content);
```

### Error Handling

The implementation includes comprehensive error handling:

- Connection failures are logged and reported
- Invalid requests return appropriate HTTP status codes
- Detailed error messages are provided for debugging

### Configuration

Environment variables required for MCP server:

```bash
DATABASETYPE=SqlServer
DATABASECONNECTIONSTRING=Server=localhost;Database=master;Trusted_Connection=true;
```

## Getting Started

### Prerequisites

1. .NET 9.0 SDK
2. SQL Server (or other supported database)
3. Node.js (for React client)

### Quick Start

1. **Start the MCP Integration:**

   **Windows:**
   ```powershell
   .\start-mcp-integration.ps1
   ```

   **Linux/macOS:**
   ```bash
   ./start-mcp-integration.sh
   ```

2. **Access the Services:**
   - API: http://localhost:5000
   - MCP Server: http://localhost:5001
   - Swagger UI: http://localhost:5000

3. **Test MCP Integration:**
   ```bash
   # Test MCP status
   curl http://localhost:5000/api/mcp/status
   
   # Execute a query
   curl -X POST http://localhost:5000/api/mcp/execute \
     -H "Content-Type: application/json" \
     -d '{
       "prompt": "Show me all users",
       "aiModel": "gpt-4",
       "aiPlatform": "AzureOpenAI",
       "databaseType": "SqlServer",
       "databaseConnectionString": "Server=localhost;Database=master;Trusted_Connection=true;"
     }'
   ```

## API Endpoints

### MCP Controller Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/mcp/execute` | Execute queries through MCP |
| POST | `/api/mcp/generate-sql` | Generate SQL from prompts |
| GET | `/api/mcp/schema` | Get database schema |
| GET | `/api/mcp/status` | Check MCP server status |

### Request/Response Examples

**Execute Query:**
```json
POST /api/mcp/execute
{
  "prompt": "Show me all users",
  "aiModel": "gpt-4",
  "aiPlatform": "AzureOpenAI",
  "databaseType": "SqlServer",
  "databaseConnectionString": "Server=localhost;Database=master;Trusted_Connection=true;"
}
```

**Response:**
```json
{
  "query": "SELECT * FROM Users",
  "results": [
    ["1", "John", "Doe"],
    ["2", "Jane", "Smith"]
  ],
  "isSuccessful": true
}
```

## React Client Integration

The React client includes MCP-related functionality:

```typescript
// Execute MCP query
const result = await api.executeMCPQuery({
  prompt: "Show me all users",
  aiModel: "gpt-4",
  aiPlatform: "AzureOpenAI",
  sessionId: "session-123",
  userId: "user-456"
});

// Check MCP status
const status = await api.getMCPStatus();
```

## Monitoring and Logging

### Logging

All MCP operations are logged with structured logging:

```csharp
_logger.LogInformation("Executing MCP query: {Prompt} with model {Model} on platform {Platform}", 
    prompt, aiModel, aiPlatform);
```

### Metrics

MCP metrics are tracked and available through the enterprise API:

- Total queries executed
- Successful vs failed queries
- Average execution time
- Queries by AI model and platform

### Audit Logging

All MCP operations are audited:

- Query execution events
- Error events
- User session tracking
- Performance metrics

## Troubleshooting

### Common Issues

1. **MCP Server Not Starting:**
   - Check environment variables
   - Verify database connection string
   - Check logs for detailed error messages

2. **API Cannot Connect to MCP Server:**
   - Ensure MCP server is running on port 5001
   - Check firewall settings
   - Verify HttpClient configuration

3. **Query Execution Failures:**
   - Check database connectivity
   - Verify AI service configuration
   - Review generated SQL for syntax errors

### Debug Mode

Enable detailed logging by setting:

```bash
export LOG_LEVEL=Debug
```

## Security Considerations

1. **Connection String Security:**
   - Use secure connection strings
   - Consider using Azure Key Vault for production
   - Implement connection string encryption

2. **API Security:**
   - Implement authentication for MCP endpoints
   - Use HTTPS in production
   - Validate all input parameters

3. **Audit Trail:**
   - All MCP operations are logged
   - User sessions are tracked
   - Performance metrics are collected

## Performance Optimization

1. **Connection Pooling:**
   - HttpClient is configured with connection pooling
   - Database connections are reused
   - MCP server maintains persistent connections

2. **Caching:**
   - Database schema is cached
   - Query results can be cached
   - AI model responses are cached where appropriate

3. **Async Operations:**
   - All MCP operations are asynchronous
   - Non-blocking I/O operations
   - Configurable timeouts

## Future Enhancements

1. **Additional MCP Tools:**
   - Database schema modification
   - Query optimization suggestions
   - Performance analysis tools

2. **Enhanced AI Integration:**
   - Support for more AI models
   - Custom prompt templates
   - Query result explanation

3. **Advanced Features:**
   - Real-time query streaming
   - Query history analysis
   - Automated query optimization

## Support

For issues or questions about the MCP integration:

1. Check the logs for detailed error messages
2. Review the troubleshooting section
3. Test with the provided examples
4. Contact the development team

## License

This MCP integration is part of the DbChatPro project and follows the same licensing terms.