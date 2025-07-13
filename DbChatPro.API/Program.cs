using Amazon.BedrockRuntime;
using Azure.AI.OpenAI;
using Azure.Identity;
using DBChatPro.Services;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Azure;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using DbChatPro.Core.Data;
using DbChatPro.Core.Repositories;
using DbChatPro.Core.Services;
using DbChatPro.Core.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "DBChatPro API", 
        Version = "v1",
        Description = "API for AI-powered database querying and management"
    });
});

// Add CORS for React clients
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173", "http://localhost:4173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add Entity Framework
builder.Services.AddDbContext<DbChatProContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Register enterprise services
builder.Services.AddScoped<IEnterpriseService, EnterpriseService>();
builder.Services.AddScoped<IMCPService, MCPService>();

// Register DBChatPro services
builder.Services.AddScoped<AIService>();
builder.Services.AddScoped<IDatabaseService, DatabaseManagerService>();
builder.Services.AddScoped<MySqlDatabaseService>();
builder.Services.AddScoped<SqlServerDatabaseService>();
builder.Services.AddScoped<PostgresDatabaseService>();
builder.Services.AddScoped<OracleDatabaseService>();

// Add HTTP context accessor for audit logging
builder.Services.AddHttpContextAccessor();

// Configure AWS Bedrock if profile is specified
if (!string.IsNullOrEmpty(builder.Configuration["AWS:Profile"]))
{
    builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
    builder.Services.AddAWSService<IAmazonBedrockRuntime>();
}

// Configure Azure services
#region Credential chain
var userAssignedIdentityCredential = 
    new ManagedIdentityCredential(builder.Configuration.GetValue<string>("AZURE_CLIENT_ID"));
    
var visualStudioCredential = new VisualStudioCredential(
    new VisualStudioCredentialOptions()
    { 
        TenantId = builder.Configuration.GetValue<string>("AZURE_TENANT_ID") 
    });

var azureDevCliCredential = new AzureDeveloperCliCredential(
    new AzureDeveloperCliCredentialOptions()
    {
        TenantId = builder.Configuration.GetValue<string>("AZURE_TENANT_ID")
    });

var azureCliCredential = new AzureCliCredential(
    new AzureCliCredentialOptions()
    {
        TenantId = builder.Configuration.GetValue<string>("AZURE_TENANT_ID")
    });

var credential = new ChainedTokenCredential(userAssignedIdentityCredential, azureDevCliCredential, visualStudioCredential, azureCliCredential);
#endregion

// Use in-memory services in local mode
if (builder.Configuration["EnvironmentMode"] == "local")
{
    builder.Services.AddSingleton<IQueryService, InMemoryQueryService>();
    builder.Services.AddSingleton<IConnectionService, InMemoryConnectionService>();
}
// AZURE HOSTED ONLY FOR USE WITH AZURE DEVELOPER CLI
else if (builder.Configuration["EnvironmentMode"] == "azure")
{
    var azureOpenAIEndpoint = new Uri(builder.Configuration["AZURE_OPENAI_ENDPOINT"]);
    var azureTableEndpoint = new Uri(builder.Configuration["AZURE_STORAGE_ENDPOINT"]);
    var azureKeyVaultEndpoint = new Uri(builder.Configuration["AZURE_KEYVAULT_ENDPOINT"]);

    builder.Services.AddAzureClients(async clientBuilder =>
    {
        clientBuilder.AddTableServiceClient(azureTableEndpoint);
        clientBuilder.AddSecretClient(azureKeyVaultEndpoint);

        clientBuilder.AddClient<AzureOpenAIClient, AzureOpenAIClientOptions>(
            (options, _, _) => new AzureOpenAIClient(
                azureOpenAIEndpoint, credential, options));

        clientBuilder.UseCredential(credential);
    });

    builder.Services.AddScoped<IQueryService, AzureTableQueryService>();
    builder.Services.AddScoped<IConnectionService, AzureKeyVaultConnectionService>();
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "DBChatPro API v1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at root
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowReactApp");
app.UseAuthorization();
app.MapControllers();

app.Run();