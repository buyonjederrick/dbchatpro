using Microsoft.AspNetCore.Mvc;
using DBChatPro.Services;
using DbChatPro.API.Models;
using DBChatPro;
using Microsoft.Extensions.AI;
using DbChatPro.Core.Services;
using DbChatPro.Core.Models;
using DbChatPro.Core.Repositories;
using System.Text.Json;

namespace DbChatPro.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AIController : ControllerBase
    {
        private readonly AIService _aiService;
        private readonly IDatabaseService _databaseService;
        private readonly IEnterpriseService _enterpriseService;
        private readonly IRepository<QueryHistory> _queryHistoryRepository;
        private readonly IRepository<DatabaseConnection> _connectionRepository;
        private readonly ILogger<AIController> _logger;

        public AIController(
            AIService aiService, 
            IDatabaseService databaseService, 
            IEnterpriseService enterpriseService,
            IRepository<QueryHistory> queryHistoryRepository,
            IRepository<DatabaseConnection> connectionRepository,
            ILogger<AIController> logger)
        {
            _aiService = aiService;
            _databaseService = databaseService;
            _enterpriseService = enterpriseService;
            _queryHistoryRepository = queryHistoryRepository;
            _connectionRepository = connectionRepository;
            _logger = logger;
        }

        [HttpPost("query")]
        public async Task<ActionResult<AIQueryResponse>> GenerateQuery([FromBody] AIQueryRequest request)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            try
            {
                _logger.LogInformation("Generating AI query for prompt: {Prompt}", request.Prompt);

                // Get or create database connection
                var connection = await GetOrCreateConnectionAsync(request.DatabaseType, request.ConnectionString, request.ConnectionName);

                // Get database schema
                var schema = await _databaseService.GetDatabaseSchema(request.DatabaseType, request.ConnectionString);
                
                // Generate AI query
                var aiQuery = await _aiService.GetAISQLQuery(request.AiModel, request.AiService, request.Prompt, schema, request.DatabaseType);
                
                // Execute the query to get results
                List<Dictionary<string, object>>? results = null;
                int rowsReturned = 0;
                bool isSuccessful = true;
                string? errorMessage = null;
                
                try
                {
                    results = await _databaseService.ExecuteQuery(request.DatabaseType, request.ConnectionString, aiQuery.query);
                    rowsReturned = results?.Count ?? 0;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to execute generated query, returning query only");
                    isSuccessful = false;
                    errorMessage = ex.Message;
                }

                stopwatch.Stop();

                // Log query history
                await LogQueryHistoryAsync(connection.Id, request.Prompt, aiQuery.query, request.AiModel, request.AiService, isSuccessful, errorMessage, rowsReturned, request.SessionId, request.UserId, (int)stopwatch.ElapsedMilliseconds);

                // Log audit event
                await _enterpriseService.LogAuditEventAsync(
                    "AI_QUERY_GENERATED",
                    "QueryHistory",
                    null,
                    request.UserId,
                    null,
                    null,
                    JsonSerializer.Serialize(new { prompt = request.Prompt, aiModel = request.AiModel, aiService = request.AiService, rowsReturned, isSuccessful })
                );

                return Ok(new AIQueryResponse
                {
                    Summary = aiQuery.summary,
                    Query = aiQuery.query,
                    Results = results,
                    ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds,
                    RowsReturned = rowsReturned
                });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Failed to generate AI query");
                
                // Log failed query
                var connection = await GetOrCreateConnectionAsync(request.DatabaseType, request.ConnectionString, request.ConnectionName);
                await LogQueryHistoryAsync(connection.Id, request.Prompt, "", request.AiModel, request.AiService, false, ex.Message, 0, request.SessionId, request.UserId, (int)stopwatch.ElapsedMilliseconds);

                return Ok(new AIQueryResponse
                {
                    Summary = string.Empty,
                    Query = string.Empty,
                    ErrorMessage = ex.Message,
                    ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds
                });
            }
        }

        [HttpPost("chat")]
        public async Task<ActionResult<ChatResponse>> Chat([FromBody] ChatRequest request)
        {
            try
            {
                _logger.LogInformation("Processing chat request with {MessageCount} messages", request.Messages.Count);

                var chatMessages = request.Messages.Select(m => new ChatMessage(
                    m.Role switch
                    {
                        "user" => Microsoft.Extensions.AI.ChatRole.User,
                        "assistant" => Microsoft.Extensions.AI.ChatRole.Assistant,
                        "system" => Microsoft.Extensions.AI.ChatRole.System,
                        _ => Microsoft.Extensions.AI.ChatRole.User
                    },
                    m.Content
                )).ToList();

                var response = await _aiService.ChatPrompt(chatMessages, request.AiModel, request.AiService);
                
                return Ok(new ChatResponse
                {
                    Response = response.Messages[0].Text
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process chat request");
                
                return Ok(new ChatResponse
                {
                    Response = string.Empty,
                    ErrorMessage = ex.Message
                });
            }
        }

        [HttpGet("models")]
        public ActionResult<Dictionary<string, List<string>>> GetAvailableModels()
        {
            return Ok(new Dictionary<string, List<string>>
            {
                ["AzureOpenAI"] = new List<string> { "gpt-4", "gpt-4o", "gpt-35-turbo" },
                ["OpenAI"] = new List<string> { "gpt-4", "gpt-4o", "gpt-3.5-turbo" },
                ["Ollama"] = new List<string> { "llama2", "codellama", "mistral" },
                ["GitHubModels"] = new List<string> { "gpt-4", "gpt-4o", "gpt-3.5-turbo" },
                ["AWSBedrock"] = new List<string> { "anthropic.claude-3-sonnet-20240229-v1:0", "anthropic.claude-3-haiku-20240307-v1:0" }
            });
        }

        [HttpGet("history")]
        public async Task<ActionResult<PagedResult<QueryHistory>>> GetQueryHistory(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] string? userId = null,
            [FromQuery] bool? isSuccessful = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var predicate = PredicateBuilder.True<QueryHistory>();

                if (fromDate.HasValue)
                    predicate = predicate.And(q => q.CreatedAt >= fromDate.Value);

                if (toDate.HasValue)
                    predicate = predicate.And(q => q.CreatedAt <= toDate.Value);

                if (!string.IsNullOrEmpty(userId))
                    predicate = predicate.And(q => q.UserId == userId);

                if (isSuccessful.HasValue)
                    predicate = predicate.And(q => q.IsSuccessful == isSuccessful.Value);

                var queries = await _queryHistoryRepository.GetPagedAsync(page, pageSize, predicate, q => q.CreatedAt, false);
                var totalCount = await _queryHistoryRepository.CountAsync(predicate);

                return Ok(new PagedResult<QueryHistory>
                {
                    Data = queries,
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get query history");
                return StatusCode(500, new { error = "Failed to retrieve query history" });
            }
        }

        private async Task<DatabaseConnection> GetOrCreateConnectionAsync(string databaseType, string connectionString, string? connectionName)
        {
            // Try to find existing connection
            var existingConnections = await _connectionRepository.FindAsync(c => 
                c.DatabaseType == databaseType && 
                c.ConnectionString == connectionString);

            var connection = existingConnections.FirstOrDefault();

            if (connection == null)
            {
                // Create new connection
                connection = new DatabaseConnection
                {
                    Name = connectionName ?? $"Connection_{DateTime.UtcNow:yyyyMMdd_HHmmss}",
                    DatabaseType = databaseType,
                    ConnectionString = connectionString,
                    IsActive = true,
                    Environment = "Production"
                };

                connection = await _connectionRepository.AddAsync(connection);

                await _enterpriseService.LogAuditEventAsync(
                    "DATABASE_CONNECTION_CREATED",
                    "DatabaseConnection",
                    connection.Id,
                    null,
                    null,
                    null,
                    JsonSerializer.Serialize(new { databaseType, connectionName = connection.Name })
                );
            }

            return connection;
        }

        private async Task LogQueryHistoryAsync(Guid connectionId, string userPrompt, string generatedSql, string? aiModel, string? aiService, bool isSuccessful, string? errorMessage, int rowsReturned, string? sessionId, string? userId, int executionTimeMs)
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
                    ExecutionTimeMs = executionTimeMs
                };

                await _queryHistoryRepository.AddAsync(queryHistory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log query history");
            }
        }
    }

    public class PagedResult<T>
    {
        public IEnumerable<T> Data { get; set; } = new List<T>();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }

    // Helper class for building predicates
    public static class PredicateBuilder
    {
        public static Expression<Func<T, bool>> True<T>() { return f => true; }
        public static Expression<Func<T, bool>> False<T>() { return f => false; }

        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>(Expression.OrElse(expr1.Body, invokedExpr), expr1.Parameters);
        }

        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(expr1.Body, invokedExpr), expr1.Parameters);
        }
    }
}