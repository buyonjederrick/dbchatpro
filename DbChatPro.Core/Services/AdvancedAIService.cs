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

namespace DBChatPro.Services
{
    public class AdvancedAIService : AIService
    {
        private readonly IConfiguration _config;
        private readonly IServiceProvider _serviceProvider;

        public AdvancedAIService(IConfiguration config, IServiceProvider serviceProvider) : base(config, serviceProvider)
        {
            _config = config;
            _serviceProvider = serviceProvider;
        }

        // Advanced SQL query generation with optimization
        public async Task<AdvancedAIQuery> GetAdvancedAISQLQuery(
            string aiModel, 
            string aiService, 
            string userPrompt, 
            DatabaseSchema dbSchema, 
            string databaseType,
            string optimizationLevel = "Advanced")
        {
            var aiClient = CreateChatClient(aiModel, aiService);
            var chatHistory = new List<ChatMessage>();
            var builder = new StringBuilder();

            // Enhanced system prompt for advanced query generation
            builder.AppendLine("You are an expert database query optimizer and SQL generator. Your role is to:");
            builder.AppendLine("1. Generate highly optimized SQL queries");
            builder.AppendLine("2. Provide detailed analysis of query performance");
            builder.AppendLine("3. Suggest optimizations for better performance");
            builder.AppendLine("4. Consider indexing strategies and query execution plans");
            builder.AppendLine("5. Ensure queries are efficient and follow best practices");

            builder.AppendLine("Database Schema:");
            foreach (var table in dbSchema.SchemaRaw)
            {
                builder.AppendLine(table);
            }

            builder.AppendLine($"Database Type: {databaseType}");
            builder.AppendLine($"Optimization Level: {optimizationLevel}");

            // Add optimization-specific instructions
            switch (optimizationLevel.ToLower())
            {
                case "expert":
                    builder.AppendLine("EXPERT MODE: Apply advanced optimization techniques including:");
                    builder.AppendLine("- Query plan analysis and optimization");
                    builder.AppendLine("- Index usage optimization");
                    builder.AppendLine("- Subquery optimization");
                    builder.AppendLine("- JOIN optimization strategies");
                    builder.AppendLine("- Memory and CPU usage considerations");
                    break;
                case "advanced":
                    builder.AppendLine("ADVANCED MODE: Apply intermediate optimization techniques including:");
                    builder.AppendLine("- Basic query optimization");
                    builder.AppendLine("- Index recommendations");
                    builder.AppendLine("- JOIN efficiency improvements");
                    break;
                default:
                    builder.AppendLine("BASIC MODE: Apply fundamental optimization techniques");
                    break;
            }

            builder.AppendLine("Provide response in this JSON format:");
            builder.AppendLine(@"{");
            builder.AppendLine(@"  ""query"": ""optimized SQL query"",");
            builder.AppendLine(@"  ""analysis"": ""detailed performance analysis"",");
            builder.AppendLine(@"  ""optimizations"": [""optimization1"", ""optimization2""],");
            builder.AppendLine(@"  ""estimatedPerformance"": ""performance estimate"",");
            builder.AppendLine(@"  ""recommendations"": [""recommendation1"", ""recommendation2""]");
            builder.AppendLine(@"}");

            chatHistory.Add(new ChatMessage(Microsoft.Extensions.AI.ChatRole.System, builder.ToString()));
            chatHistory.Add(new ChatMessage(Microsoft.Extensions.AI.ChatRole.User, userPrompt));

            var response = await aiClient.GetResponseAsync(chatHistory);
            var responseContent = response.Messages[0].Text.Replace("```json", "").Replace("```", "").Replace("\\n", " ");

            try
            {
                return JsonSerializer.Deserialize<AdvancedAIQuery>(responseContent) ?? new AdvancedAIQuery();
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to parse advanced AI response: {response.Messages[0].Text}. Error: {e.Message}");
            }
        }

        // Query performance analysis
        public async Task<QueryPerformanceAnalysis> AnalyzeQueryPerformance(
            string aiModel,
            string aiService,
            string sqlQuery,
            DatabaseSchema dbSchema,
            string databaseType,
            string performanceRequirement = "Balanced")
        {
            var aiClient = CreateChatClient(aiModel, aiService);
            var chatHistory = new List<ChatMessage>();
            var builder = new StringBuilder();

            builder.AppendLine("You are a database performance analyst. Analyze the following SQL query and provide:");
            builder.AppendLine("1. Performance analysis and bottlenecks");
            builder.AppendLine("2. Optimized version of the query");
            builder.AppendLine("3. Specific recommendations for improvement");
            builder.AppendLine("4. Estimated performance improvement percentage");

            builder.AppendLine("Database Schema:");
            foreach (var table in dbSchema.SchemaRaw)
            {
                builder.AppendLine(table);
            }

            builder.AppendLine($"Database Type: {databaseType}");
            builder.AppendLine($"Performance Requirement: {performanceRequirement}");
            builder.AppendLine($"Query to Analyze: {sqlQuery}");

            builder.AppendLine("Provide response in this JSON format:");
            builder.AppendLine(@"{");
            builder.AppendLine(@"  ""optimizedQuery"": ""optimized version"",");
            builder.AppendLine(@"  ""analysis"": ""detailed performance analysis"",");
            builder.AppendLine(@"  ""recommendations"": [""rec1"", ""rec2""],");
            builder.AppendLine(@"  ""estimatedImprovement"": 25.5");
            builder.AppendLine(@"}");

            chatHistory.Add(new ChatMessage(Microsoft.Extensions.AI.ChatRole.System, builder.ToString()));

            var response = await aiClient.GetResponseAsync(chatHistory);
            var responseContent = response.Messages[0].Text.Replace("```json", "").Replace("```", "").Replace("\\n", " ");

            try
            {
                return JsonSerializer.Deserialize<QueryPerformanceAnalysis>(responseContent) ?? new QueryPerformanceAnalysis();
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to parse query performance analysis: {response.Messages[0].Text}. Error: {e.Message}");
            }
        }

        // Database schema analysis
        public async Task<SchemaAnalysis> AnalyzeDatabaseSchema(
            string aiModel,
            string aiService,
            DatabaseSchema dbSchema,
            string databaseType,
            string analysisDepth = "Detailed")
        {
            var aiClient = CreateChatClient(aiModel, aiService);
            var chatHistory = new List<ChatMessage>();
            var builder = new StringBuilder();

            builder.AppendLine("You are a database schema analyst. Analyze the following database schema and provide:");
            builder.AppendLine("1. Schema design recommendations");
            builder.AppendLine("2. Performance optimization opportunities");
            builder.AppendLine("3. Indexing recommendations");
            builder.AppendLine("4. Data modeling improvements");

            builder.AppendLine("Database Schema:");
            foreach (var table in dbSchema.SchemaRaw)
            {
                builder.AppendLine(table);
            }

            builder.AppendLine($"Database Type: {databaseType}");
            builder.AppendLine($"Analysis Depth: {analysisDepth}");

            builder.AppendLine("Provide response in this JSON format:");
            builder.AppendLine(@"{");
            builder.AppendLine(@"  ""recommendations"": [""rec1"", ""rec2""],");
            builder.AppendLine(@"  ""insights"": [""insight1"", ""insight2""],");
            builder.AppendLine(@"  ""opportunities"": [""opp1"", ""opp2""]");
            builder.AppendLine(@"}");

            chatHistory.Add(new ChatMessage(Microsoft.Extensions.AI.ChatRole.System, builder.ToString()));

            var response = await aiClient.GetResponseAsync(chatHistory);
            var responseContent = response.Messages[0].Text.Replace("```json", "").Replace("```", "").Replace("\\n", " ");

            try
            {
                return JsonSerializer.Deserialize<SchemaAnalysis>(responseContent) ?? new SchemaAnalysis();
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to parse schema analysis: {response.Messages[0].Text}. Error: {e.Message}");
            }
        }

        // Query pattern analysis
        public async Task<QueryPatternAnalysis> AnalyzeQueryPatterns(
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

            builder.AppendLine("You are a query pattern analyst. Analyze the following historical queries and provide:");
            builder.AppendLine("1. Identified query patterns");
            builder.AppendLine("2. Performance trends");
            builder.AppendLine("3. Optimization opportunities");
            builder.AppendLine("4. Recommendations for query improvement");

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
            builder.AppendLine(@"  ""opportunities"": [""opp1"", ""opp2""]");
            builder.AppendLine(@"}");

            chatHistory.Add(new ChatMessage(Microsoft.Extensions.AI.ChatRole.System, builder.ToString()));

            var response = await aiClient.GetResponseAsync(chatHistory);
            var responseContent = response.Messages[0].Text.Replace("```json", "").Replace("```", "").Replace("\\n", " ");

            try
            {
                return JsonSerializer.Deserialize<QueryPatternAnalysis>(responseContent) ?? new QueryPatternAnalysis();
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to parse query pattern analysis: {response.Messages[0].Text}. Error: {e.Message}");
            }
        }

        // Intelligent query caching and optimization
        public async Task<CachedQueryResult> GetCachedOrGenerateQuery(
            string aiModel,
            string aiService,
            string userPrompt,
            DatabaseSchema dbSchema,
            string databaseType,
            Dictionary<string, CachedQueryResult> queryCache)
        {
            // Simple cache key generation (in production, use more sophisticated hashing)
            var cacheKey = $"{userPrompt}_{databaseType}_{aiModel}";

            if (queryCache.ContainsKey(cacheKey))
            {
                var cached = queryCache[cacheKey];
                if (cached.CachedAt.AddMinutes(30) > DateTime.UtcNow) // 30-minute cache
                {
                    return cached;
                }
            }

            // Generate new query
            var result = await GetAdvancedAISQLQuery(aiModel, aiService, userPrompt, dbSchema, databaseType);
            
            var cachedResult = new CachedQueryResult
            {
                Query = result.query,
                Analysis = result.analysis,
                Optimizations = result.optimizations,
                CachedAt = DateTime.UtcNow
            };

            queryCache[cacheKey] = cachedResult;
            return cachedResult;
        }

        // Query complexity analysis
        public async Task<QueryComplexityAnalysis> AnalyzeQueryComplexity(
            string aiModel,
            string aiService,
            string sqlQuery,
            DatabaseSchema dbSchema,
            string databaseType)
        {
            var aiClient = CreateChatClient(aiModel, aiService);
            var chatHistory = new List<ChatMessage>();
            var builder = new StringBuilder();

            builder.AppendLine("You are a query complexity analyzer. Analyze the following SQL query and provide:");
            builder.AppendLine("1. Complexity score (1-100)");
            builder.AppendLine("2. Performance impact assessment");
            builder.AppendLine("3. Scalability considerations");
            builder.AppendLine("4. Resource usage estimation");

            builder.AppendLine("Database Schema:");
            foreach (var table in dbSchema.SchemaRaw)
            {
                builder.AppendLine(table);
            }

            builder.AppendLine($"Database Type: {databaseType}");
            builder.AppendLine($"Query to Analyze: {sqlQuery}");

            builder.AppendLine("Provide response in this JSON format:");
            builder.AppendLine(@"{");
            builder.AppendLine(@"  ""complexityScore"": 75,");
            builder.AppendLine(@"  ""performanceImpact"": ""High"",");
            builder.AppendLine(@"  ""scalabilityIssues"": [""issue1"", ""issue2""],");
            builder.AppendLine(@"  ""resourceUsage"": ""High CPU and Memory"",");
            builder.AppendLine(@"  ""recommendations"": [""rec1"", ""rec2""]");
            builder.AppendLine(@"}");

            chatHistory.Add(new ChatMessage(Microsoft.Extensions.AI.ChatRole.System, builder.ToString()));

            var response = await aiClient.GetResponseAsync(chatHistory);
            var responseContent = response.Messages[0].Text.Replace("```json", "").Replace("```", "").Replace("\\n", " ");

            try
            {
                return JsonSerializer.Deserialize<QueryComplexityAnalysis>(responseContent) ?? new QueryComplexityAnalysis();
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to parse query complexity analysis: {response.Messages[0].Text}. Error: {e.Message}");
            }
        }
    }

    // Advanced result classes
    public class AdvancedAIQuery
    {
        public string query { get; set; } = string.Empty;
        public string analysis { get; set; } = string.Empty;
        public List<string> optimizations { get; set; } = new List<string>();
        public string estimatedPerformance { get; set; } = string.Empty;
        public List<string> recommendations { get; set; } = new List<string>();
    }

    public class QueryPerformanceAnalysis
    {
        public string optimizedQuery { get; set; } = string.Empty;
        public string analysis { get; set; } = string.Empty;
        public List<string> recommendations { get; set; } = new List<string>();
        public double estimatedImprovement { get; set; }
    }

    public class SchemaAnalysis
    {
        public List<string> recommendations { get; set; } = new List<string>();
        public List<string> insights { get; set; } = new List<string>();
        public List<string> opportunities { get; set; } = new List<string>();
    }

    public class QueryPatternAnalysis
    {
        public List<string> patterns { get; set; } = new List<string>();
        public List<string> recommendations { get; set; } = new List<string>();
        public List<string> trends { get; set; } = new List<string>();
        public List<string> opportunities { get; set; } = new List<string>();
    }

    public class CachedQueryResult
    {
        public string Query { get; set; } = string.Empty;
        public string Analysis { get; set; } = string.Empty;
        public List<string> Optimizations { get; set; } = new List<string>();
        public DateTime CachedAt { get; set; }
    }

    public class QueryComplexityAnalysis
    {
        public int complexityScore { get; set; }
        public string performanceImpact { get; set; } = string.Empty;
        public List<string> scalabilityIssues { get; set; } = new List<string>();
        public string resourceUsage { get; set; } = string.Empty;
        public List<string> recommendations { get; set; } = new List<string>();
    }
}