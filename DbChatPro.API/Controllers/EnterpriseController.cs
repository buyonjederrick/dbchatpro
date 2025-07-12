using Microsoft.AspNetCore.Mvc;
using DbChatPro.Core.Services;
using DbChatPro.Core.Models;
using DbChatPro.Core.Repositories;
using System.Text.Json;

namespace DbChatPro.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EnterpriseController : ControllerBase
    {
        private readonly IEnterpriseService _enterpriseService;
        private readonly IMCPService _mcpService;
        private readonly IRepository<DatabaseConnection> _connectionRepository;
        private readonly IRepository<QueryHistory> _queryHistoryRepository;
        private readonly ILogger<EnterpriseController> _logger;

        public EnterpriseController(
            IEnterpriseService enterpriseService,
            IMCPService mcpService,
            IRepository<DatabaseConnection> connectionRepository,
            IRepository<QueryHistory> queryHistoryRepository,
            ILogger<EnterpriseController> logger)
        {
            _enterpriseService = enterpriseService;
            _mcpService = mcpService;
            _connectionRepository = connectionRepository;
            _queryHistoryRepository = queryHistoryRepository;
            _logger = logger;
        }

        [HttpGet("audit-logs")]
        public async Task<ActionResult<PagedResult<AuditLog>>> GetAuditLogs(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] string? userId = null,
            [FromQuery] string? action = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var auditLogs = await _enterpriseService.GetAuditLogsAsync(fromDate, toDate, userId, action, page, pageSize);
                var totalCount = await GetAuditLogsCountAsync(fromDate, toDate, userId, action);

                return Ok(new PagedResult<AuditLog>
                {
                    Data = auditLogs,
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get audit logs");
                return StatusCode(500, new { error = "Failed to retrieve audit logs" });
            }
        }

        [HttpGet("metrics")]
        public async Task<ActionResult<object>> GetMetrics()
        {
            try
            {
                var mcpMetrics = await _mcpService.GetMetricsAsync();
                var connectionStatus = await _mcpService.GetConnectionStatusAsync();

                var totalConnections = await _connectionRepository.CountAsync();
                var activeConnections = await _connectionRepository.CountAsync(c => c.IsActive);

                var recentQueries = await _queryHistoryRepository.FindAsync(q => q.CreatedAt >= DateTime.UtcNow.AddDays(-7));
                var weeklyStats = recentQueries
                    .GroupBy(q => q.CreatedAt.Date)
                    .Select(g => new { Date = g.Key, Count = g.Count(), Successful = g.Count(q => q.IsSuccessful) })
                    .OrderBy(x => x.Date)
                    .ToList();

                return Ok(new
                {
                    mcpMetrics,
                    connectionStatus,
                    connections = new { total = totalConnections, active = activeConnections },
                    weeklyStats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get metrics");
                return StatusCode(500, new { error = "Failed to retrieve metrics" });
            }
        }

        [HttpGet("system-config")]
        public async Task<ActionResult<IEnumerable<SystemConfiguration>>> GetSystemConfigurations([FromQuery] string? category = null)
        {
            try
            {
                IEnumerable<SystemConfiguration> configs;
                if (!string.IsNullOrEmpty(category))
                {
                    configs = await _enterpriseService.GetSystemConfigurationsByCategoryAsync(category);
                }
                else
                {
                    // Get all configurations
                    var allConfigs = await _enterpriseService.GetSystemConfigurationAsync(""); // This will need to be modified
                    configs = new List<SystemConfiguration>();
                }

                return Ok(configs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get system configurations");
                return StatusCode(500, new { error = "Failed to retrieve system configurations" });
            }
        }

        [HttpPost("system-config")]
        public async Task<ActionResult<SystemConfiguration>> SetSystemConfiguration([FromBody] SystemConfigurationRequest request)
        {
            try
            {
                var config = await _enterpriseService.SetSystemConfigurationAsync(
                    request.Key,
                    request.Value,
                    request.Category,
                    request.Description,
                    request.IsEncrypted
                );

                await _enterpriseService.LogAuditEventAsync(
                    "SYSTEM_CONFIG_UPDATED",
                    "SystemConfiguration",
                    config.Id,
                    request.UserId,
                    request.UserName,
                    null,
                    JsonSerializer.Serialize(new { key = request.Key, category = request.Category })
                );

                return Ok(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set system configuration");
                return StatusCode(500, new { error = "Failed to set system configuration" });
            }
        }

        [HttpPost("session")]
        public async Task<ActionResult<UserSession>> CreateSession([FromBody] CreateSessionRequest request)
        {
            try
            {
                var session = await _enterpriseService.CreateUserSessionAsync(
                    request.SessionId,
                    request.UserId,
                    request.UserName,
                    request.IpAddress,
                    request.UserAgent
                );

                return Ok(session);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create user session");
                return StatusCode(500, new { error = "Failed to create user session" });
            }
        }

        [HttpPut("session/{sessionId}")]
        public async Task<ActionResult> UpdateSession(string sessionId, [FromBody] UpdateSessionRequest request)
        {
            try
            {
                await _enterpriseService.UpdateUserSessionAsync(sessionId, request.UserId, request.UserName);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update user session");
                return StatusCode(500, new { error = "Failed to update user session" });
            }
        }

        [HttpGet("session/{sessionId}/validate")]
        public async Task<ActionResult<bool>> ValidateSession(string sessionId)
        {
            try
            {
                var isValid = await _enterpriseService.ValidateUserSessionAsync(sessionId);
                return Ok(isValid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate user session");
                return StatusCode(500, new { error = "Failed to validate user session" });
            }
        }

        [HttpGet("mcp/status")]
        public async Task<ActionResult<MCPConnectionStatus>> GetMCPStatus()
        {
            try
            {
                var status = await _mcpService.GetConnectionStatusAsync();
                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get MCP status");
                return StatusCode(500, new { error = "Failed to get MCP status" });
            }
        }

        [HttpPost("mcp/query")]
        public async Task<ActionResult<MCPQueryResult>> ExecuteMCPQuery([FromBody] MCPQueryRequest request)
        {
            try
            {
                var result = await _mcpService.ExecuteQueryAsync(
                    request.Prompt,
                    request.AiModel,
                    request.AiPlatform,
                    request.SessionId,
                    request.UserId
                );

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to execute MCP query");
                return StatusCode(500, new { error = "Failed to execute MCP query" });
            }
        }

        private async Task<int> GetAuditLogsCountAsync(DateTime? fromDate, DateTime? toDate, string? userId, string? action)
        {
            // This is a simplified implementation - in a real scenario, you'd have a dedicated method
            return 0;
        }
    }

    public class PagedResult<T>
    {
        public IEnumerable<T> Data { get; set; } = new List<T>();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }

    public class SystemConfigurationRequest
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string? Category { get; set; }
        public string? Description { get; set; }
        public bool IsEncrypted { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
    }

    public class CreateSessionRequest
    {
        public string SessionId { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
    }

    public class UpdateSessionRequest
    {
        public string? UserId { get; set; }
        public string? UserName { get; set; }
    }

    public class MCPQueryRequest
    {
        public string Prompt { get; set; } = string.Empty;
        public string AiModel { get; set; } = string.Empty;
        public string AiPlatform { get; set; } = string.Empty;
        public string? SessionId { get; set; }
        public string? UserId { get; set; }
    }
}