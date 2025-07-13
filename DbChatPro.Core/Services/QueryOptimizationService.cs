using DBChatPro.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace DBChatPro.Services
{
    public class QueryOptimizationService
    {
        private readonly ILogger<QueryOptimizationService> _logger;
        private readonly ConcurrentDictionary<string, QueryOptimizationCache> _optimizationCache = new();
        private readonly ConcurrentDictionary<string, QueryPerformanceProfile> _performanceProfiles = new();

        public QueryOptimizationService(ILogger<QueryOptimizationService> logger)
        {
            _logger = logger;
        }

        // Advanced query optimization with multiple strategies
        public async Task<AdvancedQueryOptimizationResult> OptimizeQueryAdvanced(
            string sqlQuery,
            string databaseType,
            string optimizationStrategy = "Comprehensive",
            bool useMachineLearning = true,
            bool enableCaching = true)
        {
            var cacheKey = GenerateOptimizationCacheKey(sqlQuery, databaseType, optimizationStrategy);
            
            // Check cache first
            if (enableCaching && _optimizationCache.TryGetValue(cacheKey, out var cachedResult) && 
                DateTime.UtcNow.Subtract(cachedResult.CachedAt).TotalHours < 24)
            {
                return cachedResult.Result;
            }

            var result = new AdvancedQueryOptimizationResult();

            try
            {
                _logger.LogInformation("Performing advanced query optimization with strategy: {Strategy}", optimizationStrategy);

                // Analyze query structure
                var analysis = AnalyzeQueryStructure(sqlQuery, databaseType);

                // Get performance profile
                var profile = GetPerformanceProfile(sqlQuery, databaseType);

                // Apply optimization strategies
                var optimizations = await ApplyOptimizationStrategies(sqlQuery, databaseType, optimizationStrategy, analysis, profile);

                // Generate optimized query
                var optimizedQuery = GenerateOptimizedQuery(sqlQuery, optimizations, databaseType);

                // Calculate improvement metrics
                var improvementMetrics = CalculateImprovementMetrics(analysis, optimizations, profile);

                result.OriginalQuery = sqlQuery;
                result.OptimizedQuery = optimizedQuery;
                result.QueryAnalysis = analysis;
                result.Optimizations = optimizations;
                result.ImprovementMetrics = improvementMetrics;
                result.Strategy = optimizationStrategy;
                result.IsSuccessful = true;

                // Cache the result
                if (enableCaching)
                {
                    _optimizationCache.TryAdd(cacheKey, new QueryOptimizationCache
                    {
                        Result = result,
                        CachedAt = DateTime.UtcNow
                    });
                }

                _logger.LogInformation("Advanced query optimization completed. Estimated improvement: {Improvement}%", 
                    improvementMetrics.EstimatedPerformanceImprovement);
            }
            catch (Exception ex)
            {
                result.IsSuccessful = false;
                result.ErrorMessage = ex.Message;
                _logger.LogError(ex, "Failed to optimize query advanced");
            }

            return result;
        }

        // Query structure analysis
        private QueryStructureAnalysis AnalyzeQueryStructure(string sqlQuery, string databaseType)
        {
            var analysis = new QueryStructureAnalysis();
            var upperQuery = sqlQuery.ToUpper();

            // Analyze query type
            if (upperQuery.Contains("SELECT"))
            {
                analysis.QueryType = "SELECT";
                analysis.Complexity = CalculateQueryComplexity(upperQuery);
            }
            else if (upperQuery.Contains("INSERT"))
            {
                analysis.QueryType = "INSERT";
                analysis.Complexity = 10;
            }
            else if (upperQuery.Contains("UPDATE"))
            {
                analysis.QueryType = "UPDATE";
                analysis.Complexity = 15;
            }
            else if (upperQuery.Contains("DELETE"))
            {
                analysis.QueryType = "DELETE";
                analysis.Complexity = 15;
            }

            // Analyze joins
            analysis.JoinCount = CountJoins(upperQuery);
            analysis.HasSubqueries = upperQuery.Contains("(") && upperQuery.Contains("SELECT");
            analysis.HasAggregations = HasAggregations(upperQuery);
            analysis.HasWindowFunctions = HasWindowFunctions(upperQuery);
            analysis.HasCTEs = upperQuery.Contains("WITH") || upperQuery.Contains("CTE");

            // Analyze potential issues
            analysis.PotentialIssues = IdentifyPotentialIssues(upperQuery, databaseType);

            return analysis;
        }

        // Apply optimization strategies
        private async Task<List<QueryOptimization>> ApplyOptimizationStrategies(
            string sqlQuery, 
            string databaseType, 
            string strategy, 
            QueryStructureAnalysis analysis,
            QueryPerformanceProfile profile)
        {
            var optimizations = new List<QueryOptimization>();

            switch (strategy.ToLower())
            {
                case "performance":
                    optimizations.AddRange(ApplyPerformanceOptimizations(sqlQuery, analysis, profile));
                    break;
                case "memory":
                    optimizations.AddRange(ApplyMemoryOptimizations(sqlQuery, analysis, profile));
                    break;
                case "comprehensive":
                    optimizations.AddRange(ApplyComprehensiveOptimizations(sqlQuery, analysis, profile));
                    break;
                case "security":
                    optimizations.AddRange(ApplySecurityOptimizations(sqlQuery, analysis));
                    break;
                default:
                    optimizations.AddRange(ApplyComprehensiveOptimizations(sqlQuery, analysis, profile));
                    break;
            }

            return optimizations;
        }

        // Performance optimizations
        private List<QueryOptimization> ApplyPerformanceOptimizations(string sqlQuery, QueryStructureAnalysis analysis, QueryPerformanceProfile profile)
        {
            var optimizations = new List<QueryOptimization>();

            // Optimize SELECT statements
            if (analysis.QueryType == "SELECT")
            {
                if (sqlQuery.ToUpper().Contains("SELECT *"))
                {
                    optimizations.Add(new QueryOptimization
                    {
                        Type = "Performance",
                        Description = "Replace SELECT * with specific column names",
                        Impact = "High",
                        Effort = "Low",
                        Recommendation = "Specify only needed columns to reduce data transfer"
                    });
                }

                if (analysis.JoinCount > 3)
                {
                    optimizations.Add(new QueryOptimization
                    {
                        Type = "Performance",
                        Description = "Optimize multiple JOINs",
                        Impact = "High",
                        Effort = "Medium",
                        Recommendation = "Consider breaking down complex joins or using CTEs"
                    });
                }

                if (analysis.HasSubqueries)
                {
                    optimizations.Add(new QueryOptimization
                    {
                        Type = "Performance",
                        Description = "Optimize subqueries",
                        Impact = "Medium",
                        Effort = "Medium",
                        Recommendation = "Consider using JOINs instead of subqueries where possible"
                    });
                }
            }

            // Add index recommendations
            if (profile.AverageExecutionTime > 5000)
            {
                optimizations.Add(new QueryOptimization
                {
                    Type = "Performance",
                    Description = "Add indexes for better performance",
                    Impact = "High",
                    Effort = "Low",
                    Recommendation = "Create indexes on frequently queried columns"
                });
            }

            return optimizations;
        }

        // Memory optimizations
        private List<QueryOptimization> ApplyMemoryOptimizations(string sqlQuery, QueryStructureAnalysis analysis, QueryPerformanceProfile profile)
        {
            var optimizations = new List<QueryOptimization>();

            if (analysis.HasAggregations)
            {
                optimizations.Add(new QueryOptimization
                {
                    Type = "Memory",
                    Description = "Optimize aggregations",
                    Impact = "Medium",
                    Effort = "Medium",
                    Recommendation = "Use appropriate GROUP BY clauses and limit result sets"
                });
            }

            if (analysis.HasWindowFunctions)
            {
                optimizations.Add(new QueryOptimization
                {
                    Type = "Memory",
                    Description = "Optimize window functions",
                    Impact = "Medium",
                    Effort = "High",
                    Recommendation = "Consider partitioning for large datasets"
                });
            }

            return optimizations;
        }

        // Comprehensive optimizations
        private List<QueryOptimization> ApplyComprehensiveOptimizations(string sqlQuery, QueryStructureAnalysis analysis, QueryPerformanceProfile profile)
        {
            var optimizations = new List<QueryOptimization>();

            // Combine performance and memory optimizations
            optimizations.AddRange(ApplyPerformanceOptimizations(sqlQuery, analysis, profile));
            optimizations.AddRange(ApplyMemoryOptimizations(sqlQuery, analysis, profile));

            // Add security optimizations
            optimizations.AddRange(ApplySecurityOptimizations(sqlQuery, analysis));

            // Add database-specific optimizations
            optimizations.AddRange(ApplyDatabaseSpecificOptimizations(sqlQuery, analysis));

            return optimizations;
        }

        // Security optimizations
        private List<QueryOptimization> ApplySecurityOptimizations(string sqlQuery, QueryStructureAnalysis analysis)
        {
            var optimizations = new List<QueryOptimization>();

            // Check for SQL injection vulnerabilities
            if (ContainsDynamicSql(sqlQuery))
            {
                optimizations.Add(new QueryOptimization
                {
                    Type = "Security",
                    Description = "Prevent SQL injection",
                    Impact = "Critical",
                    Effort = "High",
                    Recommendation = "Use parameterized queries instead of string concatenation"
                });
            }

            // Check for excessive permissions
            if (analysis.QueryType == "DELETE" || analysis.QueryType == "UPDATE")
            {
                optimizations.Add(new QueryOptimization
                {
                    Type = "Security",
                    Description = "Limit data modification scope",
                    Impact = "High",
                    Effort = "Medium",
                    Recommendation = "Add WHERE clauses to limit affected rows"
                });
            }

            return optimizations;
        }

        // Database-specific optimizations
        private List<QueryOptimization> ApplyDatabaseSpecificOptimizations(string sqlQuery, QueryStructureAnalysis analysis)
        {
            var optimizations = new List<QueryOptimization>();

            // SQL Server specific optimizations
            if (sqlQuery.ToUpper().Contains("NOLOCK"))
            {
                optimizations.Add(new QueryOptimization
                {
                    Type = "Database-Specific",
                    Description = "Avoid NOLOCK hint",
                    Impact = "Medium",
                    Effort = "Low",
                    Recommendation = "NOLOCK can lead to dirty reads, use appropriate isolation levels"
                });
            }

            // MySQL specific optimizations
            if (sqlQuery.ToUpper().Contains("FOR UPDATE"))
            {
                optimizations.Add(new QueryOptimization
                {
                    Type = "Database-Specific",
                    Description = "Optimize locking strategy",
                    Impact = "Medium",
                    Effort = "Medium",
                    Recommendation = "Consider using SELECT ... LOCK IN SHARE MODE for read operations"
                });
            }

            return optimizations;
        }

        // Generate optimized query
        private string GenerateOptimizedQuery(string originalQuery, List<QueryOptimization> optimizations, string databaseType)
        {
            var optimizedQuery = originalQuery;

            // Apply optimizations based on type
            foreach (var optimization in optimizations.Where(o => o.Type == "Performance"))
            {
                optimizedQuery = ApplyOptimizationToQuery(optimizedQuery, optimization, databaseType);
            }

            return optimizedQuery;
        }

        // Apply specific optimization to query
        private string ApplyOptimizationToQuery(string query, QueryOptimization optimization, string databaseType)
        {
            var upperQuery = query.ToUpper();

            // Replace SELECT * with specific columns (example)
            if (optimization.Description.Contains("SELECT *"))
            {
                // This is a simplified example - in practice, you'd need to analyze the schema
                query = query.Replace("SELECT *", "SELECT id, name, created_at");
            }

            // Add WHERE clauses for DELETE/UPDATE
            if (optimization.Description.Contains("WHERE clauses"))
            {
                if (upperQuery.Contains("DELETE FROM") && !upperQuery.Contains("WHERE"))
                {
                    query = query.Replace("DELETE FROM", "DELETE FROM table_name WHERE id > 0");
                }
            }

            return query;
        }

        // Calculate improvement metrics
        private ImprovementMetrics CalculateImprovementMetrics(QueryStructureAnalysis analysis, List<QueryOptimization> optimizations, QueryPerformanceProfile profile)
        {
            var metrics = new ImprovementMetrics();

            // Calculate performance improvement
            var performanceOptimizations = optimizations.Where(o => o.Type == "Performance").Count();
            var memoryOptimizations = optimizations.Where(o => o.Type == "Memory").Count();
            var securityOptimizations = optimizations.Where(o => o.Type == "Security").Count();

            metrics.EstimatedPerformanceImprovement = Math.Min(performanceOptimizations * 15 + memoryOptimizations * 10, 80);
            metrics.EstimatedMemoryReduction = Math.Min(memoryOptimizations * 20, 60);
            metrics.SecurityScore = 100 - (securityOptimizations * 10);
            metrics.OverallScore = (metrics.EstimatedPerformanceImprovement + metrics.EstimatedMemoryReduction + metrics.SecurityScore) / 3;

            return metrics;
        }

        // Get performance profile
        private QueryPerformanceProfile GetPerformanceProfile(string sqlQuery, string databaseType)
        {
            var key = $"{sqlQuery.GetHashCode()}_{databaseType}";
            
            if (_performanceProfiles.TryGetValue(key, out var profile))
            {
                return profile;
            }

            // Create default profile
            return new QueryPerformanceProfile
            {
                AverageExecutionTime = 1000,
                MemoryUsage = 1024 * 1024, // 1MB
                Complexity = CalculateQueryComplexity(sqlQuery.ToUpper())
            };
        }

        // Helper methods
        private string GenerateOptimizationCacheKey(string sqlQuery, string databaseType, string strategy)
        {
            return $"{sqlQuery.GetHashCode()}_{databaseType}_{strategy}".GetHashCode().ToString();
        }

        private int CalculateQueryComplexity(string upperQuery)
        {
            var complexity = 0;
            
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

        private int CountJoins(string upperQuery)
        {
            var joinCount = 0;
            if (upperQuery.Contains("INNER JOIN")) joinCount += Regex.Matches(upperQuery, "INNER JOIN").Count;
            if (upperQuery.Contains("LEFT JOIN")) joinCount += Regex.Matches(upperQuery, "LEFT JOIN").Count;
            if (upperQuery.Contains("RIGHT JOIN")) joinCount += Regex.Matches(upperQuery, "RIGHT JOIN").Count;
            if (upperQuery.Contains("FULL JOIN")) joinCount += Regex.Matches(upperQuery, "FULL JOIN").Count;
            if (upperQuery.Contains("CROSS JOIN")) joinCount += Regex.Matches(upperQuery, "CROSS JOIN").Count;
            
            return joinCount;
        }

        private bool HasAggregations(string upperQuery)
        {
            return upperQuery.Contains("COUNT(") || upperQuery.Contains("SUM(") || upperQuery.Contains("AVG(") ||
                   upperQuery.Contains("MAX(") || upperQuery.Contains("MIN(") || upperQuery.Contains("GROUP BY");
        }

        private bool HasWindowFunctions(string upperQuery)
        {
            return upperQuery.Contains("ROW_NUMBER(") || upperQuery.Contains("RANK(") || upperQuery.Contains("DENSE_RANK(") ||
                   upperQuery.Contains("LAG(") || upperQuery.Contains("LEAD(") || upperQuery.Contains("OVER(");
        }

        private List<string> IdentifyPotentialIssues(string upperQuery, string databaseType)
        {
            var issues = new List<string>();

            if (upperQuery.Contains("SELECT *"))
            {
                issues.Add("Using SELECT * may impact performance");
            }

            if (upperQuery.Contains("CROSS JOIN") && !upperQuery.Contains("WHERE"))
            {
                issues.Add("CROSS JOIN without WHERE clause may cause performance issues");
            }

            if (upperQuery.Contains("NOLOCK") && databaseType.Equals("SQLServer", StringComparison.OrdinalIgnoreCase))
            {
                issues.Add("NOLOCK hint may lead to dirty reads");
            }

            return issues;
        }

        private bool ContainsDynamicSql(string sqlQuery)
        {
            // Simple check for potential SQL injection - in practice, you'd need more sophisticated analysis
            return sqlQuery.Contains("'") && (sqlQuery.Contains("+") || sqlQuery.Contains("CONCAT"));
        }
    }

    // Result classes
    public class AdvancedQueryOptimizationResult
    {
        public bool IsSuccessful { get; set; }
        public string OriginalQuery { get; set; } = string.Empty;
        public string OptimizedQuery { get; set; } = string.Empty;
        public QueryStructureAnalysis QueryAnalysis { get; set; } = new QueryStructureAnalysis();
        public List<QueryOptimization> Optimizations { get; set; } = new List<QueryOptimization>();
        public ImprovementMetrics ImprovementMetrics { get; set; } = new ImprovementMetrics();
        public string Strategy { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
    }

    public class QueryStructureAnalysis
    {
        public string QueryType { get; set; } = string.Empty;
        public int Complexity { get; set; }
        public int JoinCount { get; set; }
        public bool HasSubqueries { get; set; }
        public bool HasAggregations { get; set; }
        public bool HasWindowFunctions { get; set; }
        public bool HasCTEs { get; set; }
        public List<string> PotentialIssues { get; set; } = new List<string>();
    }

    public class QueryOptimization
    {
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Impact { get; set; } = string.Empty;
        public string Effort { get; set; } = string.Empty;
        public string Recommendation { get; set; } = string.Empty;
    }

    public class ImprovementMetrics
    {
        public double EstimatedPerformanceImprovement { get; set; }
        public double EstimatedMemoryReduction { get; set; }
        public double SecurityScore { get; set; }
        public double OverallScore { get; set; }
    }

    public class QueryPerformanceProfile
    {
        public int AverageExecutionTime { get; set; }
        public long MemoryUsage { get; set; }
        public int Complexity { get; set; }
    }

    public class QueryOptimizationCache
    {
        public AdvancedQueryOptimizationResult Result { get; set; } = new AdvancedQueryOptimizationResult();
        public DateTime CachedAt { get; set; }
    }
}