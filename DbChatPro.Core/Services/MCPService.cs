using DbChatPro.Core.Models;
using DbChatPro.Core.Repositories;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Text;
using System.Net.Http;

namespace DbChatPro.Core.Services
{
    public class MCPService : IMCPService
    {
        private readonly IRepository<QueryHistory> _queryHistoryRepository;
        private readonly IRepository<DatabaseConnection> _connectionRepository;
        private readonly IEnterpriseService _enterpriseService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<MCPService> _logger;
        private readonly HttpClient _httpClient;

        public MCPService(
            IRepository<QueryHistory> queryHistoryRepository,
            IRepository<DatabaseConnection> connectionRepository,
            IEnterpriseService enterpriseService,
            IConfiguration configuration,
            ILogger<MCPService> logger,
            HttpClient httpClient)
        {
            _queryHistoryRepository = queryHistoryRepository;
            _connectionRepository = connectionRepository;
            _enterpriseService = enterpriseService;
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<MCPQueryResult> ExecuteQueryAsync(string prompt, string aiModel, string aiPlatform, string? sessionId = null, string? userId = null)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var result = new MCPQueryResult();

            try
            {
                _logger.LogInformation("Executing MCP query: {Prompt} with model {Model} on platform {Platform}", prompt, aiModel, aiPlatform);

                // Get active database connection
                var connections = await _connectionRepository.FindAsync(c => c.IsActive);
                var connection = connections.FirstOrDefault();

                if (connection == null)
                {
                    throw new InvalidOperationException("No active database connection found");
                }

                // Call MCP server to execute query
                var mcpRequest = new
                {
                    prompt = prompt,
                    aiModel = aiModel,
                    aiPlatform = aiPlatform,
                    databaseType = connection.DatabaseType,
                    databaseConnectionString = connection.ConnectionString
                };

                var json = JsonSerializer.Serialize(mcpRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Call the MCP server endpoint
                var response = await _httpClient.PostAsync("/mcp/execute", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var mcpResponse = JsonSerializer.Deserialize<MCPExecuteResponse>(responseContent);

                    if (mcpResponse != null)
                    {
                        result.GeneratedSql = mcpResponse.Query ?? "";
                        result.Results = mcpResponse.Results ?? new List<List<string>>();
                        result.IsSuccessful = true;
                        result.RowsReturned = result.Results.Count;
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"MCP server error: {response.StatusCode} - {errorContent}");
                }

                // Log query history
                await LogQueryHistoryAsync(connection.Id, prompt, result.GeneratedSql, aiModel, aiPlatform, true, null, result.RowsReturned, sessionId, userId);

                // Log audit event
                await _enterpriseService.LogAuditEventAsync(
                    "MCP_QUERY_EXECUTED",
                    "QueryHistory",
                    null,
                    userId,
                    null,
                    null,
                    JsonSerializer.Serialize(new { prompt, aiModel, aiPlatform, rowsReturned = result.RowsReturned })
                );

                _logger.LogInformation("MCP query executed successfully. Rows returned: {RowsReturned}", result.RowsReturned);
            }
            catch (Exception ex)
            {
                result.IsSuccessful = false;
                result.ErrorMessage = ex.Message;

                _logger.LogError(ex, "Failed to execute MCP query: {Prompt}", prompt);

                // Log failed query
                var connections = await _connectionRepository.FindAsync(c => c.IsActive);
                var connection = connections.FirstOrDefault();
                if (connection != null)
                {
                    await LogQueryHistoryAsync(connection.Id, prompt, "", aiModel, aiPlatform, false, ex.Message, 0, sessionId, userId);
                }

                // Log audit event
                await _enterpriseService.LogAuditEventAsync(
                    "MCP_QUERY_FAILED",
                    "QueryHistory",
                    null,
                    userId,
                    null,
                    null,
                    JsonSerializer.Serialize(new { prompt, aiModel, aiPlatform, error = ex.Message })
                );
            }
            finally
            {
                stopwatch.Stop();
                result.ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds;
            }

            return result;
        }

        public async Task<string> GenerateSQLQueryAsync(string prompt, string aiModel, string aiPlatform)
        {
            try
            {
                _logger.LogInformation("Generating SQL query for prompt: {Prompt}", prompt);

                // Get active database connection
                var connections = await _connectionRepository.FindAsync(c => c.IsActive);
                var connection = connections.FirstOrDefault();

                if (connection == null)
                {
                    throw new InvalidOperationException("No active database connection found");
                }

                // Call MCP server to generate SQL
                var mcpRequest = new
                {
                    prompt = prompt,
                    aiModel = aiModel,
                    aiPlatform = aiPlatform,
                    databaseType = connection.DatabaseType,
                    databaseConnectionString = connection.ConnectionString
                };

                var json = JsonSerializer.Serialize(mcpRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Call the MCP server endpoint
                var response = await _httpClient.PostAsync("/mcp/generate-sql", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var mcpResponse = JsonSerializer.Deserialize<MCPGenerateSQLResponse>(responseContent);

                    if (mcpResponse != null)
                    {
                        return mcpResponse.Query ?? "";
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"MCP server error: {response.StatusCode} - {errorContent}");
                }

                return "";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate SQL query for prompt: {Prompt}", prompt);
                throw;
            }
        }

        public async Task<DatabaseSchema> GetDatabaseSchemaAsync()
        {
            try
            {
                var connections = await _connectionRepository.FindAsync(c => c.IsActive);
                var connection = connections.FirstOrDefault();

                if (connection == null)
                {
                    throw new InvalidOperationException("No active database connection found");
                }

                // Call MCP server to get schema
                var response = await _httpClient.GetAsync("/mcp/schema");
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var mcpResponse = JsonSerializer.Deserialize<MCPSchemaResponse>(responseContent);

                    if (mcpResponse != null)
                    {
                        return new DatabaseSchema
                        {
                            SchemaRaw = mcpResponse.SchemaRaw ?? new string[0],
                            DatabaseConnectionId = connection.Id,
                            LastRefreshed = DateTime.UtcNow
                        };
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"MCP server error: {response.StatusCode} - {errorContent}");
                }

                return new DatabaseSchema
                {
                    SchemaRaw = new string[0],
                    DatabaseConnectionId = connection.Id,
                    LastRefreshed = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get database schema");
                throw;
            }
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                var connections = await _connectionRepository.FindAsync(c => c.IsActive);
                var connection = connections.FirstOrDefault();

                if (connection == null)
                {
                    return false;
                }

                // Call MCP server to test connection
                var response = await _httpClient.GetAsync("/mcp/status");
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var mcpResponse = JsonSerializer.Deserialize<MCPStatusResponse>(responseContent);

                    return mcpResponse?.IsConnected ?? false;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to test MCP connection");
                return false;
            }
        }

        public async Task<MCPConnectionStatus> GetConnectionStatusAsync()
        {
            var status = new MCPConnectionStatus();

            try
            {
                var connections = await _connectionRepository.FindAsync(c => c.IsActive);
                var connection = connections.FirstOrDefault();

                if (connection != null)
                {
                    status.IsConnected = await TestConnectionAsync();
                    status.DatabaseType = connection.DatabaseType;
                }
                else
                {
                    status.IsConnected = false;
                    status.ErrorMessage = "No active database connection found";
                }
            }
            catch (Exception ex)
            {
                status.IsConnected = false;
                status.ErrorMessage = ex.Message;
                _logger.LogError(ex, "Failed to get MCP connection status");
            }

            return status;
        }

        public async Task<MCPMetrics> GetMetricsAsync()
        {
            var metrics = new MCPMetrics();

            try
            {
                var allQueries = await _queryHistoryRepository.GetAllAsync();
                var recentQueries = allQueries.Where(q => q.CreatedAt >= DateTime.UtcNow.AddDays(-30)).ToList();

                metrics.TotalQueriesExecuted = recentQueries.Count;
                metrics.SuccessfulQueries = recentQueries.Count(q => q.IsSuccessful);
                metrics.FailedQueries = recentQueries.Count(q => !q.IsSuccessful);
                metrics.AverageExecutionTimeMs = recentQueries.Any() ? (int)recentQueries.Average(q => q.ExecutionTimeMs) : 0;
                metrics.LastQueryTime = recentQueries.Any() ? recentQueries.Max(q => q.CreatedAt) : DateTime.UtcNow;

                // Group by AI model
                metrics.QueriesByModel = recentQueries
                    .Where(q => !string.IsNullOrEmpty(q.AiModel))
                    .GroupBy(q => q.AiModel!)
                    .ToDictionary(g => g.Key, g => g.Count());

                // Group by AI platform
                metrics.QueriesByPlatform = recentQueries
                    .Where(q => !string.IsNullOrEmpty(q.AiService))
                    .GroupBy(q => q.AiService!)
                    .ToDictionary(g => g.Key, g => g.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get MCP metrics");
            }

            return metrics;
        }

        private async Task LogQueryHistoryAsync(Guid connectionId, string userPrompt, string generatedSql, string? aiModel, string? aiService, bool isSuccessful, string? errorMessage, int rowsReturned, string? sessionId, string? userId)
        {
            try
            {
                var queryHistory = new QueryHistory
                {
                    Id = Guid.NewGuid(),
                    DatabaseConnectionId = connectionId,
                    UserPrompt = userPrompt,
                    GeneratedSql = generatedSql,
                    AiModel = aiModel,
                    AiService = aiService,
                    IsSuccessful = isSuccessful,
                    ErrorMessage = errorMessage,
                    RowsReturned = rowsReturned,
                    SessionId = sessionId,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };

                await _queryHistoryRepository.AddAsync(queryHistory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log query history");
            }
        }

        // MCP Response classes
        private class MCPExecuteResponse
        {
            public string? Query { get; set; }
            public List<List<string>>? Results { get; set; }
            public bool IsSuccessful { get; set; }
            public string? ErrorMessage { get; set; }
        }

        private class MCPGenerateSQLResponse
        {
            public string? Query { get; set; }
            public bool IsSuccessful { get; set; }
            public string? ErrorMessage { get; set; }
        }

        private class MCPSchemaResponse
        {
            public string[]? SchemaRaw { get; set; }
            public bool IsSuccessful { get; set; }
            public string? ErrorMessage { get; set; }
        }

        private class MCPStatusResponse
        {
            public bool IsConnected { get; set; }
            public string? ErrorMessage { get; set; }
        }
    }
}