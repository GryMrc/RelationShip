using Microsoft.EntityFrameworkCore;
using RelationshipService.Application;
using RelationshipService.Application.ServiceContracts;
using RelationshipService.Infra;
using RelationshipService.Application.Services;
using Scalar.AspNetCore;
using StackExchange.Redis;
using MassTransit;
using RelationshipService.Application.Consumers;
using RelationshipService.Application.Events;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        // Add X-User-Id header parameter globally to all operations
        document.Components ??= new Microsoft.OpenApi.Models.OpenApiComponents();
        document.Components.Parameters ??= new Dictionary<string, Microsoft.OpenApi.Models.OpenApiParameter>();
        
        document.Components.Parameters["X-User-Id"] = new Microsoft.OpenApi.Models.OpenApiParameter
        {
            Name = "X-User-Id",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Required = true,
            Description = "User ID for authentication and authorization",
            Schema = new Microsoft.OpenApi.Models.OpenApiSchema
            {
                Type = "integer",
                Format = "int32"
            }
        };

        // Add the parameter to all operations
        foreach (var path in document.Paths.Values)
        {
            foreach (var operation in path.Operations.Values)
            {
                operation.Parameters ??= new List<Microsoft.OpenApi.Models.OpenApiParameter>();
                operation.Parameters.Add(new Microsoft.OpenApi.Models.OpenApiParameter
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.Parameter,
                        Id = "X-User-Id"
                    }
                });
            }
        }

        return Task.CompletedTask;
    });
});
builder.Services.AddControllers();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    });
}

builder.Services.AddValidatorsFromAssembly(System.Reflection.Assembly.Load("RelationshipService.Application"));

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Redis Configuration
builder.Services.AddSingleton<IConnectionMultiplexer>(sp => 
{
    var configuration = new ConfigurationOptions
    {
        EndPoints = { "localhost" },
        SyncTimeout = 500,        // 500ms timeout for sync operations
        ConnectTimeout = 2000,    // 2s timeout for connecting
        AbortOnConnectFail = false
    };
    return ConnectionMultiplexer.Connect(configuration);
});

// MassTransit & RabbitMQ Configuration
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<SwipeConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/");

        // Enable Topic Exchange for refined routing
        cfg.Publish<SwipeEvent>(p => p.ExchangeType = "topic");

        // 1. Isolated Queue for Date Mode
        cfg.ReceiveEndpoint("swipe-date-queue", e =>
        {
            e.ConfigureConsumer<SwipeConsumer>(context);
            e.Bind("RelationshipService.Application.Events:SwipeEvent", b =>
            {
                b.RoutingKey = "date";
                b.ExchangeType = "topic";
            });
        });

        // 2. Isolated Queue for BFF Mode
        cfg.ReceiveEndpoint("swipe-bff-queue", e =>
        {
            e.ConfigureConsumer<SwipeConsumer>(context);
            e.Bind("RelationshipService.Application.Events:SwipeEvent", b =>
            {
                b.RoutingKey = "bff";
                b.ExchangeType = "topic";
            });
        });

        // Retry Policy
        cfg.UseMessageRetry(r => r.Exponential(3, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(2)));
    });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<RelationShipDbContext>(options =>
    options.UseNpgsql(connectionString,
      npgsql =>
      {
          npgsql.MigrationsAssembly(typeof(RelationShipDbContext).Assembly.FullName);
          npgsql.UseNetTopologySuite();
      }
    ));

builder.Services.AddScoped<IRelationShipDbContext>(provider =>
    provider.GetRequiredService<RelationShipDbContext>());

builder.Services.AddScoped<IUserProfileService, UserProfileService>();
builder.Services.AddScoped<IHobbyService, HobbyService>();
builder.Services.AddScoped<IQuestionService, QuestionService>();
builder.Services.AddScoped<IDiscoveryTokenService, DiscoveryTokenService>();
builder.Services.AddScoped<ISwipeService, SwipeService>();
builder.Services.AddScoped<IMatchService, MatchService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.UseCors();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();