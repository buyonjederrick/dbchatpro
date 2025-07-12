using Microsoft.AspNetCore.Mvc;
using DBChatPro.Services;
using DbChatPro.API.Models;
using DBChatPro;
using Microsoft.Extensions.AI;

namespace DbChatPro.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AIController : ControllerBase
    {
        private readonly AIService _aiService;
        private readonly IDatabaseService _databaseService;
        private readonly ILogger<AIController> _logger;

        public AIController(AIService aiService, IDatabaseService databaseService, ILogger<AIController> logger)
        {
            _aiService = aiService;
            _databaseService = databaseService;
            _logger = logger;
        }

        [HttpPost("query")]
        public async Task<ActionResult<AIQueryResponse>> GenerateQuery([FromBody] AIQueryRequest request)
        {
            try
            {
                _logger.LogInformation("Generating AI query for prompt: {Prompt}", request.Prompt);

                // Get database schema
                var schema = await _databaseService.GetDatabaseSchema(request.DatabaseType, request.ConnectionString);
                
                // Generate AI query
                var aiQuery = await _aiService.GetAISQLQuery(request.AiModel, request.AiService, request.Prompt, schema, request.DatabaseType);
                
                // Execute the query to get results
                List<Dictionary<string, object>>? results = null;
                try
                {
                    results = await _databaseService.ExecuteQuery(request.DatabaseType, request.ConnectionString, aiQuery.query);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to execute generated query, returning query only");
                }

                return Ok(new AIQueryResponse
                {
                    Summary = aiQuery.summary,
                    Query = aiQuery.query,
                    Results = results
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate AI query");
                
                return Ok(new AIQueryResponse
                {
                    Summary = string.Empty,
                    Query = string.Empty,
                    ErrorMessage = ex.Message
                });
            }
        }

        [HttpPost("chat")]
        public async Task<ActionResult<ChatResponse>> Chat([FromBody] ChatRequest request)
        {
            try
            {
                _logger.LogInformation("Processing chat request with {MessageCount} messages", request.Messages.Count);

                var chatMessages = request.Messages.Select(m => new ChatMessage(
                    m.Role switch
                    {
                        "user" => Microsoft.Extensions.AI.ChatRole.User,
                        "assistant" => Microsoft.Extensions.AI.ChatRole.Assistant,
                        "system" => Microsoft.Extensions.AI.ChatRole.System,
                        _ => Microsoft.Extensions.AI.ChatRole.User
                    },
                    m.Content
                )).ToList();

                var response = await _aiService.ChatPrompt(chatMessages, request.AiModel, request.AiService);
                
                return Ok(new ChatResponse
                {
                    Response = response.Messages[0].Text
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process chat request");
                
                return Ok(new ChatResponse
                {
                    Response = string.Empty,
                    ErrorMessage = ex.Message
                });
            }
        }

        [HttpGet("models")]
        public ActionResult<Dictionary<string, List<string>>> GetAvailableModels()
        {
            return Ok(new Dictionary<string, List<string>>
            {
                ["AzureOpenAI"] = new List<string> { "gpt-4", "gpt-4o", "gpt-35-turbo" },
                ["OpenAI"] = new List<string> { "gpt-4", "gpt-4o", "gpt-3.5-turbo" },
                ["Ollama"] = new List<string> { "llama2", "codellama", "mistral" },
                ["GitHubModels"] = new List<string> { "gpt-4", "gpt-4o", "gpt-3.5-turbo" },
                ["AWSBedrock"] = new List<string> { "anthropic.claude-3-sonnet-20240229-v1:0", "anthropic.claude-3-haiku-20240307-v1:0" }
            });
        }
    }
}