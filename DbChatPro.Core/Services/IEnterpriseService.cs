using DbChatPro.Core.Models;

namespace DbChatPro.Core.Services
{
    public interface IEnterpriseService
    {
        Task<AuditLog> LogAuditEventAsync(string action, string entityName, Guid? entityId, string? userId = null, string? userName = null, string? oldValues = null, string? newValues = null, string? additionalData = null);
        Task<UserSession> CreateUserSessionAsync(string sessionId, string? userId = null, string? userName = null, string? ipAddress = null, string? userAgent = null);
        Task UpdateUserSessionAsync(string sessionId, string? userId = null, string? userName = null);
        Task<bool> ValidateUserSessionAsync(string sessionId);
        Task<IEnumerable<AuditLog>> GetAuditLogsAsync(DateTime? fromDate = null, DateTime? toDate = null, string? userId = null, string? action = null, int page = 1, int pageSize = 50);
        Task<SystemConfiguration?> GetSystemConfigurationAsync(string key);
        Task<SystemConfiguration> SetSystemConfigurationAsync(string key, string value, string? category = null, string? description = null, bool isEncrypted = false);
        Task<IEnumerable<SystemConfiguration>> GetSystemConfigurationsByCategoryAsync(string category);
    }
}