using Microsoft.AspNetCore.Mvc;
using DBChatPro.Services;
using DbChatPro.API.Models;
using DBChatPro;

namespace DbChatPro.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DatabaseController : ControllerBase
    {
        private readonly IDatabaseService _databaseService;
        private readonly ILogger<DatabaseController> _logger;

        public DatabaseController(IDatabaseService databaseService, ILogger<DatabaseController> logger)
        {
            _databaseService = databaseService;
            _logger = logger;
        }

        [HttpPost("connect")]
        public async Task<ActionResult<DatabaseConnectionResponse>> Connect([FromBody] DatabaseConnectionRequest request)
        {
            try
            {
                _logger.LogInformation("Attempting to connect to database: {Name}", request.Name);

                var schema = await _databaseService.GetDatabaseSchema(request.DatabaseType, request.ConnectionString);
                
                return Ok(new DatabaseConnectionResponse
                {
                    Name = request.Name,
                    DatabaseType = request.DatabaseType,
                    IsConnected = true,
                    Schema = new DatabaseSchema
                    {
                        SchemaRaw = schema.SchemaRaw,
                        Tables = schema.Tables
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to database: {Name}", request.Name);
                
                return Ok(new DatabaseConnectionResponse
                {
                    Name = request.Name,
                    DatabaseType = request.DatabaseType,
                    IsConnected = false,
                    ErrorMessage = ex.Message
                });
            }
        }

        [HttpGet("supported-types")]
        public ActionResult<List<string>> GetSupportedDatabaseTypes()
        {
            return Ok(new List<string> { "MSSQL", "MYSQL", "POSTGRESQL", "ORACLE" });
        }
    }
}