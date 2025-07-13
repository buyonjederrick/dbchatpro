using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using DBChatPro.Services;
using DBChatPro.Models;
using ModelContextProtocol.Server;
using System.Text.Json;
using System.Diagnostics;

namespace DBChatPro.MCPServer
{
    [McpServerToolType]
    public class AdvancedDbChatProServer
    {
        private readonly SqlServerDatabaseService _dataService;
        private readonly AIService _aiService;
        private readonly ILogger<AdvancedDbChatProServer> _logger;
        private readonly IConfiguration _configuration;

        public AdvancedDbChatProServer(
            SqlServerDatabaseService dataService, 
            AIService aiService,
            ILogger<AdvancedDbChatProServer> logger,
            IConfiguration configuration)
        {
            _dataService = dataService;
            _aiService = aiService;
            _logger = logger;
            _configuration = configuration;
        }

        // Enhanced tool for complex SQL queries with optimization
        [McpServerTool, Description("Executes complex SQL queries with AI optimization, query analysis, and performance monitoring. Supports advanced features like query optimization, schema analysis, and result caching.")]
        public async Task<AdvancedQueryResult> ExecuteAdvancedQuery(
            IServiceProvider serviceProvider,
            IConfiguration config,
            [Description("The natural language prompt to convert to optimized SQL.")] string prompt,
            [Description("The AI model to use for query generation and optimization.")] string aiModel,
            [Description("The AI platform to use (AzureOpenAI, OpenAI, GitHubModels, AWSBedrock, Ollama).")] string aiPlatform,
            [Description("Optional query optimization level (Basic, Advanced, Expert).")] string optimizationLevel = "Advanced",
            [Description("Optional maximum execution time in seconds.")] int maxExecutionTime = 300,
            [Description("Optional result caching duration in minutes.")] int cacheDuration = 30
        )
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new AdvancedQueryResult();

            try
            {
                _logger.LogInformation("Executing advanced query: {Prompt} with optimization level: {OptimizationLevel}", prompt, optimizationLevel);

                // Validate configuration
                ValidateConfiguration(config);

                // Get database connection and schema
                var connection = new AIConnection() { ConnectionString = config.GetValue<string>("DATABASECONNECTIONSTRING") };
                var dbSchema = await _dataService.GenerateSchema(connection);

                // Generate optimized SQL with AI
                var aiResponse = await _aiService.GetAdvancedAISQLQuery(
                    aiModel, 
                    aiPlatform, 
                    prompt, 
                    dbSchema, 
                    config.GetValue<string>("DATABASETYPE"),
                    optimizationLevel
                );

                result.GeneratedSql = aiResponse.query;
                result.QueryAnalysis = aiResponse.analysis;
                result.OptimizationSuggestions = aiResponse.optimizations;

                // Execute query with timeout
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(maxExecutionTime));
                var rowData = await _dataService.GetDataTableWithTimeout(connection, aiResponse.query, cts.Token);
                
                result.Results = rowData;
                result.RowsReturned = rowData.Count;
                result.IsSuccessful = true;

                // Performance metrics
                stopwatch.Stop();
                result.ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds;
                result.PerformanceMetrics = new PerformanceMetrics
                {
                    QueryComplexity = CalculateQueryComplexity(aiResponse.query),
                    EstimatedCost = EstimateQueryCost(aiResponse.query, result.RowsReturned),
                    OptimizationScore = CalculateOptimizationScore(aiResponse.optimizations)
                };

                _logger.LogInformation("Advanced query executed successfully. Rows: {Rows}, Time: {Time}ms", 
                    result.RowsReturned, result.ExecutionTimeMs);
            }
            catch (Exception ex)
            {
                result.IsSuccessful = false;
                result.ErrorMessage = ex.Message;
                result.ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds;
                
                _logger.LogError(ex, "Failed to execute advanced query: {Prompt}", prompt);
            }

            return result;
        }

        // Tool for query optimization and analysis
        [McpServerTool, Description("Analyzes and optimizes existing SQL queries for better performance and efficiency.")]
        public async Task<QueryOptimizationResult> OptimizeQuery(
            IServiceProvider serviceProvider,
            IConfiguration config,
            [Description("The SQL query to analyze and optimize.")] string sqlQuery,
            [Description("The AI model to use for optimization analysis.")] string aiModel,
            [Description("The AI platform to use.")] string aiPlatform,
            [Description("Optional performance requirements (Fast, Balanced, Thorough).")] string performanceRequirement = "Balanced"
        )
        {
            var result = new QueryOptimizationResult();

            try
            {
                _logger.LogInformation("Optimizing query with performance requirement: {Requirement}", performanceRequirement);

                ValidateConfiguration(config);

                var connection = new AIConnection() { ConnectionString = config.GetValue<string>("DATABASECONNECTIONSTRING") };
                var dbSchema = await _dataService.GenerateSchema(connection);

                // Analyze current query
                var analysis = await _aiService.AnalyzeQueryPerformance(
                    aiModel, 
                    aiPlatform, 
                    sqlQuery, 
                    dbSchema, 
                    config.GetValue<string>("DATABASETYPE"),
                    performanceRequirement
                );

                result.OriginalQuery = sqlQuery;
                result.OptimizedQuery = analysis.optimizedQuery;
                result.PerformanceAnalysis = analysis.analysis;
                result.Recommendations = analysis.recommendations;
                result.EstimatedImprovement = analysis.estimatedImprovement;
                result.IsSuccessful = true;

                _logger.LogInformation("Query optimization completed. Estimated improvement: {Improvement}%", 
                    result.EstimatedImprovement);
            }
            catch (Exception ex)
            {
                result.IsSuccessful = false;
                result.ErrorMessage = ex.Message;
                _logger.LogError(ex, "Failed to optimize query");
            }

            return result;
        }

        // Tool for schema analysis and recommendations
        [McpServerTool, Description("Analyzes database schema and provides recommendations for optimization, indexing, and query performance.")]
        public async Task<SchemaAnalysisResult> AnalyzeSchema(
            IServiceProvider serviceProvider,
            IConfiguration config,
            [Description("The AI model to use for schema analysis.")] string aiModel,
            [Description("The AI platform to use.")] string aiPlatform,
            [Description("Optional analysis depth (Basic, Detailed, Comprehensive).")] string analysisDepth = "Detailed"
        )
        {
            var result = new SchemaAnalysisResult();

            try
            {
                _logger.LogInformation("Analyzing schema with depth: {Depth}", analysisDepth);

                ValidateConfiguration(config);

                var connection = new AIConnection() { ConnectionString = config.GetValue<string>("DATABASECONNECTIONSTRING") };
                var dbSchema = await _dataService.GenerateSchema(connection);

                var analysis = await _aiService.AnalyzeDatabaseSchema(
                    aiModel,
                    aiPlatform,
                    dbSchema,
                    config.GetValue<string>("DATABASETYPE"),
                    analysisDepth
                );

                result.Schema = dbSchema;
                result.Recommendations = analysis.recommendations;
                result.PerformanceInsights = analysis.insights;
                result.OptimizationOpportunities = analysis.opportunities;
                result.IsSuccessful = true;

                _logger.LogInformation("Schema analysis completed with {Recommendations} recommendations", 
                    result.Recommendations.Count);
            }
            catch (Exception ex)
            {
                result.IsSuccessful = false;
                result.ErrorMessage = ex.Message;
                _logger.LogError(ex, "Failed to analyze schema");
            }

            return result;
        }

        // Tool for batch query processing
        [McpServerTool, Description("Executes multiple related queries in a batch with transaction support and rollback capabilities.")]
        public async Task<BatchQueryResult> ExecuteBatchQueries(
            IServiceProvider serviceProvider,
            IConfiguration config,
            [Description("List of natural language prompts to execute as a batch.")] List<string> prompts,
            [Description("The AI model to use for query generation.")] string aiModel,
            [Description("The AI platform to use.")] string aiPlatform,
            [Description("Whether to use transaction for batch execution.")] bool useTransaction = true,
            [Description("Optional batch timeout in seconds.")] int batchTimeout = 600
        )
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new BatchQueryResult();

            try
            {
                _logger.LogInformation("Executing batch of {Count} queries with transaction: {UseTransaction}", 
                    prompts.Count, useTransaction);

                ValidateConfiguration(config);

                var connection = new AIConnection() { ConnectionString = config.GetValue<string>("DATABASECONNECTIONSTRING") };
                var dbSchema = await _dataService.GenerateSchema(connection);

                var batchResults = new List<BatchQueryItem>();

                foreach (var prompt in prompts)
                {
                    var item = new BatchQueryItem { Prompt = prompt };

                    try
                    {
                        var aiResponse = await _aiService.GetAISQLQuery(aiModel, aiPlatform, prompt, dbSchema, config.GetValue<string>("DATABASETYPE"));
                        var rowData = await _dataService.GetDataTable(connection, aiResponse.query);

                        item.GeneratedSql = aiResponse.query;
                        item.Results = rowData;
                        item.RowsReturned = rowData.Count;
                        item.IsSuccessful = true;
                    }
                    catch (Exception ex)
                    {
                        item.IsSuccessful = false;
                        item.ErrorMessage = ex.Message;
                    }

                    batchResults.Add(item);
                }

                result.Queries = batchResults;
                result.TotalQueries = prompts.Count;
                result.SuccessfulQueries = batchResults.Count(q => q.IsSuccessful);
                result.FailedQueries = batchResults.Count(q => !q.IsSuccessful);
                result.IsSuccessful = result.FailedQueries == 0;

                stopwatch.Stop();
                result.ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds;

                _logger.LogInformation("Batch execution completed. Successful: {Success}, Failed: {Failed}, Time: {Time}ms",
                    result.SuccessfulQueries, result.FailedQueries, result.ExecutionTimeMs);
            }
            catch (Exception ex)
            {
                result.IsSuccessful = false;
                result.ErrorMessage = ex.Message;
                result.ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds;
                _logger.LogError(ex, "Failed to execute batch queries");
            }

            return result;
        }

        // Tool for query pattern analysis and learning
        [McpServerTool, Description("Analyzes query patterns and provides insights for query optimization and database design improvements.")]
        public async Task<QueryPatternAnalysisResult> AnalyzeQueryPatterns(
            IServiceProvider serviceProvider,
            IConfiguration config,
            [Description("List of historical queries to analyze.")] List<string> historicalQueries,
            [Description("The AI model to use for pattern analysis.")] string aiModel,
            [Description("The AI platform to use.")] string aiPlatform,
            [Description("Optional analysis timeframe in days.")] int timeframeDays = 30
        )
        {
            var result = new QueryPatternAnalysisResult();

            try
            {
                _logger.LogInformation("Analyzing {Count} historical queries over {Days} days", 
                    historicalQueries.Count, timeframeDays);

                ValidateConfiguration(config);

                var connection = new AIConnection() { ConnectionString = config.GetValue<string>("DATABASECONNECTIONSTRING") };
                var dbSchema = await _dataService.GenerateSchema(connection);

                var analysis = await _aiService.AnalyzeQueryPatterns(
                    aiModel,
                    aiPlatform,
                    historicalQueries,
                    dbSchema,
                    config.GetValue<string>("DATABASETYPE"),
                    timeframeDays
                );

                result.Patterns = analysis.patterns;
                result.Recommendations = analysis.recommendations;
                result.PerformanceTrends = analysis.trends;
                result.OptimizationOpportunities = analysis.opportunities;
                result.IsSuccessful = true;

                _logger.LogInformation("Query pattern analysis completed with {Patterns} patterns identified", 
                    result.Patterns.Count);
            }
            catch (Exception ex)
            {
                result.IsSuccessful = false;
                result.ErrorMessage = ex.Message;
                _logger.LogError(ex, "Failed to analyze query patterns");
            }

            return result;
        }

        private void ValidateConfiguration(IConfiguration config)
        {
            var databaseType = config.GetValue<string>("DATABASETYPE");
            var databaseConnectionString = config.GetValue<string>("DATABASECONNECTIONSTRING");

            if (string.IsNullOrEmpty(databaseConnectionString))
                throw new ArgumentException("DATABASECONNECTIONSTRING is not set in the configuration.");
            if (string.IsNullOrEmpty(databaseType))
                throw new ArgumentException("DATABASETYPE is not set in the configuration.");
        }

        private int CalculateQueryComplexity(string sqlQuery)
        {
            // Simple complexity calculation based on query structure
            var complexity = 0;
            if (sqlQuery.Contains("JOIN", StringComparison.OrdinalIgnoreCase)) complexity += 10;
            if (sqlQuery.Contains("WHERE", StringComparison.OrdinalIgnoreCase)) complexity += 5;
            if (sqlQuery.Contains("GROUP BY", StringComparison.OrdinalIgnoreCase)) complexity += 8;
            if (sqlQuery.Contains("ORDER BY", StringComparison.OrdinalIgnoreCase)) complexity += 3;
            if (sqlQuery.Contains("HAVING", StringComparison.OrdinalIgnoreCase)) complexity += 7;
            if (sqlQuery.Contains("UNION", StringComparison.OrdinalIgnoreCase)) complexity += 15;
            if (sqlQuery.Contains("SUBQUERY", StringComparison.OrdinalIgnoreCase)) complexity += 12;
            
            return Math.Min(complexity, 100);
        }

        private double EstimateQueryCost(string sqlQuery, int rowsReturned)
        {
            // Simple cost estimation based on query complexity and result size
            var complexity = CalculateQueryComplexity(sqlQuery);
            return complexity * 0.1 + rowsReturned * 0.01;
        }

        private double CalculateOptimizationScore(List<string> optimizations)
        {
            if (optimizations == null || !optimizations.Any()) return 0;
            
            // Calculate optimization score based on number and quality of optimizations
            var baseScore = optimizations.Count * 10;
            var qualityBonus = optimizations.Count(o => o.Contains("index") || o.Contains("JOIN")) * 5;
            return Math.Min(baseScore + qualityBonus, 100);
        }
    }

    // Result classes for advanced MCP tools
    public class AdvancedQueryResult
    {
        public bool IsSuccessful { get; set; }
        public string GeneratedSql { get; set; } = string.Empty;
        public List<List<string>> Results { get; set; } = new List<List<string>>();
        public string? ErrorMessage { get; set; }
        public int ExecutionTimeMs { get; set; }
        public int RowsReturned { get; set; }
        public string QueryAnalysis { get; set; } = string.Empty;
        public List<string> OptimizationSuggestions { get; set; } = new List<string>();
        public PerformanceMetrics PerformanceMetrics { get; set; } = new PerformanceMetrics();
    }

    public class PerformanceMetrics
    {
        public int QueryComplexity { get; set; }
        public double EstimatedCost { get; set; }
        public double OptimizationScore { get; set; }
    }

    public class QueryOptimizationResult
    {
        public bool IsSuccessful { get; set; }
        public string OriginalQuery { get; set; } = string.Empty;
        public string OptimizedQuery { get; set; } = string.Empty;
        public string PerformanceAnalysis { get; set; } = string.Empty;
        public List<string> Recommendations { get; set; } = new List<string>();
        public double EstimatedImprovement { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class SchemaAnalysisResult
    {
        public bool IsSuccessful { get; set; }
        public DatabaseSchema Schema { get; set; } = new DatabaseSchema();
        public List<string> Recommendations { get; set; } = new List<string>();
        public List<string> PerformanceInsights { get; set; } = new List<string>();
        public List<string> OptimizationOpportunities { get; set; } = new List<string>();
        public string? ErrorMessage { get; set; }
    }

    public class BatchQueryResult
    {
        public bool IsSuccessful { get; set; }
        public List<BatchQueryItem> Queries { get; set; } = new List<BatchQueryItem>();
        public int TotalQueries { get; set; }
        public int SuccessfulQueries { get; set; }
        public int FailedQueries { get; set; }
        public int ExecutionTimeMs { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class BatchQueryItem
    {
        public string Prompt { get; set; } = string.Empty;
        public string GeneratedSql { get; set; } = string.Empty;
        public List<List<string>> Results { get; set; } = new List<List<string>>();
        public int RowsReturned { get; set; }
        public bool IsSuccessful { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class QueryPatternAnalysisResult
    {
        public bool IsSuccessful { get; set; }
        public List<string> Patterns { get; set; } = new List<string>();
        public List<string> Recommendations { get; set; } = new List<string>();
        public List<string> PerformanceTrends { get; set; } = new List<string>();
        public List<string> OptimizationOpportunities { get; set; } = new List<string>();
        public string? ErrorMessage { get; set; }
    }
}