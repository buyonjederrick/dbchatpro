using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;
using DBChatPro.Services;
using DBChatPro;
using DBChatPro.Models;
using DBChatPro.MCPServer;

// Create a generic host builder for
// dependency injection, logging, and configuration.
var builder = Host.CreateApplicationBuilder(args);

// Configure logging for better integration with MCP clients.
builder.Logging.AddConsole(consoleLogOptions =>
{
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});

// Register the MCP server and configure it to use stdio transport.
// Scan the assembly for tool definitions.
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<DbChatProServer>()
    .WithTools<AdvancedDbChatProServer>()
    .WithTools<EnterpriseDbChatProServer>();

// Register core services
builder.Services.AddScoped<SqlServerDatabaseService>();
builder.Services.AddScoped<AIService>();

// Register advanced services
builder.Services.AddScoped<AdvancedAIService>();
builder.Services.AddScoped<AdvancedDatabaseService>();

// Register enterprise services
builder.Services.AddScoped<EnterpriseAIService>();
builder.Services.AddScoped<EnterpriseDatabaseService>();

// Register AWS Bedrock runtime for enterprise features
builder.Services.AddAWSService<Amazon.BedrockRuntime.IAmazonBedrockRuntime>();

var host = builder.Build();

// Build and run the host. This starts the MCP server.
await host.RunAsync();