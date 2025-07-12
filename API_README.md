# DBChatPro Web API

This project has been enhanced with a **Web API** that exposes all the core DBChatPro functionality through REST endpoints, enabling you to build clients using React, Angular, Vue, or any other framework.

## 🚀 New Features

### Web API (`DbChatPro.API`)
- **RESTful endpoints** for all DBChatPro functionality
- **CORS support** for web clients
- **Swagger documentation** for easy API exploration
- **Comprehensive error handling**
- **Support for all AI services** (Azure OpenAI, OpenAI, Ollama, GitHub Models, AWS Bedrock)

### React Client Example (`react-client-example`)
- **Complete React application** demonstrating API usage
- **Modern UI** with Material-UI components
- **Real-time database connections**
- **AI query generation interface**
- **Responsive design**

## 📁 Project Structure

```
DBChatPro/
├── DbChatPro.Core/           # Core business logic and services
├── DbChatPro.API/            # 🆕 Web API project
│   ├── Controllers/          # API endpoints
│   ├── Models/              # API request/response models
│   └── Program.cs           # API configuration
├── DBChatPro.UI/            # Original Blazor UI
├── DbChatPro.MCPServer/     # Model Context Protocol server
├── react-client-example/     # 🆕 React client example
│   ├── src/
│   │   ├── App.jsx         # Main React component
│   │   └── main.jsx        # React entry point
│   ├── package.json         # React dependencies
│   └── vite.config.js       # Vite configuration
└── DBChatPro.sln            # Updated solution file
```

## 🛠️ Quick Start

### 1. Build and Run the API

```bash
# Build the entire solution
dotnet build

# Run the API
dotnet run --project DbChatPro.API/DbChatPro.API.csproj
```

The API will be available at:
- **API**: `https://localhost:5001` or `http://localhost:5000`
- **Swagger Docs**: `https://localhost:5001` (served at root in development)

### 2. Run the React Client

```bash
# Navigate to React client
cd react-client-example

# Install dependencies
npm install

# Start development server
npm run dev
```

The React app will be available at `http://localhost:3000`

## 🔌 API Endpoints

### Database Management
- `POST /api/database/connect` - Connect to database and get schema
- `GET /api/database/supported-types` - Get supported database types

### AI Query Generation
- `POST /api/ai/query` - Generate SQL from natural language
- `POST /api/ai/chat` - General AI chat functionality
- `GET /api/ai/models` - Get available AI models

## 🎯 React Client Features

The React client demonstrates:

- **Database Connection Management**
  - Connect to multiple databases
  - View database schemas
  - Select active database for queries

- **AI Query Generation**
  - Natural language to SQL conversion
  - Multiple AI service support
  - Real-time query results display

- **Modern UI/UX**
  - Material-UI components
  - Responsive design
  - Loading states and error handling
  - Real-time feedback

## 🔧 Configuration

### API Configuration (`DbChatPro.API/appsettings.json`)

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

### React Client Configuration

The React client is configured to proxy API requests to the .NET API:

```javascript
// vite.config.js
export default defineConfig({
  server: {
    proxy: {
      '/api': {
        target: 'http://localhost:5000',
        changeOrigin: true
      }
    }
  }
})
```

## 🚀 Usage Examples

### Using the API Directly

```javascript
// Connect to database
const response = await fetch('/api/database/connect', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    name: 'My Database',
    databaseType: 'MSSQL',
    connectionString: 'Data Source=localhost;Initial Catalog=mydb;Trusted_Connection=True;'
  })
});

// Generate AI query
const queryResponse = await fetch('/api/ai/query', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    prompt: 'Show me the top 10 customers by order value',
    aiModel: 'gpt-4',
    aiService: 'OpenAI',
    databaseType: 'MSSQL',
    connectionString: 'Data Source=localhost;Initial Catalog=mydb;Trusted_Connection=True;'
  })
});
```

### Using with React

The React client provides a complete example of how to integrate with the API:

```jsx
// Example from the React client
const handleQuerySubmit = async (e) => {
  e.preventDefault();
  
  const response = await axios.post('/api/ai/query', {
    prompt: prompt,
    aiModel: aiModel,
    aiService: aiService,
    databaseType: selectedConnection.databaseType,
    connectionString: selectedConnection.connectionString
  });

  setQueryResult(response.data);
};
```

## 🔒 Security Considerations

- **Connection strings** are sent in requests but not stored
- **CORS** is configured for development ports
- **HTTPS** should be used in production
- Consider implementing **authentication** for production use

## 🧪 Testing

### Test the API

1. **Start the API:**
   ```bash
   dotnet run --project DbChatPro.API/DbChatPro.API.csproj
   ```

2. **Open Swagger UI:**
   Navigate to `https://localhost:5001` to see interactive API documentation

3. **Test endpoints:**
   Use the Swagger UI to test all endpoints directly

### Test the React Client

1. **Start the React app:**
   ```bash
   cd react-client-example
   npm run dev
   ```

2. **Configure AI services:**
   Update `DbChatPro.API/appsettings.json` with your AI service keys

3. **Test functionality:**
   - Connect to a database
   - Generate AI queries
   - View results

## 🏗️ Building Your Own Client

The API is designed to work with any framework. Here's how to get started:

1. **Choose your framework** (React, Angular, Vue, etc.)
2. **Configure CORS** if needed
3. **Use the API endpoints** documented in Swagger
4. **Handle responses** according to the API models

## 📚 Additional Resources

- **API Documentation**: Available at `https://localhost:5001` when running
- **React Example**: Complete example in `react-client-example/`
- **Original Documentation**: See `readme.md` for the original project details

## 🤝 Contributing

The API maintains the same functionality as the original DBChatPro while providing a modern REST interface. You can:

- Add new endpoints to `DbChatPro.API/Controllers/`
- Extend the React client in `react-client-example/`
- Improve error handling and validation
- Add authentication and authorization

## 📄 License

This project maintains the same license as the original DBChatPro project.