using System.ComponentModel.DataAnnotations;

namespace DbChatPro.API.Models
{
    public class DatabaseConnectionRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public string DatabaseType { get; set; } = string.Empty;
        
        [Required]
        public string ConnectionString { get; set; } = string.Empty;
    }

    public class DatabaseConnectionResponse
    {
        public string Name { get; set; } = string.Empty;
        public string DatabaseType { get; set; } = string.Empty;
        public bool IsConnected { get; set; }
        public string? ErrorMessage { get; set; }
        public DatabaseSchema? Schema { get; set; }
    }

    public class AIQueryRequest
    {
        [Required]
        public string Prompt { get; set; } = string.Empty;
        
        [Required]
        public string AiModel { get; set; } = string.Empty;
        
        [Required]
        public string AiService { get; set; } = string.Empty;
        
        [Required]
        public string DatabaseType { get; set; } = string.Empty;
        
        [Required]
        public string ConnectionString { get; set; } = string.Empty;
    }

    public class AIQueryResponse
    {
        public string Summary { get; set; } = string.Empty;
        public string Query { get; set; } = string.Empty;
        public List<Dictionary<string, object>>? Results { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class ChatRequest
    {
        [Required]
        public List<ChatMessage> Messages { get; set; } = new();
        
        [Required]
        public string AiModel { get; set; } = string.Empty;
        
        [Required]
        public string AiService { get; set; } = string.Empty;
    }

    public class ChatResponse
    {
        public string Response { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
    }

    public class ChatMessage
    {
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }

    public class DatabaseSchema
    {
        public List<string> SchemaRaw { get; set; } = new();
        public List<TableSchema> Tables { get; set; } = new();
    }

    public class TableSchema
    {
        public string TableName { get; set; } = string.Empty;
        public List<string> Columns { get; set; } = new();
    }
}