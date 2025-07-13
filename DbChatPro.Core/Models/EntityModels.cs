using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DbChatPro.Core.Models
{
    public abstract class BaseEntity
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        [StringLength(100)]
        public string? CreatedBy { get; set; }
        
        [StringLength(100)]
        public string? UpdatedBy { get; set; }
        
        public bool IsDeleted { get; set; } = false;
    }

    public class DatabaseConnection : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string DatabaseType { get; set; } = string.Empty;
        
        [Required]
        public string ConnectionString { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        [StringLength(100)]
        public string? Environment { get; set; }
        
        public virtual ICollection<QueryHistory> QueryHistories { get; set; } = new List<QueryHistory>();
        public virtual ICollection<DatabaseSchema> Schemas { get; set; } = new List<DatabaseSchema>();
    }

    public class QueryHistory : BaseEntity
    {
        [Required]
        public string UserPrompt { get; set; } = string.Empty;
        
        [Required]
        public string GeneratedSql { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string? AiModel { get; set; }
        
        [StringLength(50)]
        public string? AiService { get; set; }
        
        public int ExecutionTimeMs { get; set; }
        
        public bool IsSuccessful { get; set; }
        
        [StringLength(1000)]
        public string? ErrorMessage { get; set; }
        
        public int RowsReturned { get; set; }
        
        [Required]
        public Guid DatabaseConnectionId { get; set; }
        public virtual DatabaseConnection DatabaseConnection { get; set; } = null!;
        
        [StringLength(100)]
        public string? UserId { get; set; }
        
        [StringLength(100)]
        public string? SessionId { get; set; }
    }

    public class DatabaseSchema : BaseEntity
    {
        [Required]
        public string SchemaRaw { get; set; } = string.Empty;
        
        public virtual ICollection<DatabaseTable> Tables { get; set; } = new List<DatabaseTable>();
        
        [Required]
        public Guid DatabaseConnectionId { get; set; }
        public virtual DatabaseConnection DatabaseConnection { get; set; } = null!;
        
        public DateTime LastRefreshed { get; set; } = DateTime.UtcNow;
    }

    public class DatabaseTable : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string? Schema { get; set; }
        
        public virtual ICollection<DatabaseColumn> Columns { get; set; } = new List<DatabaseColumn>();
        
        public Guid DatabaseSchemaId { get; set; }
        public virtual DatabaseSchema DatabaseSchema { get; set; } = null!;
    }

    public class DatabaseColumn : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string? DataType { get; set; }
        
        public bool IsNullable { get; set; }
        
        public bool IsPrimaryKey { get; set; }
        
        public bool IsForeignKey { get; set; }
        
        [StringLength(100)]
        public string? ReferencedTable { get; set; }
        
        [StringLength(100)]
        public string? ReferencedColumn { get; set; }
        
        public Guid DatabaseTableId { get; set; }
        public virtual DatabaseTable DatabaseTable { get; set; } = null!;
    }

    public class UserSession : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string SessionId { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string? UserId { get; set; }
        
        [StringLength(100)]
        public string? UserName { get; set; }
        
        public DateTime LastActivity { get; set; } = DateTime.UtcNow;
        
        [StringLength(50)]
        public string? IpAddress { get; set; }
        
        [StringLength(500)]
        public string? UserAgent { get; set; }
        
        public bool IsActive { get; set; } = true;
    }

    public class SystemConfiguration : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string Key { get; set; } = string.Empty;
        
        [Required]
        public string Value { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string? Category { get; set; }
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public bool IsEncrypted { get; set; } = false;
    }

    public class AuditLog : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string Action { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string EntityName { get; set; } = string.Empty;
        
        public Guid? EntityId { get; set; }
        
        [StringLength(100)]
        public string? UserId { get; set; }
        
        [StringLength(100)]
        public string? UserName { get; set; }
        
        public string? OldValues { get; set; }
        
        public string? NewValues { get; set; }
        
        [StringLength(50)]
        public string? IpAddress { get; set; }
        
        [StringLength(500)]
        public string? UserAgent { get; set; }
        
        [StringLength(1000)]
        public string? AdditionalData { get; set; }
    }
}