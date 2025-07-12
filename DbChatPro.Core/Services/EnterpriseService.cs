using DbChatPro.Core.Models;
using DbChatPro.Core.Repositories;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace DbChatPro.Core.Services
{
    public class EnterpriseService : IEnterpriseService
    {
        private readonly IRepository<AuditLog> _auditLogRepository;
        private readonly IRepository<UserSession> _userSessionRepository;
        private readonly IRepository<SystemConfiguration> _systemConfigRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EnterpriseService(
            IRepository<AuditLog> auditLogRepository,
            IRepository<UserSession> userSessionRepository,
            IRepository<SystemConfiguration> systemConfigRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _auditLogRepository = auditLogRepository;
            _userSessionRepository = userSessionRepository;
            _systemConfigRepository = systemConfigRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<AuditLog> LogAuditEventAsync(string action, string entityName, Guid? entityId, string? userId = null, string? userName = null, string? oldValues = null, string? newValues = null, string? additionalData = null)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var ipAddress = httpContext?.Connection?.RemoteIpAddress?.ToString();
            var userAgent = httpContext?.Request?.Headers["User-Agent"].ToString();

            var auditLog = new AuditLog
            {
                Action = action,
                EntityName = entityName,
                EntityId = entityId,
                UserId = userId,
                UserName = userName,
                OldValues = oldValues,
                NewValues = newValues,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                AdditionalData = additionalData
            };

            return await _auditLogRepository.AddAsync(auditLog);
        }

        public async Task<UserSession> CreateUserSessionAsync(string sessionId, string? userId = null, string? userName = null, string? ipAddress = null, string? userAgent = null)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var actualIpAddress = ipAddress ?? httpContext?.Connection?.RemoteIpAddress?.ToString();
            var actualUserAgent = userAgent ?? httpContext?.Request?.Headers["User-Agent"].ToString();

            var userSession = new UserSession
            {
                SessionId = sessionId,
                UserId = userId,
                UserName = userName,
                IpAddress = actualIpAddress,
                UserAgent = actualUserAgent,
                LastActivity = DateTime.UtcNow,
                IsActive = true
            };

            return await _userSessionRepository.AddAsync(userSession);
        }

        public async Task UpdateUserSessionAsync(string sessionId, string? userId = null, string? userName = null)
        {
            var sessions = await _userSessionRepository.FindAsync(s => s.SessionId == sessionId);
            var session = sessions.FirstOrDefault();

            if (session != null)
            {
                session.LastActivity = DateTime.UtcNow;
                if (userId != null) session.UserId = userId;
                if (userName != null) session.UserName = userName;

                await _userSessionRepository.UpdateAsync(session);
            }
        }

        public async Task<bool> ValidateUserSessionAsync(string sessionId)
        {
            var sessions = await _userSessionRepository.FindAsync(s => s.SessionId == sessionId && s.IsActive);
            var session = sessions.FirstOrDefault();

            if (session != null)
            {
                // Update last activity
                session.LastActivity = DateTime.UtcNow;
                await _userSessionRepository.UpdateAsync(session);

                // Check if session is not expired (24 hours)
                return session.LastActivity > DateTime.UtcNow.AddHours(-24);
            }

            return false;
        }

        public async Task<IEnumerable<AuditLog>> GetAuditLogsAsync(DateTime? fromDate = null, DateTime? toDate = null, string? userId = null, string? action = null, int page = 1, int pageSize = 50)
        {
            var predicate = PredicateBuilder.True<AuditLog>();

            if (fromDate.HasValue)
                predicate = predicate.And(a => a.CreatedAt >= fromDate.Value);

            if (toDate.HasValue)
                predicate = predicate.And(a => a.CreatedAt <= toDate.Value);

            if (!string.IsNullOrEmpty(userId))
                predicate = predicate.And(a => a.UserId == userId);

            if (!string.IsNullOrEmpty(action))
                predicate = predicate.And(a => a.Action == action);

            return await _auditLogRepository.GetPagedAsync(page, pageSize, predicate, a => a.CreatedAt, false);
        }

        public async Task<SystemConfiguration?> GetSystemConfigurationAsync(string key)
        {
            var configs = await _systemConfigRepository.FindAsync(c => c.Key == key);
            return configs.FirstOrDefault();
        }

        public async Task<SystemConfiguration> SetSystemConfigurationAsync(string key, string value, string? category = null, string? description = null, bool isEncrypted = false)
        {
            var existingConfig = await GetSystemConfigurationAsync(key);

            if (existingConfig != null)
            {
                existingConfig.Value = value;
                existingConfig.Category = category ?? existingConfig.Category;
                existingConfig.Description = description ?? existingConfig.Description;
                existingConfig.IsEncrypted = isEncrypted;
                existingConfig.UpdatedAt = DateTime.UtcNow;

                return await _systemConfigRepository.UpdateAsync(existingConfig);
            }
            else
            {
                var newConfig = new SystemConfiguration
                {
                    Key = key,
                    Value = value,
                    Category = category,
                    Description = description,
                    IsEncrypted = isEncrypted
                };

                return await _systemConfigRepository.AddAsync(newConfig);
            }
        }

        public async Task<IEnumerable<SystemConfiguration>> GetSystemConfigurationsByCategoryAsync(string category)
        {
            return await _systemConfigRepository.FindAsync(c => c.Category == category);
        }
    }

    // Helper class for building predicates
    public static class PredicateBuilder
    {
        public static Expression<Func<T, bool>> True<T>() { return f => true; }
        public static Expression<Func<T, bool>> False<T>() { return f => false; }

        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>(Expression.OrElse(expr1.Body, invokedExpr), expr1.Parameters);
        }

        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(expr1.Body, invokedExpr), expr1.Parameters);
        }
    }
}