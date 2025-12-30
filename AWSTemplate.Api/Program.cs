using AWSTemplate.Api.Controllers;
using AWSTemplate.Application.Abstractions.Caching;
using AWSTemplate.Application.Abstractions.Persistence;
using AWSTemplate.Application.Items.Commands;
using AWSTemplate.Application.Items.Queries;
using AWSTemplate.Infrastructure.Caching;
using AWSTemplate.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------
// Setup database and Redis connection
// ---------------------------
string postgresConn;
string redisConn;

if (builder.Environment.IsDevelopment())
{
    // Local connections
    postgresConn = builder.Configuration.GetConnectionString("Postgres")!;
    redisConn = builder.Configuration.GetConnectionString("Redis")!;
}
else
{
    // Production: use AWS Secrets Manager
    var secretName = builder.Configuration["SecretsManager:SecretName"]!;
    var region = builder.Configuration["SecretsManager:Region"]!;

    var secrets = await GetSecretsAsync(secretName, region);

    postgresConn = $"Host={secrets["PostgresHost"]};Port={secrets["PostgresPort"]};Database={secrets["PostgresDatabase"]};Username={secrets["PostgresUsername"]};Password={secrets["PostgresPassword"]};SSL Mode=Require;Trust Server Certificate=true";
    redisConn = $"{secrets["RedisEndpoint"]}:{secrets["RedisPort"]}";
}

// ---------------------------
// Add Services
// ---------------------------

// DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(postgresConn));

// Redis
var redisOptions = ConfigurationOptions.Parse(redisConn);
if (!builder.Environment.IsDevelopment())
{
    redisOptions.Ssl = true;
    redisOptions.AbortOnConnectFail = false;
    redisOptions.ConnectTimeout = 5000;
}
var redis = ConnectionMultiplexer.Connect(redisOptions);
builder.Services.AddSingleton<IConnectionMultiplexer>(redis);
builder.Services.AddScoped<ICacheService, RedisCacheService>();

// Repositories
builder.Services.AddScoped<IItemRepository, ItemRepository>();

// MediatR
builder.Services.AddMediatR(typeof(CreateItemCommand).Assembly);

// Behaviors
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CacheInvalidationBehavior<,>));

// Controllers
builder.Services.AddControllers();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ---------------------------
// Configure Middleware
// ---------------------------
// if (app.Environment.IsDevelopment())
// {
app.UseSwagger();
app.UseSwaggerUI();
// }

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();

// ---------------------------
// Method to retrieve secrets from AWS Secrets Manager
// ---------------------------
static async Task<Dictionary<string, string>> GetSecretsAsync(string secretName, string region)
{
    using IAmazonSecretsManager client = new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(region));

    var request = new GetSecretValueRequest
    {
        SecretId = secretName,
        VersionStage = "AWSCURRENT",
    };

    GetSecretValueResponse response;
    try
    {
        response = await client.GetSecretValueAsync(request);
    }
    catch (Exception e)
    {
        throw new InvalidOperationException("Cannot fetch secret from AWS Secrets Manager", e);
    }

    var secretString = response.SecretString!;
    return JsonSerializer.Deserialize<Dictionary<string, string>>(secretString)
           ?? throw new InvalidOperationException("Secret string is empty or invalid JSON");
}
