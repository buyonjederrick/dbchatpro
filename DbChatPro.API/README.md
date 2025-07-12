# DBChatPro API

This is the Web API version of DBChatPro that exposes the core functionality through REST endpoints, enabling you to build clients using any framework (React, Angular, Vue, etc.).

## Features

- **Database Connections**: Connect to SQL Server, MySQL, PostgreSQL, and Oracle databases
- **AI-Powered Queries**: Generate SQL queries from natural language using various AI services
- **Chat Interface**: General chat functionality with AI models
- **CORS Support**: Configured for React and other web clients
- **Swagger Documentation**: Interactive API documentation

## Supported AI Services

- Azure OpenAI
- OpenAI
- Ollama
- GitHub AI Models
- AWS Bedrock

## API Endpoints

### Database Management

#### `POST /api/database/connect`
Connect to a database and retrieve its schema.

**Request Body:**
```json
{
  "name": "My Database",
  "databaseType": "MSSQL",
  "connectionString": "Data Source=localhost;Initial Catalog=mydb;Trusted_Connection=True;"
}
```

**Response:**
```json
{
  "name": "My Database",
  "databaseType": "MSSQL",
  "isConnected": true,
  "schema": {
    "schemaRaw": ["CREATE TABLE Users...", "CREATE TABLE Orders..."],
    "tables": [
      {
        "tableName": "Users",
        "columns": ["Id", "Name", "Email"]
      }
    ]
  }
}
```

#### `GET /api/database/supported-types`
Get list of supported database types.

**Response:**
```json
["MSSQL", "MYSQL", "POSTGRESQL", "ORACLE"]
```

### AI Query Generation

#### `POST /api/ai/query`
Generate SQL queries from natural language prompts.

**Request Body:**
```json
{
  "prompt": "Show me the top 10 customers by order value",
  "aiModel": "gpt-4",
  "aiService": "OpenAI",
  "databaseType": "MSSQL",
  "connectionString": "Data Source=localhost;Initial Catalog=mydb;Trusted_Connection=True;"
}
```

**Response:**
```json
{
  "summary": "This query retrieves the top 10 customers based on their total order value...",
  "query": "SELECT TOP 10 c.CustomerName, SUM(o.OrderValue) as TotalOrderValue FROM Customers c JOIN Orders o ON c.CustomerId = o.CustomerId GROUP BY c.CustomerName ORDER BY TotalOrderValue DESC",
  "results": [
    {
      "CustomerName": "John Doe",
      "TotalOrderValue": 15000.00
    }
  ]
}
```

### AI Chat

#### `POST /api/ai/chat`
General chat functionality with AI models.

**Request Body:**
```json
{
  "messages": [
    {
      "role": "user",
      "content": "Hello, how can you help me with databases?"
    }
  ],
  "aiModel": "gpt-4",
  "aiService": "OpenAI"
}
```

**Response:**
```json
{
  "response": "I can help you with database queries, schema analysis, and SQL generation..."
}
```

### AI Models

#### `GET /api/ai/models`
Get available AI models for each service.

**Response:**
```json
{
  "AzureOpenAI": ["gpt-4", "gpt-4o", "gpt-35-turbo"],
  "OpenAI": ["gpt-4", "gpt-4o", "gpt-3.5-turbo"],
  "Ollama": ["llama2", "codellama", "mistral"],
  "GitHubModels": ["gpt-4", "gpt-4o", "gpt-3.5-turbo"],
  "AWSBedrock": ["anthropic.claude-3-sonnet-20240229-v1:0"]
}
```

## Configuration

Configure your AI services in `appsettings.json`:

```json
{
  "EnvironmentMode": "local",
  "AZURE_OPENAI_ENDPOINT": "https://your-resource.openai.azure.com/",
  "AZURE_TENANT_ID": "your-tenant-id",
  "AZURE_CLIENT_ID": "your-client-id",
  "OPENAI_KEY": "your-openai-key",
  "OLLAMA_ENDPOINT": "http://localhost:11434",
  "GITHUB_MODELS_KEY": "your-github-models-key",
  "AWS": {
    "Region": "us-east-1",
    "Profile": "your-aws-profile"
  }
}
```

## Running the API

1. **Build the project:**
   ```bash
   dotnet build DbChatPro.API/DbChatPro.API.csproj
   ```

2. **Run the API:**
   ```bash
   dotnet run --project DbChatPro.API/DbChatPro.API.csproj
   ```

3. **Access the API:**
   - API Base URL: `https://localhost:5001` or `http://localhost:5000`
   - Swagger Documentation: `https://localhost:5001` (served at root in development)

## React Client Example

A complete React client example is included in the `react-client-example` directory. To run it:

1. **Install dependencies:**
   ```bash
   cd react-client-example
   npm install
   ```

2. **Start the development server:**
   ```bash
   npm run dev
   ```

3. **Access the React app:**
   - URL: `http://localhost:3000`
   - The app is configured to proxy API requests to the .NET API

## CORS Configuration

The API is configured to allow requests from common React development ports:
- `http://localhost:3000` (Create React App)
- `http://localhost:5173` (Vite)
- `http://localhost:4173` (Vite preview)

## Error Handling

All endpoints return consistent error responses:

```json
{
  "errorMessage": "Description of the error"
}
```

## Security Notes

- Connection strings are sent in requests but not stored
- Consider implementing authentication for production use
- Use HTTPS in production environments
- Validate and sanitize all inputs

## Database Connection String Examples

### SQL Server
```
Data Source=localhost;Initial Catalog=mydb;Trusted_Connection=True;TrustServerCertificate=true
```

### MySQL
```
Server=127.0.0.1;Port=3306;Database=mydb;Uid=username;Pwd=password
```

### PostgreSQL
```
Host=127.0.0.1;Port=5432;Database=mydb;Username=username;Password=password
```

### Oracle
```
User Id=username;Password=password;Data Source=host:port/service
```