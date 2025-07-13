using DbChatPro.Core.Models;

namespace DbChatPro.Core.Services
{
    public interface IMCPService
    {
        Task<MCPQueryResult> ExecuteQueryAsync(string prompt, string aiModel, string aiPlatform, string? sessionId = null, string? userId = null);
        Task<string> GenerateSQLQueryAsync(string prompt, string aiModel, string aiPlatform);
        Task<DatabaseSchema> GetDatabaseSchemaAsync();
        Task<bool> TestConnectionAsync();
        Task<MCPConnectionStatus> GetConnectionStatusAsync();
        Task<MCPMetrics> GetMetricsAsync();
    }

    public class MCPQueryResult
    {
        public bool IsSuccessful { get; set; }
        public string GeneratedSql { get; set; } = string.Empty;
        public List<List<string>> Results { get; set; } = new List<List<string>>();
        public string? ErrorMessage { get; set; }
        public int ExecutionTimeMs { get; set; }
        public int RowsReturned { get; set; }
        public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
    }

    public class MCPConnectionStatus
    {
        public bool IsConnected { get; set; }
        public string DatabaseType { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public DateTime LastChecked { get; set; } = DateTime.UtcNow;
    }

    public class MCPMetrics
    {
        public int TotalQueriesExecuted { get; set; }
        public int SuccessfulQueries { get; set; }
        public int FailedQueries { get; set; }
        public double AverageExecutionTimeMs { get; set; }
        public DateTime LastQueryTime { get; set; }
        public Dictionary<string, int> QueriesByModel { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> QueriesByPlatform { get; set; } = new Dictionary<string, int>();
    }
}