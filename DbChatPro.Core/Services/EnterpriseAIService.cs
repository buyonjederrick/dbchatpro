using Amazon.BedrockRuntime;
using Azure.AI.Inference;
using Azure;
using Azure.AI.OpenAI;
using Azure.Identity;
using DBChatPro.Models;
using Microsoft.Extensions.AI;
using OpenAI;
using System.Text;
using System.Text.Json;
using System.ClientModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace DBChatPro.Services
{
    public class EnterpriseAIService : AdvancedAIService
    {
        private readonly IConfiguration _config;
        private readonly IServiceProvider _serviceProvider;
        private readonly ConcurrentDictionary<string, AICache> _aiCache = new();
        private readonly ConcurrentDictionary<string, QueryPattern> _queryPatterns = new();

        public EnterpriseAIService(IConfiguration config, IServiceProvider serviceProvider) : base(config, serviceProvider)
        {
            _config = config;
            _serviceProvider = serviceProvider;
        }

        // Enterprise-level SQL query generation with advanced optimization
        public async Task<EnterpriseAIQuery> GetEnterpriseAISQLQuery(
            string aiModel, 
            string aiService, 
            string userPrompt, 
            DatabaseSchema dbSchema, 
            string databaseType,
            string complexityLevel = "Advanced",
            bool enableCaching = true,
            bool enableValidation = true)
        {
            var cacheKey = GenerateCacheKey(userPrompt, aiModel, aiService, complexityLevel);
            
            // Check cache first
            if (enableCaching && _aiCache.TryGetValue(cacheKey, out var cachedResult) && 
                DateTime.UtcNow.Subtract(cachedResult.CachedAt).TotalMinutes < 30)
            {
                return cachedResult.Result;
            }

            var aiClient = CreateChatClient(aiModel, aiService);
            var chatHistory = new List<ChatMessage>();
            var builder = new StringBuilder();

            // Enhanced enterprise system prompt
            builder.AppendLine("You are an enterprise-level database query optimizer and SQL generator with expertise in:");
            builder.AppendLine("1. Complex multi-table joins and subqueries");
            builder.AppendLine("2. Performance optimization and query execution plans");
            builder.AppendLine("3. Advanced SQL features (CTEs, window functions, recursive queries)");
            builder.AppendLine("4. Database-specific optimizations and best practices");
            builder.AppendLine("5. Security considerations and query validation");
            builder.AppendLine("6. Scalability and resource management");

            builder.AppendLine("Database Schema:");
            foreach (var table in dbSchema.SchemaRaw)
            {
                builder.AppendLine(table);
            }

            builder.AppendLine($"Database Type: {databaseType}");
            builder.AppendLine($"Complexity Level: {complexityLevel}");

            // Add complexity-specific instructions
            switch (complexityLevel.ToLower())
            {
                case "expert":
                    builder.AppendLine("EXPERT MODE: Generate highly optimized queries with:");
                    builder.AppendLine("- Advanced JOIN optimization strategies");
                    builder.AppendLine("- Subquery and CTE optimization");
                    builder.AppendLine("- Window functions and analytical queries");
                    builder.AppendLine("- Performance monitoring considerations");
                    builder.AppendLine("- Resource usage optimization");
                    break;
                case "advanced":
                    builder.AppendLine("ADVANCED MODE: Generate optimized queries with:");
                    builder.AppendLine("- Efficient JOIN strategies");
                    builder.AppendLine("- Index-aware query design");
                    builder.AppendLine("- Query plan optimization");
                    break;
                case "intermediate":
                    builder.AppendLine("INTERMEDIATE MODE: Generate balanced queries with:");
                    builder.AppendLine("- Basic optimization techniques");
                    builder.AppendLine("- Standard JOIN patterns");
                    break;
                default:
                    builder.AppendLine("BASIC MODE: Generate simple, readable queries");
                    break;
            }

            builder.AppendLine("Provide response in this JSON format:");
            builder.AppendLine(@"{");
            builder.AppendLine(@"  ""query"": ""optimized SQL query"",");
            builder.AppendLine(@"  ""analysis"": ""detailed performance analysis"",");
            builder.AppendLine(@"  ""optimizations"": [""optimization1"", ""optimization2""],");
            builder.AppendLine(@"  ""estimatedPerformance"": ""performance estimate"",");
            builder.AppendLine(@"  ""recommendations"": [""recommendation1"", ""recommendation2""],");
            builder.AppendLine(@"  ""complexityScore"": 85,");
            builder.AppendLine(@"  ""securityConsiderations"": [""security1"", ""security2""],");
            builder.AppendLine(@"  ""scalabilityNotes"": ""scalability considerations""");
            builder.AppendLine(@"}");

            chatHistory.Add(new ChatMessage(Microsoft.Extensions.AI.ChatRole.System, builder.ToString()));
            chatHistory.Add(new ChatMessage(Microsoft.Extensions.AI.ChatRole.User, userPrompt));

            var response = await aiClient.GetResponseAsync(chatHistory);
            var responseContent = response.Messages[0].Text.Replace("```json", "").Replace("```", "").Replace("\\n", " ");

            try
            {
                var result = JsonSerializer.Deserialize<EnterpriseAIQuery>(responseContent) ?? new EnterpriseAIQuery();
                
                // Validate query if enabled
                if (enableValidation)
                {
                    var validationResult = ValidateEnterpriseQuery(result.query, databaseType);
                    if (!validationResult.IsValid)
                    {
                        result.validationErrors = validationResult.Errors;
                    }
                }

                // Cache the result
                if (enableCaching)
                {
                    _aiCache.TryAdd(cacheKey, new AICache { Result = result, CachedAt = DateTime.UtcNow });
                }

                return result;
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to parse enterprise AI response: {response.Messages[0].Text}. Error: {e.Message}");
            }
        }

        // Advanced query optimization with machine learning insights
        public async Task<EnterpriseQueryOptimization> OptimizeQueryEnterprise(
            string aiModel,
            string aiService,
            string sqlQuery,
            DatabaseSchema dbSchema,
            string databaseType,
            string optimizationStrategy = "Comprehensive",
            bool useHistoricalData = true)
        {
            var aiClient = CreateChatClient(aiModel, aiService);
            var chatHistory = new List<ChatMessage>();
            var builder = new StringBuilder();

            builder.AppendLine("You are an enterprise database performance optimization expert. Analyze and optimize the following SQL query:");
            builder.AppendLine("1. Identify performance bottlenecks and optimization opportunities");
            builder.AppendLine("2. Provide multiple optimization strategies");
            builder.AppendLine("3. Consider database-specific optimizations");
            builder.AppendLine("4. Analyze query execution plans and resource usage");
            builder.AppendLine("5. Provide security and scalability recommendations");

            builder.AppendLine("Database Schema:");
            foreach (var table in dbSchema.SchemaRaw)
            {
                builder.AppendLine(table);
            }

            builder.AppendLine($"Database Type: {databaseType}");
            builder.AppendLine($"Optimization Strategy: {optimizationStrategy}");
            builder.AppendLine($"Query to Optimize: {sqlQuery}");

            // Add strategy-specific instructions
            switch (optimizationStrategy.ToLower())
            {
                case "performance":
                    builder.AppendLine("Focus on execution speed and query plan optimization");
                    break;
                case "memory":
                    builder.AppendLine("Focus on memory usage and resource efficiency");
                    break;
                case "comprehensive":
                    builder.AppendLine("Provide balanced optimization across all aspects");
                    break;
            }

            builder.AppendLine("Provide response in this JSON format:");
            builder.AppendLine(@"{");
            builder.AppendLine(@"  ""optimizedQuery"": ""optimized version"",");
            builder.AppendLine(@"  ""analysis"": ""detailed performance analysis"",");
            builder.AppendLine(@"  ""recommendations"": [""rec1"", ""rec2""],");
            builder.AppendLine(@"  ""estimatedImprovement"": 25.5,");
            builder.AppendLine(@"  ""optimizationStrategies"": [""strategy1"", ""strategy2""],");
            builder.AppendLine(@"  ""performanceMetrics"": {");
            builder.AppendLine(@"    ""estimatedExecutionTime"": ""time estimate"",");
            builder.AppendLine(@"    ""estimatedMemoryUsage"": ""memory estimate"",");
            builder.AppendLine(@"    ""complexityReduction"": 15");
            builder.AppendLine(@"  },");
            builder.AppendLine(@"  ""securityRecommendations"": [""security1"", ""security2""],");
            builder.AppendLine(@"  ""scalabilityNotes"": ""scalability considerations""");
            builder.AppendLine(@"}");

            chatHistory.Add(new ChatMessage(Microsoft.Extensions.AI.ChatRole.System, builder.ToString()));

            var response = await aiClient.GetResponseAsync(chatHistory);
            var responseContent = response.Messages[0].Text.Replace("```json", "").Replace("```", "").Replace("\\n", " ");

            try
            {
                return JsonSerializer.Deserialize<EnterpriseQueryOptimization>(responseContent) ?? new EnterpriseQueryOptimization();
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to parse enterprise query optimization: {response.Messages[0].Text}. Error: {e.Message}");
            }
        }

        // Intelligent schema analysis with AI-powered recommendations
        public async Task<EnterpriseSchemaAnalysis> AnalyzeSchemaEnterprise(
            string aiModel,
            string aiService,
            DatabaseSchema dbSchema,
            string databaseType,
            string analysisScope = "Comprehensive")
        {
            var aiClient = CreateChatClient(aiModel, aiService);
            var chatHistory = new List<ChatMessage>();
            var builder = new StringBuilder();

            builder.AppendLine("You are an enterprise database architect and performance expert. Analyze the following database schema and provide:");
            builder.AppendLine("1. Schema design recommendations and improvements");
            builder.AppendLine("2. Performance optimization opportunities");
            builder.AppendLine("3. Indexing strategies and recommendations");
            builder.AppendLine("4. Data modeling and normalization analysis");
            builder.AppendLine("5. Security and compliance considerations");
            builder.AppendLine("6. Scalability and maintenance recommendations");

            builder.AppendLine("Database Schema:");
            foreach (var table in dbSchema.SchemaRaw)
            {
                builder.AppendLine(table);
            }

            builder.AppendLine($"Database Type: {databaseType}");
            builder.AppendLine($"Analysis Scope: {analysisScope}");

            builder.AppendLine("Provide response in this JSON format:");
            builder.AppendLine(@"{");
            builder.AppendLine(@"  ""recommendations"": [""rec1"", ""rec2""],");
            builder.AppendLine(@"  ""insights"": [""insight1"", ""insight2""],");
            builder.AppendLine(@"  ""opportunities"": [""opp1"", ""opp2""],");
            builder.AppendLine(@"  ""indexingStrategies"": [""index1"", ""index2""],");
            builder.AppendLine(@"  ""securityRecommendations"": [""security1"", ""security2""],");
            builder.AppendLine(@"  ""scalabilityRecommendations"": [""scalability1"", ""scalability2""],");
            builder.AppendLine(@"  ""performanceImpact"": {");
            builder.AppendLine(@"    ""estimatedImprovement"": 35.5,");
            builder.AppendLine(@"    ""implementationEffort"": ""medium"",");
            builder.AppendLine(@"    ""riskLevel"": ""low""");
            builder.AppendLine(@"  }");
            builder.AppendLine(@"}");

            chatHistory.Add(new ChatMessage(Microsoft.Extensions.AI.ChatRole.System, builder.ToString()));

            var response = await aiClient.GetResponseAsync(chatHistory);
            var responseContent = response.Messages[0].Text.Replace("```json", "").Replace("```", "").Replace("\\n", " ");

            try
            {
                return JsonSerializer.Deserialize<EnterpriseSchemaAnalysis>(responseContent) ?? new EnterpriseSchemaAnalysis();
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to parse enterprise schema analysis: {response.Messages[0].Text}. Error: {e.Message}");
            }
        }

        // Advanced query pattern analysis with machine learning
        public async Task<EnterpriseQueryPatternAnalysis> AnalyzeQueryPatternsEnterprise(
            string aiModel,
            string aiService,
            List<string> historicalQueries,
            DatabaseSchema dbSchema,
            string databaseType,
            int timeframeDays = 30)
        {
            var aiClient = CreateChatClient(aiModel, aiService);
            var chatHistory = new List<ChatMessage>();
            var builder = new StringBuilder();

            builder.AppendLine("You are an enterprise query pattern analyst with expertise in:");
            builder.AppendLine("1. Query pattern recognition and classification");
            builder.AppendLine("2. Performance trend analysis and prediction");
            builder.AppendLine("3. Optimization opportunity identification");
            builder.AppendLine("4. Resource usage pattern analysis");
            builder.AppendLine("5. Security and compliance pattern analysis");

            builder.AppendLine("Database Schema:");
            foreach (var table in dbSchema.SchemaRaw)
            {
                builder.AppendLine(table);
            }

            builder.AppendLine($"Database Type: {databaseType}");
            builder.AppendLine($"Analysis Timeframe: {timeframeDays} days");

            builder.AppendLine("Historical Queries:");
            foreach (var query in historicalQueries)
            {
                builder.AppendLine($"- {query}");
            }

            builder.AppendLine("Provide response in this JSON format:");
            builder.AppendLine(@"{");
            builder.AppendLine(@"  ""patterns"": [""pattern1"", ""pattern2""],");
            builder.AppendLine(@"  ""recommendations"": [""rec1"", ""rec2""],");
            builder.AppendLine(@"  ""trends"": [""trend1"", ""trend2""],");
            builder.AppendLine(@"  ""opportunities"": [""opp1"", ""opp2""],");
            builder.AppendLine(@"  ""performanceMetrics"": {");
            builder.AppendLine(@"    ""averageComplexity"": 75.5,");
            builder.AppendLine(@"    ""commonBottlenecks"": [""bottleneck1"", ""bottleneck2""],");
            builder.AppendLine(@"    ""optimizationPotential"": 40.5");
            builder.AppendLine(@"  },");
            builder.AppendLine(@"  ""securityPatterns"": [""security1"", ""security2""],");
            builder.AppendLine(@"  ""resourceUsagePatterns"": [""resource1"", ""resource2""]");
            builder.AppendLine(@"}");

            chatHistory.Add(new ChatMessage(Microsoft.Extensions.AI.ChatRole.System, builder.ToString()));

            var response = await aiClient.GetResponseAsync(chatHistory);
            var responseContent = response.Messages[0].Text.Replace("```json", "").Replace("```", "").Replace("\\n", " ");

            try
            {
                return JsonSerializer.Deserialize<EnterpriseQueryPatternAnalysis>(responseContent) ?? new EnterpriseQueryPatternAnalysis();
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to parse enterprise query pattern analysis: {response.Messages[0].Text}. Error: {e.Message}");
            }
        }

        // Intelligent query validation and security analysis
        public async Task<EnterpriseQueryValidation> ValidateQueryEnterprise(
            string aiModel,
            string aiService,
            string sqlQuery,
            DatabaseSchema dbSchema,
            string databaseType)
        {
            var aiClient = CreateChatClient(aiModel, aiService);
            var chatHistory = new List<ChatMessage>();
            var builder = new StringBuilder();

            builder.AppendLine("You are an enterprise database security and validation expert. Analyze the following SQL query for:");
            builder.AppendLine("1. Security vulnerabilities and SQL injection risks");
            builder.AppendLine("2. Performance and resource usage concerns");
            builder.AppendLine("3. Compliance and best practice violations");
            builder.AppendLine("4. Data access and privacy considerations");
            builder.AppendLine("5. Scalability and maintenance issues");

            builder.AppendLine("Database Schema:");
            foreach (var table in dbSchema.SchemaRaw)
            {
                builder.AppendLine(table);
            }

            builder.AppendLine($"Database Type: {databaseType}");
            builder.AppendLine($"Query to Validate: {sqlQuery}");

            builder.AppendLine("Provide response in this JSON format:");
            builder.AppendLine(@"{");
            builder.AppendLine(@"  ""isValid"": true,");
            builder.AppendLine(@"  ""securityIssues"": [""issue1"", ""issue2""],");
            builder.AppendLine(@"  ""performanceConcerns"": [""concern1"", ""concern2""],");
            builder.AppendLine(@"  ""complianceIssues"": [""compliance1"", ""compliance2""],");
            builder.AppendLine(@"  ""recommendations"": [""rec1"", ""rec2""],");
            builder.AppendLine(@"  ""riskLevel"": ""low"",");
            builder.AppendLine(@"  ""validationScore"": 85.5");
            builder.AppendLine(@"}");

            chatHistory.Add(new ChatMessage(Microsoft.Extensions.AI.ChatRole.System, builder.ToString()));

            var response = await aiClient.GetResponseAsync(chatHistory);
            var responseContent = response.Messages[0].Text.Replace("```json", "").Replace("```", "").Replace("\\n", " ");

            try
            {
                return JsonSerializer.Deserialize<EnterpriseQueryValidation>(responseContent) ?? new EnterpriseQueryValidation();
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to parse enterprise query validation: {response.Messages[0].Text}. Error: {e.Message}");
            }
        }

        // Private helper methods
        private string GenerateCacheKey(string prompt, string aiModel, string aiService, string complexityLevel)
        {
            return $"{prompt}_{aiModel}_{aiService}_{complexityLevel}".GetHashCode().ToString();
        }

        private QueryValidationResult ValidateEnterpriseQuery(string sqlQuery, string databaseType)
        {
            var result = new QueryValidationResult { IsValid = true, Errors = new List<string>() };

            // Basic validation
            if (string.IsNullOrWhiteSpace(sqlQuery))
            {
                result.IsValid = false;
                result.Errors.Add("SQL query is empty");
                return result;
            }

            var upperQuery = sqlQuery.ToUpper();

            // Security validation
            var dangerousKeywords = new[] { "DROP", "DELETE", "TRUNCATE", "ALTER", "CREATE", "INSERT", "UPDATE" };
            foreach (var keyword in dangerousKeywords)
            {
                if (upperQuery.Contains(keyword))
                {
                    result.Errors.Add($"Query contains potentially dangerous operation: {keyword}");
                }
            }

            // Performance validation
            if (upperQuery.Contains("SELECT *"))
            {
                result.Errors.Add("Consider using specific column names instead of SELECT * for better performance");
            }

            if (upperQuery.Contains("CROSS JOIN") && !upperQuery.Contains("WHERE"))
            {
                result.Errors.Add("CROSS JOIN without WHERE clause may cause performance issues");
            }

            // Database-specific validation
            if (databaseType.Equals("SQLServer", StringComparison.OrdinalIgnoreCase))
            {
                if (upperQuery.Contains("NOLOCK"))
                {
                    result.Errors.Add("NOLOCK hint may lead to dirty reads and data inconsistency");
                }
            }

            result.IsValid = result.Errors.Count == 0;
            return result;
        }
    }

    // Enterprise result classes
    public class EnterpriseAIQuery
    {
        public string query { get; set; } = string.Empty;
        public string analysis { get; set; } = string.Empty;
        public List<string> optimizations { get; set; } = new List<string>();
        public string estimatedPerformance { get; set; } = string.Empty;
        public List<string> recommendations { get; set; } = new List<string>();
        public int complexityScore { get; set; }
        public List<string> securityConsiderations { get; set; } = new List<string>();
        public string scalabilityNotes { get; set; } = string.Empty;
        public List<string> validationErrors { get; set; } = new List<string>();
    }

    public class EnterpriseQueryOptimization
    {
        public string optimizedQuery { get; set; } = string.Empty;
        public string analysis { get; set; } = string.Empty;
        public List<string> recommendations { get; set; } = new List<string>();
        public double estimatedImprovement { get; set; }
        public List<string> optimizationStrategies { get; set; } = new List<string>();
        public PerformanceMetrics performanceMetrics { get; set; } = new PerformanceMetrics();
        public List<string> securityRecommendations { get; set; } = new List<string>();
        public string scalabilityNotes { get; set; } = string.Empty;
    }

    public class EnterpriseSchemaAnalysis
    {
        public List<string> recommendations { get; set; } = new List<string>();
        public List<string> insights { get; set; } = new List<string>();
        public List<string> opportunities { get; set; } = new List<string>();
        public List<string> indexingStrategies { get; set; } = new List<string>();
        public List<string> securityRecommendations { get; set; } = new List<string>();
        public List<string> scalabilityRecommendations { get; set; } = new List<string>();
        public PerformanceImpact performanceImpact { get; set; } = new PerformanceImpact();
    }

    public class EnterpriseQueryPatternAnalysis
    {
        public List<string> patterns { get; set; } = new List<string>();
        public List<string> recommendations { get; set; } = new List<string>();
        public List<string> trends { get; set; } = new List<string>();
        public List<string> opportunities { get; set; } = new List<string>();
        public QueryPerformanceMetrics performanceMetrics { get; set; } = new QueryPerformanceMetrics();
        public List<string> securityPatterns { get; set; } = new List<string>();
        public List<string> resourceUsagePatterns { get; set; } = new List<string>();
    }

    public class EnterpriseQueryValidation
    {
        public bool isValid { get; set; }
        public List<string> securityIssues { get; set; } = new List<string>();
        public List<string> performanceConcerns { get; set; } = new List<string>();
        public List<string> complianceIssues { get; set; } = new List<string>();
        public List<string> recommendations { get; set; } = new List<string>();
        public string riskLevel { get; set; } = string.Empty;
        public double validationScore { get; set; }
    }

    public class PerformanceMetrics
    {
        public string estimatedExecutionTime { get; set; } = string.Empty;
        public string estimatedMemoryUsage { get; set; } = string.Empty;
        public int complexityReduction { get; set; }
    }

    public class PerformanceImpact
    {
        public double estimatedImprovement { get; set; }
        public string implementationEffort { get; set; } = string.Empty;
        public string riskLevel { get; set; } = string.Empty;
    }

    public class QueryValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }

    public class AICache
    {
        public EnterpriseAIQuery Result { get; set; } = new EnterpriseAIQuery();
        public DateTime CachedAt { get; set; }
    }

    public class QueryPattern
    {
        public string Pattern { get; set; } = string.Empty;
        public int Frequency { get; set; }
        public double AveragePerformance { get; set; }
        public DateTime LastSeen { get; set; }
    }
}