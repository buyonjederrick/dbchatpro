using DbChatPro.Core.Models;
using DbChatPro.Core.Repositories;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace DbChatPro.Core.Services
{
    public class MCPService : IMCPService
    {
        private readonly IRepository<QueryHistory> _queryHistoryRepository;
        private readonly IRepository<DatabaseConnection> _connectionRepository;
        private readonly IEnterpriseService _enterpriseService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<MCPService> _logger;

        public MCPService(
            IRepository<QueryHistory> queryHistoryRepository,
            IRepository<DatabaseConnection> connectionRepository,
            IEnterpriseService enterpriseService,
            IConfiguration configuration,
            ILogger<MCPService> logger)
        {
            _queryHistoryRepository = queryHistoryRepository;
            _connectionRepository = connectionRepository;
            _enterpriseService = enterpriseService;
            _configuration = configuration;
            _logger = logger;
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

                // TODO: Integrate with actual MCP server call
                // For now, simulate the MCP server response
                var generatedSql = await GenerateSQLQueryAsync(prompt, aiModel, aiPlatform);
                result.GeneratedSql = generatedSql;

                // Simulate query execution
                result.Results = new List<List<string>>
                {
                    new List<string> { "Sample", "Data", "Result" },
                    new List<string> { "1", "Test", "Value" }
                };

                result.IsSuccessful = true;
                result.RowsReturned = result.Results.Count;

                // Log query history
                await LogQueryHistoryAsync(connection.Id, prompt, generatedSql, aiModel, aiPlatform, true, null, result.RowsReturned, sessionId, userId);

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

                // TODO: Integrate with actual MCP server call
                // For now, return a simulated SQL query
                return $"SELECT * FROM SampleTable WHERE Description LIKE '%{prompt}%'";
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

                // TODO: Integrate with actual MCP server call
                // For now, return a simulated schema
                return new DatabaseSchema
                {
                    SchemaRaw = "Sample schema data",
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

                // TODO: Integrate with actual MCP server call
                // For now, simulate connection test
                return true;
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
                    UserPrompt = userPrompt,
                    GeneratedSql = generatedSql,
                    AiModel = aiModel,
                    AiService = aiService,
                    IsSuccessful = isSuccessful,
                    ErrorMessage = errorMessage,
                    RowsReturned = rowsReturned,
                    DatabaseConnectionId = connectionId,
                    SessionId = sessionId,
                    UserId = userId,
                    ExecutionTimeMs = 0 // Will be updated by the calling method
                };

                await _queryHistoryRepository.AddAsync(queryHistory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log query history");
            }
        }
    }
}