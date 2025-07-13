using Microsoft.AspNetCore.Mvc;
using DBChatPro.Services;
using DBChatPro.Models;
using System.Text.Json;
using System.Text;

namespace DbChatPro.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MCPController : ControllerBase
    {
        private readonly SqlServerDatabaseService _dataService;
        private readonly AIService _aiService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<MCPController> _logger;

        public MCPController(
            SqlServerDatabaseService dataService,
            AIService aiService,
            IConfiguration configuration,
            ILogger<MCPController> logger)
        {
            _dataService = dataService;
            _aiService = aiService;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("execute")]
        public async Task<ActionResult<MCPExecuteResponse>> ExecuteQuery([FromBody] MCPExecuteRequest request)
        {
            try
            {
                _logger.LogInformation("Executing MCP query: {Prompt}", request.Prompt);

                // Validate required parameters
                if (string.IsNullOrEmpty(request.DatabaseConnectionString))
                {
                    return BadRequest(new { error = "DATABASECONNECTIONSTRING is required" });
                }
                if (string.IsNullOrEmpty(request.DatabaseType))
                {
                    return BadRequest(new { error = "DATABASETYPE is required" });
                }
                if (string.IsNullOrEmpty(request.AiModel))
                {
                    return BadRequest(new { error = "aiModel is required" });
                }
                if (string.IsNullOrEmpty(request.AiPlatform))
                {
                    return BadRequest(new { error = "aiPlatform is required" });
                }
                if (string.IsNullOrEmpty(request.Prompt))
                {
                    return BadRequest(new { error = "prompt is required" });
                }

                // Build connection and get schema
                var connection = new AIConnection() { ConnectionString = request.DatabaseConnectionString };
                var dbSchema = await _dataService.GenerateSchema(connection);

                // Get AI-generated SQL and run it
                var aiResponse = await _aiService.GetAISQLQuery(request.AiModel, request.AiPlatform, request.Prompt, dbSchema, request.DatabaseType);
                var rowData = await _dataService.GetDataTable(connection, aiResponse.query);

                return Ok(new MCPExecuteResponse
                {
                    Query = aiResponse.query,
                    Results = rowData,
                    IsSuccessful = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to execute MCP query");
                return Ok(new MCPExecuteResponse
                {
                    IsSuccessful = false,
                    ErrorMessage = ex.Message
                });
            }
        }

        [HttpPost("generate-sql")]
        public async Task<ActionResult<MCPGenerateSQLResponse>> GenerateSQL([FromBody] MCPGenerateSQLRequest request)
        {
            try
            {
                _logger.LogInformation("Generating SQL for prompt: {Prompt}", request.Prompt);

                // Validate required parameters
                if (string.IsNullOrEmpty(request.DatabaseConnectionString))
                {
                    return BadRequest(new { error = "DATABASECONNECTIONSTRING is required" });
                }
                if (string.IsNullOrEmpty(request.DatabaseType))
                {
                    return BadRequest(new { error = "DATABASETYPE is required" });
                }
                if (string.IsNullOrEmpty(request.AiModel))
                {
                    return BadRequest(new { error = "aiModel is required" });
                }
                if (string.IsNullOrEmpty(request.AiPlatform))
                {
                    return BadRequest(new { error = "aiPlatform is required" });
                }
                if (string.IsNullOrEmpty(request.Prompt))
                {
                    return BadRequest(new { error = "prompt is required" });
                }

                // Build connection and get schema
                var connection = new AIConnection() { ConnectionString = request.DatabaseConnectionString };
                var dbSchema = await _dataService.GenerateSchema(connection);

                // Get AI-generated SQL
                var aiResponse = await _aiService.GetAISQLQuery(request.AiModel, request.AiPlatform, request.Prompt, dbSchema, request.DatabaseType);

                return Ok(new MCPGenerateSQLResponse
                {
                    Query = aiResponse.query,
                    IsSuccessful = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate SQL");
                return Ok(new MCPGenerateSQLResponse
                {
                    IsSuccessful = false,
                    ErrorMessage = ex.Message
                });
            }
        }

        [HttpGet("schema")]
        public async Task<ActionResult<MCPSchemaResponse>> GetSchema()
        {
            try
            {
                var databaseConnectionString = _configuration.GetValue<string>("DATABASECONNECTIONSTRING");
                if (string.IsNullOrEmpty(databaseConnectionString))
                {
                    return BadRequest(new { error = "DATABASECONNECTIONSTRING is not set in the configuration" });
                }

                // Build connection and get schema
                var connection = new AIConnection() { ConnectionString = databaseConnectionString };
                var dbSchema = await _dataService.GenerateSchema(connection);

                return Ok(new MCPSchemaResponse
                {
                    SchemaRaw = dbSchema.schemaRaw,
                    IsSuccessful = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get schema");
                return Ok(new MCPSchemaResponse
                {
                    IsSuccessful = false,
                    ErrorMessage = ex.Message
                });
            }
        }

        [HttpGet("status")]
        public async Task<ActionResult<MCPStatusResponse>> GetStatus()
        {
            try
            {
                var databaseConnectionString = _configuration.GetValue<string>("DATABASECONNECTIONSTRING");
                if (string.IsNullOrEmpty(databaseConnectionString))
                {
                    return Ok(new MCPStatusResponse
                    {
                        IsConnected = false,
                        ErrorMessage = "DATABASECONNECTIONSTRING is not set in the configuration"
                    });
                }

                // Test connection by trying to get schema
                var connection = new AIConnection() { ConnectionString = databaseConnectionString };
                await _dataService.GenerateSchema(connection);

                return Ok(new MCPStatusResponse
                {
                    IsConnected = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get MCP status");
                return Ok(new MCPStatusResponse
                {
                    IsConnected = false,
                    ErrorMessage = ex.Message
                });
            }
        }

        // Request/Response classes
        public class MCPExecuteRequest
        {
            public string Prompt { get; set; } = string.Empty;
            public string AiModel { get; set; } = string.Empty;
            public string AiPlatform { get; set; } = string.Empty;
            public string DatabaseType { get; set; } = string.Empty;
            public string DatabaseConnectionString { get; set; } = string.Empty;
        }

        public class MCPExecuteResponse
        {
            public string? Query { get; set; }
            public List<List<string>>? Results { get; set; }
            public bool IsSuccessful { get; set; }
            public string? ErrorMessage { get; set; }
        }

        public class MCPGenerateSQLRequest
        {
            public string Prompt { get; set; } = string.Empty;
            public string AiModel { get; set; } = string.Empty;
            public string AiPlatform { get; set; } = string.Empty;
            public string DatabaseType { get; set; } = string.Empty;
            public string DatabaseConnectionString { get; set; } = string.Empty;
        }

        public class MCPGenerateSQLResponse
        {
            public string? Query { get; set; }
            public bool IsSuccessful { get; set; }
            public string? ErrorMessage { get; set; }
        }

        public class MCPSchemaResponse
        {
            public string[]? SchemaRaw { get; set; }
            public bool IsSuccessful { get; set; }
            public string? ErrorMessage { get; set; }
        }

        public class MCPStatusResponse
        {
            public bool IsConnected { get; set; }
            public string? ErrorMessage { get; set; }
        }
    }
}