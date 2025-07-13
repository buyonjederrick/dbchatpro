using DBChatPro.Models;
using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;
using Npgsql;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Diagnostics;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace DBChatPro.Services
{
    public class EnterpriseDatabaseService : AdvancedDatabaseService
    {
        private readonly ILogger<EnterpriseDatabaseService> _logger;
        private readonly ConcurrentDictionary<string, ConnectionPool> _connectionPools = new();
        private readonly ConcurrentDictionary<string, QueryExecutionHistory> _executionHistory = new();
        private readonly ConcurrentDictionary<string, PerformanceBaseline> _performanceBaselines = new();

        public EnterpriseDatabaseService(ILogger<EnterpriseDatabaseService> logger) : base(logger)
        {
            _logger = logger;
        }

        // Enterprise-level query execution with comprehensive monitoring
        public async Task<EnterpriseQueryExecutionResult> ExecuteQueryEnterprise(
            AIConnection connection, 
            string query, 
            CancellationToken cancellationToken = default,
            bool enableMonitoring = true,
            bool enableCaching = true,
            int timeoutSeconds = 600)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new EnterpriseQueryExecutionResult();
            var cacheKey = GenerateQueryCacheKey(query, connection.ConnectionString);

            try
            {
                _logger.LogInformation("Executing enterprise query with monitoring: {EnableMonitoring}, caching: {EnableCaching}", 
                    enableMonitoring, enableCaching);

                // Check cache first
                if (enableCaching && _executionHistory.TryGetValue(cacheKey, out var cachedHistory) && 
                    DateTime.UtcNow.Subtract(cachedHistory.LastExecuted).TotalMinutes < 30)
                {
                    result.Results = cachedHistory.Results;
                    result.RowsReturned = cachedHistory.RowsReturned;
                    result.IsFromCache = true;
                    result.IsSuccessful = true;
                    _logger.LogInformation("Returning cached query result");
                    return result;
                }

                // Validate connection health
                var healthStatus = await CheckDatabaseHealth(connection);
                if (!healthStatus.IsConnected)
                {
                    throw new InvalidOperationException($"Database connection failed: {healthStatus.ErrorMessage}");
                }

                // Execute query with enhanced monitoring
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
                var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token);

                var performanceMetrics = new EnterprisePerformanceMetrics();
                
                if (enableMonitoring)
                {
                    performanceMetrics = await MonitorQueryPerformanceEnterprise(connection, query, combinedCts.Token);
                    result.Results = performanceMetrics.Results;
                    result.RowsReturned = performanceMetrics.RowsReturned;
                }
                else
                {
                    var rowData = await GetDataTableWithTimeout(connection, query, combinedCts.Token);
                    result.Results = rowData;
                    result.RowsReturned = rowData.Count - 1;
                }

                result.IsSuccessful = true;

                // Performance metrics
                stopwatch.Stop();
                result.ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds;
                result.PerformanceMetrics = performanceMetrics;

                // Cache the result
                if (enableCaching)
                {
                    _executionHistory.TryAdd(cacheKey, new QueryExecutionHistory
                    {
                        Results = result.Results,
                        RowsReturned = result.RowsReturned,
                        LastExecuted = DateTime.UtcNow,
                        ExecutionTimeMs = result.ExecutionTimeMs
                    });
                }

                // Update performance baseline
                UpdatePerformanceBaseline(query, result.ExecutionTimeMs, result.RowsReturned);

                _logger.LogInformation("Enterprise query executed successfully. Rows: {Rows}, Time: {Time}ms, Memory: {Memory}MB", 
                    result.RowsReturned, result.ExecutionTimeMs, result.PerformanceMetrics.MemoryUsageBytes / 1024 / 1024);
            }
            catch (Exception ex)
            {
                result.IsSuccessful = false;
                result.ErrorMessage = ex.Message;
                result.ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds;
                
                _logger.LogError(ex, "Failed to execute enterprise query");
            }

            return result;
        }

        // Advanced batch execution with transaction management
        public async Task<EnterpriseBatchExecutionResult> ExecuteBatchEnterprise(
            AIConnection connection, 
            List<string> queries, 
            bool useTransaction = true,
            bool enableRollback = true,
            CancellationToken cancellationToken = default)
        {
            var result = new EnterpriseBatchExecutionResult();
            var stopwatch = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("Executing enterprise batch of {Count} queries with transaction: {UseTransaction}", 
                    queries.Count, useTransaction);

                // Validate connection health
                var healthStatus = await CheckDatabaseHealth(connection);
                if (!healthStatus.IsConnected)
                {
                    throw new InvalidOperationException($"Database connection failed: {healthStatus.ErrorMessage}");
                }

                using var dbConnection = CreateConnection(connection.ConnectionString, connection.DatabaseType);
                await dbConnection.OpenAsync(cancellationToken);

                IDbTransaction? transaction = null;
                if (useTransaction)
                {
                    transaction = await dbConnection.BeginTransactionAsync(cancellationToken);
                }

                try
                {
                    var stepResults = new List<EnterpriseBatchStep>();

                    foreach (var query in queries)
                    {
                        var step = new EnterpriseBatchStep { Query = query, StepNumber = stepResults.Count + 1 };

                        try
                        {
                            using var command = CreateCommand(dbConnection, query, connection.DatabaseType);
                            if (transaction != null)
                            {
                                command.Transaction = transaction;
                            }

                            var rowData = new List<List<string>>();
                            using var reader = await command.ExecuteReaderAsync(cancellationToken);
                            
                            // Get headers
                            var headers = new List<string>();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                headers.Add(reader.GetName(i));
                            }
                            rowData.Add(headers);

                            // Get data
                            while (await reader.ReadAsync(cancellationToken))
                            {
                                var row = new List<string>();
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    var value = reader.IsDBNull(i) ? "NULL" : reader.GetValue(i)?.ToString() ?? "NULL";
                                    row.Add(value);
                                }
                                rowData.Add(row);
                            }

                            step.Results = rowData;
                            step.RowsReturned = rowData.Count - 1;
                            step.IsSuccessful = true;
                        }
                        catch (Exception ex)
                        {
                            step.IsSuccessful = false;
                            step.ErrorMessage = ex.Message;
                            _logger.LogError(ex, "Failed to execute query in enterprise batch: {Query}", query);
                            
                            if (enableRollback)
                            {
                                _logger.LogWarning("Step {Step} failed, rolling back transaction", step.StepNumber);
                                break;
                            }
                        }

                        stepResults.Add(step);
                    }

                    if (transaction != null && stepResults.All(s => s.IsSuccessful))
                    {
                        await transaction.CommitAsync(cancellationToken);
                    }
                    else if (transaction != null)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                    }

                    result.Steps = stepResults;
                    result.TotalSteps = stepResults.Count;
                    result.SuccessfulSteps = stepResults.Count(s => s.IsSuccessful);
                    result.FailedSteps = stepResults.Count(s => !s.IsSuccessful);
                    result.IsSuccessful = stepResults.All(s => s.IsSuccessful);
                }
                catch (Exception ex)
                {
                    if (transaction != null)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                    }
                    throw;
                }
                finally
                {
                    transaction?.Dispose();
                }

                stopwatch.Stop();
                result.ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds;

                _logger.LogInformation("Enterprise batch execution completed. Successful: {Success}, Failed: {Failed}, Time: {Time}ms",
                    result.SuccessfulSteps, result.FailedSteps, result.ExecutionTimeMs);
            }
            catch (Exception ex)
            {
                result.IsSuccessful = false;
                result.ErrorMessage = ex.Message;
                result.ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds;
                _logger.LogError(ex, "Failed to execute enterprise batch");
            }

            return result;
        }

        // Enhanced performance monitoring with detailed metrics
        public async Task<EnterprisePerformanceMetrics> MonitorQueryPerformanceEnterprise(
            AIConnection connection, 
            string query, 
            CancellationToken cancellationToken = default)
        {
            var metrics = new EnterprisePerformanceMetrics();
            var stopwatch = Stopwatch.StartNew();

            try
            {
                using var dbConnection = CreateConnection(connection.ConnectionString, connection.DatabaseType);
                await dbConnection.OpenAsync(cancellationToken);

                using var command = CreateCommand(dbConnection, query, connection.DatabaseType);
                
                // Record start time and memory usage
                var startTime = DateTime.UtcNow;
                var startMemory = GC.GetTotalMemory(false);

                using var reader = await command.ExecuteReaderAsync(cancellationToken);
                
                var rowCount = 0;
                var columnCount = 0;
                var headers = new List<string>();

                // Read headers
                columnCount = reader.FieldCount;
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    headers.Add(reader.GetName(i));
                }
                metrics.Results.Add(headers);

                // Read data
                while (await reader.ReadAsync(cancellationToken))
                {
                    var row = new List<string>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        var value = reader.IsDBNull(i) ? "NULL" : reader.GetValue(i)?.ToString() ?? "NULL";
                        row.Add(value);
                    }
                    metrics.Results.Add(row);
                    rowCount++;
                }

                stopwatch.Stop();
                var endTime = DateTime.UtcNow;
                var endMemory = GC.GetTotalMemory(false);

                metrics.IsSuccessful = true;
                metrics.ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds;
                metrics.RowsReturned = rowCount;
                metrics.ColumnsReturned = columnCount;
                metrics.MemoryUsageBytes = endMemory - startMemory;
                metrics.StartTime = startTime;
                metrics.EndTime = endTime;

                // Calculate additional metrics
                metrics.QueryComplexity = CalculateQueryComplexity(query);
                metrics.EstimatedCost = EstimateQueryCost(query, rowCount);
                metrics.PerformanceScore = CalculatePerformanceScore(metrics);

                _logger.LogInformation("Enterprise performance monitoring completed. Rows: {Rows}, Time: {Time}ms, Memory: {Memory}MB",
                    rowCount, metrics.ExecutionTimeMs, metrics.MemoryUsageBytes / 1024 / 1024);
            }
            catch (Exception ex)
            {
                metrics.IsSuccessful = false;
                metrics.ErrorMessage = ex.Message;
                metrics.ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds;
                _logger.LogError(ex, "Failed to monitor query performance enterprise");
            }

            return metrics;
        }

        // Advanced connection pooling with health monitoring
        public async Task<object> GetEnterprisePooledConnection(string connectionString, string databaseType)
        {
            var poolKey = $"{connectionString}_{databaseType}";

            lock (_connectionPools)
            {
                if (_connectionPools.ContainsKey(poolKey))
                {
                    var pool = _connectionPools[poolKey];
                    if (IsConnectionHealthy(pool.Connection, databaseType))
                    {
                        pool.LastUsed = DateTime.UtcNow;
                        pool.UseCount++;
                        return pool.Connection;
                    }
                    else
                    {
                        _connectionPools.TryRemove(poolKey, out _);
                    }
                }
            }

            // Create new connection
            var newConnection = CreateConnection(connectionString, databaseType);
            var newPool = new ConnectionPool
            {
                Connection = newConnection,
                CreatedAt = DateTime.UtcNow,
                LastUsed = DateTime.UtcNow,
                UseCount = 1
            };
            
            lock (_connectionPools)
            {
                _connectionPools[poolKey] = newPool;
            }

            return newConnection;
        }

        // Comprehensive database health monitoring
        public async Task<EnterpriseHealthStatus> MonitorDatabaseHealthEnterprise(AIConnection connection)
        {
            var result = new EnterpriseHealthStatus();

            try
            {
                // Basic health check
                var basicHealth = await CheckDatabaseHealth(connection);
                result.IsConnected = basicHealth.IsConnected;
                result.IsQueryable = basicHealth.IsQueryable;
                result.DatabaseType = basicHealth.DatabaseType;
                result.LastChecked = basicHealth.LastChecked;
                result.ErrorMessage = basicHealth.ErrorMessage;

                if (result.IsConnected)
                {
                    // Get detailed schema information
                    result.DetailedSchema = await GetDetailedSchema(connection);

                    // Check performance metrics
                    result.PerformanceMetrics = await CollectPerformanceMetrics(connection);

                    // Check connection pool status
                    result.ConnectionPoolStatus = GetConnectionPoolStatus();

                    // Check query execution history
                    result.QueryHistoryStatus = GetQueryHistoryStatus();
                }

                result.IsSuccessful = true;

                _logger.LogInformation("Enterprise database health monitoring completed. Connected: {Connected}, Queryable: {Queryable}",
                    result.IsConnected, result.IsQueryable);
            }
            catch (Exception ex)
            {
                result.IsSuccessful = false;
                result.ErrorMessage = ex.Message;
                _logger.LogError(ex, "Failed to monitor database health enterprise");
            }

            return result;
        }

        // Query optimization with historical data
        public async Task<QueryOptimizationResult> OptimizeQueryWithHistory(
            AIConnection connection,
            string query,
            bool useHistoricalData = true)
        {
            var result = new QueryOptimizationResult();

            try
            {
                // Get historical performance data
                var historicalData = new List<QueryExecutionHistory>();
                if (useHistoricalData)
                {
                    historicalData = GetHistoricalQueryData(query);
                }

                // Analyze query complexity
                var complexity = CalculateQueryComplexity(query);

                // Get performance baseline
                var baseline = GetPerformanceBaseline(query);

                // Generate optimization recommendations
                var recommendations = GenerateOptimizationRecommendations(query, complexity, baseline, historicalData);

                result.OriginalQuery = query;
                result.OptimizedQuery = query; // Would be enhanced with actual optimization
                result.PerformanceAnalysis = $"Query complexity: {complexity}, Baseline performance: {baseline?.AverageExecutionTimeMs}ms";
                result.Recommendations = recommendations;
                result.EstimatedImprovement = CalculateEstimatedImprovement(complexity, baseline, historicalData);
                result.IsSuccessful = true;

                _logger.LogInformation("Query optimization with history completed. Estimated improvement: {Improvement}%", 
                    result.EstimatedImprovement);
            }
            catch (Exception ex)
            {
                result.IsSuccessful = false;
                result.ErrorMessage = ex.Message;
                _logger.LogError(ex, "Failed to optimize query with history");
            }

            return result;
        }

        // Private helper methods
        private string GenerateQueryCacheKey(string query, string connectionString)
        {
            return $"{query}_{connectionString}".GetHashCode().ToString();
        }

        private void UpdatePerformanceBaseline(string query, int executionTime, int rowsReturned)
        {
            var key = query.GetHashCode().ToString();
            
            if (_performanceBaselines.TryGetValue(key, out var baseline))
            {
                baseline.ExecutionCount++;
                baseline.TotalExecutionTime += executionTime;
                baseline.TotalRowsReturned += rowsReturned;
                baseline.AverageExecutionTimeMs = baseline.TotalExecutionTime / baseline.ExecutionCount;
                baseline.LastExecuted = DateTime.UtcNow;
            }
            else
            {
                _performanceBaselines.TryAdd(key, new PerformanceBaseline
                {
                    Query = query,
                    ExecutionCount = 1,
                    TotalExecutionTime = executionTime,
                    TotalRowsReturned = rowsReturned,
                    AverageExecutionTimeMs = executionTime,
                    LastExecuted = DateTime.UtcNow
                });
            }
        }

        private PerformanceBaseline? GetPerformanceBaseline(string query)
        {
            var key = query.GetHashCode().ToString();
            _performanceBaselines.TryGetValue(key, out var baseline);
            return baseline;
        }

        private List<QueryExecutionHistory> GetHistoricalQueryData(string query)
        {
            var historicalData = new List<QueryExecutionHistory>();
            var queryHash = query.GetHashCode().ToString();

            foreach (var history in _executionHistory.Values)
            {
                if (history.QueryHash == queryHash)
                {
                    historicalData.Add(history);
                }
            }

            return historicalData;
        }

        private List<string> GenerateOptimizationRecommendations(string query, int complexity, PerformanceBaseline? baseline, List<QueryExecutionHistory> historicalData)
        {
            var recommendations = new List<string>();

            if (complexity > 50)
            {
                recommendations.Add("Consider breaking down complex query into smaller subqueries");
            }

            if (query.ToUpper().Contains("SELECT *"))
            {
                recommendations.Add("Use specific column names instead of SELECT * for better performance");
            }

            if (query.ToUpper().Contains("CROSS JOIN") && !query.ToUpper().Contains("WHERE"))
            {
                recommendations.Add("Add WHERE clause to CROSS JOIN to prevent performance issues");
            }

            if (baseline != null && baseline.AverageExecutionTimeMs > 5000)
            {
                recommendations.Add("Query execution time is high, consider adding indexes or optimizing joins");
            }

            if (historicalData.Count > 10)
            {
                var avgTime = historicalData.Average(h => h.ExecutionTimeMs);
                if (avgTime > 3000)
                {
                    recommendations.Add("Historical data shows consistent slow performance, optimization recommended");
                }
            }

            return recommendations;
        }

        private double CalculateEstimatedImprovement(int complexity, PerformanceBaseline? baseline, List<QueryExecutionHistory> historicalData)
        {
            var improvement = 0.0;

            if (complexity > 50) improvement += 20.0;
            if (baseline?.AverageExecutionTimeMs > 5000) improvement += 30.0;
            if (historicalData.Count > 10)
            {
                var avgTime = historicalData.Average(h => h.ExecutionTimeMs);
                if (avgTime > 3000) improvement += 25.0;
            }

            return Math.Min(improvement, 100.0);
        }

        private int CalculateQueryComplexity(string query)
        {
            var complexity = 0;
            var upperQuery = query.ToUpper();
            
            if (upperQuery.Contains("JOIN")) complexity += 10;
            if (upperQuery.Contains("WHERE")) complexity += 5;
            if (upperQuery.Contains("GROUP BY")) complexity += 15;
            if (upperQuery.Contains("ORDER BY")) complexity += 5;
            if (upperQuery.Contains("HAVING")) complexity += 10;
            if (upperQuery.Contains("SUBQUERY") || upperQuery.Contains("(")) complexity += 20;
            if (upperQuery.Contains("UNION")) complexity += 15;
            if (upperQuery.Contains("CTE") || upperQuery.Contains("WITH")) complexity += 25;
            
            return complexity;
        }

        private double EstimateQueryCost(string query, int rowsReturned)
        {
            var baseCost = 1.0;
            var complexity = CalculateQueryComplexity(query);
            var rowCost = rowsReturned * 0.001;
            
            return baseCost + (complexity * 0.1) + rowCost;
        }

        private double CalculatePerformanceScore(EnterprisePerformanceMetrics metrics)
        {
            var score = 100.0;
            
            if (metrics.ExecutionTimeMs > 10000) score -= 30;
            else if (metrics.ExecutionTimeMs > 5000) score -= 20;
            else if (metrics.ExecutionTimeMs > 1000) score -= 10;

            if (metrics.MemoryUsageBytes > 100 * 1024 * 1024) score -= 20; // 100MB
            else if (metrics.MemoryUsageBytes > 50 * 1024 * 1024) score -= 10; // 50MB

            if (metrics.QueryComplexity > 50) score -= 15;
            else if (metrics.QueryComplexity > 25) score -= 10;

            return Math.Max(score, 0.0);
        }

        private ConnectionPoolStatus GetConnectionPoolStatus()
        {
            return new ConnectionPoolStatus
            {
                TotalPools = _connectionPools.Count,
                ActiveConnections = _connectionPools.Values.Count(p => IsConnectionHealthy(p.Connection, "Unknown")),
                TotalUseCount = _connectionPools.Values.Sum(p => p.UseCount)
            };
        }

        private QueryHistoryStatus GetQueryHistoryStatus()
        {
            return new QueryHistoryStatus
            {
                TotalQueries = _executionHistory.Count,
                AverageExecutionTime = _executionHistory.Values.Average(h => h.ExecutionTimeMs),
                TotalRowsReturned = _executionHistory.Values.Sum(h => h.RowsReturned)
            };
        }

        private async Task<PerformanceMetrics> CollectPerformanceMetrics(AIConnection connection)
        {
            return new PerformanceMetrics
            {
                QueryComplexity = 0,
                EstimatedCost = 0,
                OptimizationScore = 0
            };
        }
    }

    // Enterprise result classes
    public class EnterpriseQueryExecutionResult
    {
        public bool IsSuccessful { get; set; }
        public bool IsFromCache { get; set; }
        public List<List<string>> Results { get; set; } = new List<List<string>>();
        public string? ErrorMessage { get; set; }
        public int ExecutionTimeMs { get; set; }
        public int RowsReturned { get; set; }
        public EnterprisePerformanceMetrics PerformanceMetrics { get; set; } = new EnterprisePerformanceMetrics();
    }

    public class EnterprisePerformanceMetrics
    {
        public bool IsSuccessful { get; set; }
        public int ExecutionTimeMs { get; set; }
        public int RowsReturned { get; set; }
        public int ColumnsReturned { get; set; }
        public long MemoryUsageBytes { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int QueryComplexity { get; set; }
        public double EstimatedCost { get; set; }
        public double PerformanceScore { get; set; }
        public List<List<string>> Results { get; set; } = new List<List<string>>();
        public string? ErrorMessage { get; set; }
    }

    public class EnterpriseBatchExecutionResult
    {
        public bool IsSuccessful { get; set; }
        public List<EnterpriseBatchStep> Steps { get; set; } = new List<EnterpriseBatchStep>();
        public int TotalSteps { get; set; }
        public int SuccessfulSteps { get; set; }
        public int FailedSteps { get; set; }
        public int ExecutionTimeMs { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class EnterpriseBatchStep
    {
        public int StepNumber { get; set; }
        public string Query { get; set; } = string.Empty;
        public List<List<string>> Results { get; set; } = new List<List<string>>();
        public int RowsReturned { get; set; }
        public bool IsSuccessful { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class EnterpriseHealthStatus
    {
        public bool IsSuccessful { get; set; }
        public bool IsConnected { get; set; }
        public bool IsQueryable { get; set; }
        public string DatabaseType { get; set; } = string.Empty;
        public DateTime LastChecked { get; set; }
        public DetailedSchemaInfo DetailedSchema { get; set; } = new DetailedSchemaInfo();
        public PerformanceMetrics PerformanceMetrics { get; set; } = new PerformanceMetrics();
        public ConnectionPoolStatus ConnectionPoolStatus { get; set; } = new ConnectionPoolStatus();
        public QueryHistoryStatus QueryHistoryStatus { get; set; } = new QueryHistoryStatus();
        public string? ErrorMessage { get; set; }
    }

    public class ConnectionPool
    {
        public object Connection { get; set; } = new object();
        public DateTime CreatedAt { get; set; }
        public DateTime LastUsed { get; set; }
        public int UseCount { get; set; }
    }

    public class ConnectionPoolStatus
    {
        public int TotalPools { get; set; }
        public int ActiveConnections { get; set; }
        public int TotalUseCount { get; set; }
    }

    public class QueryHistoryStatus
    {
        public int TotalQueries { get; set; }
        public double AverageExecutionTime { get; set; }
        public int TotalRowsReturned { get; set; }
    }

    public class QueryExecutionHistory
    {
        public List<List<string>> Results { get; set; } = new List<List<string>>();
        public int RowsReturned { get; set; }
        public DateTime LastExecuted { get; set; }
        public int ExecutionTimeMs { get; set; }
        public string QueryHash { get; set; } = string.Empty;
    }

    public class PerformanceBaseline
    {
        public string Query { get; set; } = string.Empty;
        public int ExecutionCount { get; set; }
        public int TotalExecutionTime { get; set; }
        public int TotalRowsReturned { get; set; }
        public int AverageExecutionTimeMs { get; set; }
        public DateTime LastExecuted { get; set; }
    }
}