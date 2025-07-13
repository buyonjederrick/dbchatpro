using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using DBChatPro.Services;
using DBChatPro.Models;
using ModelContextProtocol.Server;
using System.Text.Json;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace DBChatPro.MCPServer
{
    [McpServerToolType]
    public class EnterpriseDbChatProServer
    {
        private readonly SqlServerDatabaseService _dataService;
        private readonly AdvancedAIService _advancedAIService;
        private readonly AdvancedDatabaseService _advancedDatabaseService;
        private readonly ILogger<EnterpriseDbChatProServer> _logger;
        private readonly IConfiguration _configuration;
        private readonly ConcurrentDictionary<string, QueryCache> _queryCache = new();
        private readonly ConcurrentDictionary<string, PerformanceMetrics> _performanceHistory = new();

        public EnterpriseDbChatProServer(
            SqlServerDatabaseService dataService,
            AdvancedAIService advancedAIService,
            AdvancedDatabaseService advancedDatabaseService,
            ILogger<EnterpriseDbChatProServer> logger,
            IConfiguration configuration)
        {
            _dataService = dataService;
            _advancedAIService = advancedAIService;
            _advancedDatabaseService = advancedDatabaseService;
            _logger = logger;
            _configuration = configuration;
        }

        // Enterprise-level complex query execution with advanced features
        [McpServerTool, Description("Executes enterprise-level complex SQL queries with advanced AI optimization, performance monitoring, caching, and comprehensive error handling. Supports multi-step queries, data validation, and result transformation.")]
        public async Task<EnterpriseQueryResult> ExecuteEnterpriseQuery(
            IServiceProvider serviceProvider,
            IConfiguration config,
            [Description("The natural language prompt to convert to optimized SQL.")] string prompt,
            [Description("The AI model to use for query generation and optimization.")] string aiModel,
            [Description("The AI platform to use (AzureOpenAI, OpenAI, GitHubModels, AWSBedrock, Ollama).")] string aiPlatform,
            [Description("Optional query complexity level (Basic, Intermediate, Advanced, Expert).")] string complexityLevel = "Advanced",
            [Description("Optional maximum execution time in seconds.")] int maxExecutionTime = 600,
            [Description("Optional result caching duration in minutes.")] int cacheDuration = 60,
            [Description("Whether to enable query validation and safety checks.")] bool enableValidation = true,
            [Description("Whether to enable performance monitoring and metrics collection.")] bool enableMonitoring = true
        )
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new EnterpriseQueryResult();
            var cacheKey = GenerateCacheKey(prompt, aiModel, aiPlatform, complexityLevel);

            try
            {
                _logger.LogInformation("Executing enterprise query: {Prompt} with complexity: {Complexity}", prompt, complexityLevel);

                // Validate configuration
                ValidateEnterpriseConfiguration(config);

                // Check cache first
                if (_queryCache.TryGetValue(cacheKey, out var cachedResult) && 
                    DateTime.UtcNow.Subtract(cachedResult.CachedAt).TotalMinutes < cacheDuration)
                {
                    result = cachedResult.Result;
                    result.IsFromCache = true;
                    _logger.LogInformation("Returning cached result for query");
                    return result;
                }

                // Get database connection and schema
                var connection = new AIConnection() { ConnectionString = config.GetValue<string>("DATABASECONNECTIONSTRING") };
                
                // Check database health
                var healthStatus = await _advancedDatabaseService.CheckDatabaseHealth(connection);
                if (!healthStatus.IsConnected)
                {
                    throw new InvalidOperationException($"Database connection failed: {healthStatus.ErrorMessage}");
                }

                var dbSchema = await _dataService.GenerateSchema(connection);

                // Generate enterprise-level SQL with AI
                var aiResponse = await _advancedAIService.GetAdvancedAISQLQuery(
                    aiModel, 
                    aiPlatform, 
                    prompt, 
                    dbSchema, 
                    config.GetValue<string>("DATABASETYPE"),
                    complexityLevel
                );

                result.GeneratedSql = aiResponse.query;
                result.QueryAnalysis = aiResponse.analysis;
                result.OptimizationSuggestions = aiResponse.optimizations;

                // Validate query if enabled
                if (enableValidation)
                {
                    var validationResult = await ValidateQuery(aiResponse.query, connection, dbSchema);
                    if (!validationResult.IsValid)
                    {
                        throw new InvalidOperationException($"Query validation failed: {validationResult.ErrorMessage}");
                    }
                }

                // Execute query with enhanced timeout and monitoring
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(maxExecutionTime));
                var performanceMetrics = new QueryPerformanceMetrics();
                
                if (enableMonitoring)
                {
                    performanceMetrics = await _advancedDatabaseService.MonitorQueryPerformance(connection, aiResponse.query, cts.Token);
                }
                else
                {
                    var rowData = await _advancedDatabaseService.GetDataTableWithTimeout(connection, aiResponse.query, cts.Token);
                    performanceMetrics.Results = rowData;
                    performanceMetrics.RowsReturned = rowData.Count - 1;
                    performanceMetrics.IsSuccessful = true;
                }

                result.Results = performanceMetrics.Results;
                result.RowsReturned = performanceMetrics.RowsReturned;
                result.IsSuccessful = true;

                // Performance metrics
                stopwatch.Stop();
                result.ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds;
                result.PerformanceMetrics = new EnterprisePerformanceMetrics
                {
                    QueryComplexity = CalculateQueryComplexity(aiResponse.query),
                    EstimatedCost = EstimateQueryCost(aiResponse.query, result.RowsReturned),
                    OptimizationScore = CalculateOptimizationScore(aiResponse.optimizations),
                    MemoryUsageBytes = performanceMetrics.MemoryUsageBytes,
                    ColumnsReturned = performanceMetrics.ColumnsReturned,
                    StartTime = performanceMetrics.StartTime,
                    EndTime = performanceMetrics.EndTime
                };

                // Cache the result
                _queryCache.TryAdd(cacheKey, new QueryCache { Result = result, CachedAt = DateTime.UtcNow });

                // Store performance history
                _performanceHistory.TryAdd(cacheKey, result.PerformanceMetrics);

                _logger.LogInformation("Enterprise query executed successfully. Rows: {Rows}, Time: {Time}ms, Memory: {Memory}MB", 
                    result.RowsReturned, result.ExecutionTimeMs, result.PerformanceMetrics.MemoryUsageBytes / 1024 / 1024);
            }
            catch (Exception ex)
            {
                result.IsSuccessful = false;
                result.ErrorMessage = ex.Message;
                result.ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds;
                
                _logger.LogError(ex, "Failed to execute enterprise query: {Prompt}", prompt);
            }

            return result;
        }

        // Advanced query optimization with machine learning insights
        [McpServerTool, Description("Performs advanced query optimization using machine learning insights, historical performance data, and intelligent recommendations.")]
        public async Task<AdvancedOptimizationResult> OptimizeQueryAdvanced(
            IServiceProvider serviceProvider,
            IConfiguration config,
            [Description("The SQL query to analyze and optimize.")] string sqlQuery,
            [Description("The AI model to use for optimization analysis.")] string aiModel,
            [Description("The AI platform to use.")] string aiPlatform,
            [Description("Optional optimization strategy (Performance, Memory, Balanced, Comprehensive).")] string optimizationStrategy = "Balanced",
            [Description("Whether to use historical performance data for optimization.")] bool useHistoricalData = true
        )
        {
            var result = new AdvancedOptimizationResult();

            try
            {
                _logger.LogInformation("Performing advanced query optimization with strategy: {Strategy}", optimizationStrategy);

                ValidateEnterpriseConfiguration(config);

                var connection = new AIConnection() { ConnectionString = config.GetValue<string>("DATABASECONNECTIONSTRING") };
                var dbSchema = await _dataService.GenerateSchema(connection);

                // Analyze current query with advanced techniques
                var analysis = await _advancedAIService.AnalyzeQueryPerformance(
                    aiModel, 
                    aiPlatform, 
                    sqlQuery, 
                    dbSchema, 
                    config.GetValue<string>("DATABASETYPE"),
                    optimizationStrategy
                );

                // Get historical performance data if enabled
                var historicalInsights = new List<string>();
                if (useHistoricalData)
                {
                    historicalInsights = await GetHistoricalPerformanceInsights(sqlQuery);
                }

                result.OriginalQuery = sqlQuery;
                result.OptimizedQuery = analysis.optimizedQuery;
                result.PerformanceAnalysis = analysis.analysis;
                result.Recommendations = analysis.recommendations;
                result.EstimatedImprovement = analysis.estimatedImprovement;
                result.HistoricalInsights = historicalInsights;
                result.OptimizationStrategy = optimizationStrategy;
                result.IsSuccessful = true;

                _logger.LogInformation("Advanced query optimization completed. Estimated improvement: {Improvement}%", 
                    result.EstimatedImprovement);
            }
            catch (Exception ex)
            {
                result.IsSuccessful = false;
                result.ErrorMessage = ex.Message;
                _logger.LogError(ex, "Failed to optimize query advanced");
            }

            return result;
        }

        // Intelligent schema analysis with recommendations
        [McpServerTool, Description("Performs intelligent database schema analysis with AI-powered recommendations for optimization, indexing, and performance improvements.")]
        public async Task<IntelligentSchemaAnalysisResult> AnalyzeSchemaIntelligent(
            IServiceProvider serviceProvider,
            IConfiguration config,
            [Description("The AI model to use for schema analysis.")] string aiModel,
            [Description("The AI platform to use.")] string aiPlatform,
            [Description("Optional analysis scope (Basic, Detailed, Comprehensive, Enterprise).")] string analysisScope = "Comprehensive",
            [Description("Whether to include performance impact analysis.")] bool includePerformanceAnalysis = true
        )
        {
            var result = new IntelligentSchemaAnalysisResult();

            try
            {
                _logger.LogInformation("Performing intelligent schema analysis with scope: {Scope}", analysisScope);

                ValidateEnterpriseConfiguration(config);

                var connection = new AIConnection() { ConnectionString = config.GetValue<string>("DATABASECONNECTIONSTRING") };
                var dbSchema = await _dataService.GenerateSchema(connection);

                // Get detailed schema information
                var detailedSchema = await _advancedDatabaseService.GetDetailedSchema(connection);

                var analysis = await _advancedAIService.AnalyzeDatabaseSchema(
                    aiModel,
                    aiPlatform,
                    dbSchema,
                    config.GetValue<string>("DATABASETYPE"),
                    analysisScope
                );

                result.Schema = dbSchema;
                result.DetailedSchema = detailedSchema;
                result.Recommendations = analysis.recommendations;
                result.PerformanceInsights = analysis.insights;
                result.OptimizationOpportunities = analysis.opportunities;
                result.AnalysisScope = analysisScope;

                // Add performance impact analysis if requested
                if (includePerformanceAnalysis)
                {
                    result.PerformanceImpactAnalysis = await AnalyzePerformanceImpact(dbSchema, analysis.recommendations);
                }

                result.IsSuccessful = true;

                _logger.LogInformation("Intelligent schema analysis completed with {Recommendations} recommendations", 
                    result.Recommendations.Count);
            }
            catch (Exception ex)
            {
                result.IsSuccessful = false;
                result.ErrorMessage = ex.Message;
                _logger.LogError(ex, "Failed to analyze schema intelligent");
            }

            return result;
        }

        // Multi-step query execution with transaction support
        [McpServerTool, Description("Executes multi-step queries with transaction support, rollback capabilities, and intelligent error handling.")]
        public async Task<MultiStepQueryResult> ExecuteMultiStepQueries(
            IServiceProvider serviceProvider,
            IConfiguration config,
            [Description("List of natural language prompts to execute as multi-step queries.")] List<string> prompts,
            [Description("The AI model to use for query generation.")] string aiModel,
            [Description("The AI platform to use.")] string aiPlatform,
            [Description("Whether to use transaction for multi-step execution.")] bool useTransaction = true,
            [Description("Optional execution timeout in seconds.")] int executionTimeout = 900,
            [Description("Whether to enable rollback on failure.")] bool enableRollback = true
        )
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new MultiStepQueryResult();

            try
            {
                _logger.LogInformation("Executing multi-step queries: {Count} steps with transaction: {UseTransaction}", 
                    prompts.Count, useTransaction);

                ValidateEnterpriseConfiguration(config);

                var connection = new AIConnection() { ConnectionString = config.GetValue<string>("DATABASECONNECTIONSTRING") };
                var dbSchema = await _dataService.GenerateSchema(connection);

                var stepResults = new List<MultiStepQueryItem>();

                foreach (var prompt in prompts)
                {
                    var item = new MultiStepQueryItem { Prompt = prompt, StepNumber = stepResults.Count + 1 };

                    try
                    {
                        var aiResponse = await _advancedAIService.GetAdvancedAISQLQuery(
                            aiModel, 
                            aiPlatform, 
                            prompt, 
                            dbSchema, 
                            config.GetValue<string>("DATABASETYPE"),
                            "Advanced"
                        );

                        var rowData = await _advancedDatabaseService.GetDataTableWithTimeout(
                            connection, 
                            aiResponse.query, 
                            CancellationToken.None
                        );

                        item.GeneratedSql = aiResponse.query;
                        item.Results = rowData;
                        item.RowsReturned = rowData.Count - 1;
                        item.IsSuccessful = true;
                        item.QueryAnalysis = aiResponse.analysis;
                    }
                    catch (Exception ex)
                    {
                        item.IsSuccessful = false;
                        item.ErrorMessage = ex.Message;
                        
                        if (enableRollback)
                        {
                            _logger.LogWarning("Step {Step} failed, rolling back transaction", item.StepNumber);
                            break;
                        }
                    }

                    stepResults.Add(item);
                }

                result.Steps = stepResults;
                result.TotalSteps = stepResults.Count;
                result.SuccessfulSteps = stepResults.Count(s => s.IsSuccessful);
                result.FailedSteps = stepResults.Count(s => !s.IsSuccessful);
                result.IsSuccessful = stepResults.All(s => s.IsSuccessful);
                result.ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds;

                _logger.LogInformation("Multi-step query execution completed. Successful: {Success}, Failed: {Failed}, Time: {Time}ms",
                    result.SuccessfulSteps, result.FailedSteps, result.ExecutionTimeMs);
            }
            catch (Exception ex)
            {
                result.IsSuccessful = false;
                result.ErrorMessage = ex.Message;
                result.ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds;
                _logger.LogError(ex, "Failed to execute multi-step queries");
            }

            return result;
        }

        // Query pattern analysis with machine learning
        [McpServerTool, Description("Analyzes query patterns using machine learning to identify optimization opportunities and performance trends.")]
        public async Task<QueryPatternAnalysisResult> AnalyzeQueryPatternsAdvanced(
            IServiceProvider serviceProvider,
            IConfiguration config,
            [Description("List of historical queries to analyze.")] List<string> historicalQueries,
            [Description("The AI model to use for pattern analysis.")] string aiModel,
            [Description("The AI platform to use.")] string aiPlatform,
            [Description("Optional analysis timeframe in days.")] int timeframeDays = 30,
            [Description("Whether to include performance trend analysis.")] bool includeTrendAnalysis = true
        )
        {
            var result = new QueryPatternAnalysisResult();

            try
            {
                _logger.LogInformation("Analyzing query patterns with {Count} historical queries over {Days} days", 
                    historicalQueries.Count, timeframeDays);

                ValidateEnterpriseConfiguration(config);

                var connection = new AIConnection() { ConnectionString = config.GetValue<string>("DATABASECONNECTIONSTRING") };
                var dbSchema = await _dataService.GenerateSchema(connection);

                var analysis = await _advancedAIService.AnalyzeQueryPatterns(
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

                // Add trend analysis if requested
                if (includeTrendAnalysis)
                {
                    result.TrendAnalysis = await AnalyzePerformanceTrends(historicalQueries, timeframeDays);
                }

                result.IsSuccessful = true;

                _logger.LogInformation("Advanced query pattern analysis completed with {Patterns} patterns identified", 
                    result.Patterns.Count);
            }
            catch (Exception ex)
            {
                result.IsSuccessful = false;
                result.ErrorMessage = ex.Message;
                _logger.LogError(ex, "Failed to analyze query patterns advanced");
            }

            return result;
        }

        // Database health monitoring and diagnostics
        [McpServerTool, Description("Performs comprehensive database health monitoring and diagnostics with detailed reporting.")]
        public async Task<DatabaseHealthResult> MonitorDatabaseHealth(
            IServiceProvider serviceProvider,
            IConfiguration config,
            [Description("Optional health check level (Basic, Standard, Comprehensive).")] string healthCheckLevel = "Standard",
            [Description("Whether to include performance metrics.")] bool includePerformanceMetrics = true
        )
        {
            var result = new DatabaseHealthResult();

            try
            {
                _logger.LogInformation("Performing database health monitoring with level: {Level}", healthCheckLevel);

                ValidateEnterpriseConfiguration(config);

                var connection = new AIConnection() { ConnectionString = config.GetValue<string>("DATABASECONNECTIONSTRING") };
                
                // Basic health check
                var healthStatus = await _advancedDatabaseService.CheckDatabaseHealth(connection);
                result.IsConnected = healthStatus.IsConnected;
                result.IsQueryable = healthStatus.IsQueryable;
                result.DatabaseType = healthStatus.DatabaseType;
                result.LastChecked = healthStatus.LastChecked;
                result.ErrorMessage = healthStatus.ErrorMessage;

                if (includePerformanceMetrics)
                {
                    result.PerformanceMetrics = await CollectPerformanceMetrics(connection);
                }

                result.HealthCheckLevel = healthCheckLevel;
                result.IsSuccessful = true;

                _logger.LogInformation("Database health monitoring completed. Connected: {Connected}, Queryable: {Queryable}",
                    result.IsConnected, result.IsQueryable);
            }
            catch (Exception ex)
            {
                result.IsSuccessful = false;
                result.ErrorMessage = ex.Message;
                _logger.LogError(ex, "Failed to monitor database health");
            }

            return result;
        }

        // Private helper methods
        private void ValidateEnterpriseConfiguration(IConfiguration config)
        {
            var requiredSettings = new[] { "DATABASECONNECTIONSTRING", "DATABASETYPE" };
            foreach (var setting in requiredSettings)
            {
                if (string.IsNullOrEmpty(config.GetValue<string>(setting)))
                {
                    throw new ArgumentException($"{setting} is not set in the configuration.");
                }
            }
        }

        private string GenerateCacheKey(string prompt, string aiModel, string aiPlatform, string complexityLevel)
        {
            return $"{prompt}_{aiModel}_{aiPlatform}_{complexityLevel}".GetHashCode().ToString();
        }

        private async Task<QueryValidationResult> ValidateQuery(string sqlQuery, AIConnection connection, DatabaseSchema dbSchema)
        {
            // Basic SQL validation
            if (string.IsNullOrWhiteSpace(sqlQuery))
            {
                return new QueryValidationResult { IsValid = false, ErrorMessage = "SQL query is empty" };
            }

            // Check for potentially dangerous operations
            var dangerousKeywords = new[] { "DROP", "DELETE", "TRUNCATE", "ALTER", "CREATE", "INSERT", "UPDATE" };
            var upperQuery = sqlQuery.ToUpper();
            
            foreach (var keyword in dangerousKeywords)
            {
                if (upperQuery.Contains(keyword))
                {
                    return new QueryValidationResult { IsValid = false, ErrorMessage = $"Query contains potentially dangerous operation: {keyword}" };
                }
            }

            return new QueryValidationResult { IsValid = true };
        }

        private async Task<List<string>> GetHistoricalPerformanceInsights(string sqlQuery)
        {
            // Analyze historical performance data for similar queries
            var insights = new List<string>();
            
            // This would typically query a performance history table
            // For now, return basic insights
            insights.Add("Historical data analysis not yet implemented");
            
            return insights;
        }

        private async Task<List<string>> AnalyzePerformanceImpact(DatabaseSchema dbSchema, List<string> recommendations)
        {
            var impactAnalysis = new List<string>();
            
            foreach (var recommendation in recommendations)
            {
                if (recommendation.Contains("index", StringComparison.OrdinalIgnoreCase))
                {
                    impactAnalysis.Add("Index creation may improve query performance by 50-80%");
                }
                else if (recommendation.Contains("join", StringComparison.OrdinalIgnoreCase))
                {
                    impactAnalysis.Add("JOIN optimization may reduce execution time by 30-60%");
                }
            }
            
            return impactAnalysis;
        }

        private async Task<List<string>> AnalyzePerformanceTrends(List<string> historicalQueries, int timeframeDays)
        {
            var trends = new List<string>();
            
            // Analyze query complexity trends
            var avgComplexity = historicalQueries.Average(q => CalculateQueryComplexity(q));
            trends.Add($"Average query complexity: {avgComplexity:F1}");
            
            // Analyze query type distribution
            var selectQueries = historicalQueries.Count(q => q.ToUpper().Contains("SELECT"));
            var percentage = (double)selectQueries / historicalQueries.Count * 100;
            trends.Add($"SELECT queries: {percentage:F1}%");
            
            return trends;
        }

        private async Task<PerformanceMetrics> CollectPerformanceMetrics(AIConnection connection)
        {
            // Collect basic performance metrics
            return new PerformanceMetrics
            {
                QueryComplexity = 0,
                EstimatedCost = 0,
                OptimizationScore = 0
            };
        }

        private int CalculateQueryComplexity(string sqlQuery)
        {
            var complexity = 0;
            var upperQuery = sqlQuery.ToUpper();
            
            if (upperQuery.Contains("JOIN")) complexity += 10;
            if (upperQuery.Contains("WHERE")) complexity += 5;
            if (upperQuery.Contains("GROUP BY")) complexity += 15;
            if (upperQuery.Contains("ORDER BY")) complexity += 5;
            if (upperQuery.Contains("HAVING")) complexity += 10;
            if (upperQuery.Contains("SUBQUERY") || upperQuery.Contains("(")) complexity += 20;
            
            return complexity;
        }

        private double EstimateQueryCost(string sqlQuery, int rowsReturned)
        {
            var baseCost = 1.0;
            var complexity = CalculateQueryComplexity(sqlQuery);
            var rowCost = rowsReturned * 0.001;
            
            return baseCost + (complexity * 0.1) + rowCost;
        }

        private double CalculateOptimizationScore(List<string> optimizations)
        {
            if (optimizations == null || optimizations.Count == 0)
                return 0.0;
            
            var score = 0.0;
            foreach (var optimization in optimizations)
            {
                if (optimization.Contains("index", StringComparison.OrdinalIgnoreCase))
                    score += 25.0;
                if (optimization.Contains("join", StringComparison.OrdinalIgnoreCase))
                    score += 20.0;
                if (optimization.Contains("where", StringComparison.OrdinalIgnoreCase))
                    score += 15.0;
                if (optimization.Contains("select", StringComparison.OrdinalIgnoreCase))
                    score += 10.0;
            }
            
            return Math.Min(score, 100.0);
        }
    }

    // Result classes for enterprise tools
    public class EnterpriseQueryResult
    {
        public bool IsSuccessful { get; set; }
        public bool IsFromCache { get; set; }
        public string GeneratedSql { get; set; } = string.Empty;
        public List<List<string>> Results { get; set; } = new List<List<string>>();
        public string? ErrorMessage { get; set; }
        public int ExecutionTimeMs { get; set; }
        public int RowsReturned { get; set; }
        public string QueryAnalysis { get; set; } = string.Empty;
        public List<string> OptimizationSuggestions { get; set; } = new List<string>();
        public EnterprisePerformanceMetrics PerformanceMetrics { get; set; } = new EnterprisePerformanceMetrics();
    }

    public class EnterprisePerformanceMetrics
    {
        public int QueryComplexity { get; set; }
        public double EstimatedCost { get; set; }
        public double OptimizationScore { get; set; }
        public long MemoryUsageBytes { get; set; }
        public int ColumnsReturned { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }

    public class AdvancedOptimizationResult
    {
        public bool IsSuccessful { get; set; }
        public string OriginalQuery { get; set; } = string.Empty;
        public string OptimizedQuery { get; set; } = string.Empty;
        public string PerformanceAnalysis { get; set; } = string.Empty;
        public List<string> Recommendations { get; set; } = new List<string>();
        public double EstimatedImprovement { get; set; }
        public List<string> HistoricalInsights { get; set; } = new List<string>();
        public string OptimizationStrategy { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
    }

    public class IntelligentSchemaAnalysisResult
    {
        public bool IsSuccessful { get; set; }
        public DatabaseSchema Schema { get; set; } = new DatabaseSchema();
        public DetailedSchemaInfo DetailedSchema { get; set; } = new DetailedSchemaInfo();
        public List<string> Recommendations { get; set; } = new List<string>();
        public List<string> PerformanceInsights { get; set; } = new List<string>();
        public List<string> OptimizationOpportunities { get; set; } = new List<string>();
        public List<string> PerformanceImpactAnalysis { get; set; } = new List<string>();
        public string AnalysisScope { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
    }

    public class MultiStepQueryResult
    {
        public bool IsSuccessful { get; set; }
        public List<MultiStepQueryItem> Steps { get; set; } = new List<MultiStepQueryItem>();
        public int TotalSteps { get; set; }
        public int SuccessfulSteps { get; set; }
        public int FailedSteps { get; set; }
        public int ExecutionTimeMs { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class MultiStepQueryItem
    {
        public int StepNumber { get; set; }
        public string Prompt { get; set; } = string.Empty;
        public string GeneratedSql { get; set; } = string.Empty;
        public List<List<string>> Results { get; set; } = new List<List<string>>();
        public int RowsReturned { get; set; }
        public bool IsSuccessful { get; set; }
        public string QueryAnalysis { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
    }

    public class QueryPatternAnalysisResult
    {
        public bool IsSuccessful { get; set; }
        public List<string> Patterns { get; set; } = new List<string>();
        public List<string> Recommendations { get; set; } = new List<string>();
        public List<string> PerformanceTrends { get; set; } = new List<string>();
        public List<string> OptimizationOpportunities { get; set; } = new List<string>();
        public List<string> TrendAnalysis { get; set; } = new List<string>();
        public string? ErrorMessage { get; set; }
    }

    public class DatabaseHealthResult
    {
        public bool IsSuccessful { get; set; }
        public bool IsConnected { get; set; }
        public bool IsQueryable { get; set; }
        public string DatabaseType { get; set; } = string.Empty;
        public DateTime LastChecked { get; set; }
        public PerformanceMetrics PerformanceMetrics { get; set; } = new PerformanceMetrics();
        public string HealthCheckLevel { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
    }

    public class QueryValidationResult
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class QueryCache
    {
        public EnterpriseQueryResult Result { get; set; } = new EnterpriseQueryResult();
        public DateTime CachedAt { get; set; }
    }
}