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
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using RelationshipService.Api.Filters;
using RelationshipService.Api.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Components ??= new Microsoft.OpenApi.Models.OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, Microsoft.OpenApi.Models.OpenApiSecurityScheme>();

        document.Components.SecuritySchemes["Bearer"] = new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Enter your JWT token here."
        };

        document.SecurityRequirements ??= new List<Microsoft.OpenApi.Models.OpenApiSecurityRequirement>();
        document.SecurityRequirements.Add(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            [new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            }] = Array.Empty<string>()
        });

        return Task.CompletedTask;
    });
});

// JWT Authentication Configuration
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"] ?? "DefaultVeryLongSecretKeyForDevelopmentOnly123!"))
    };
});

builder.Services.AddLocalization();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ResultLocalizationFilter>();
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
});

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
    x.AddConsumer<SaveMessageConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/");

        // Enable Topic Exchange for refined routing
        cfg.Publish<SwipeEvent>(p => p.ExchangeType = "topic");

        cfg.ReceiveEndpoint("save-message-queue", e =>
        {
            e.ConfigureConsumer<SaveMessageConsumer>(context);
        });

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

builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IHobbyService, HobbyService>();
builder.Services.AddScoped<IQuestionService, QuestionService>();
builder.Services.AddScoped<IDiscoveryTokenService, DiscoveryTokenService>();
builder.Services.AddScoped<ISwipeService, SwipeService>();
builder.Services.AddScoped<IMatchService, MatchService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IPushNotificationService, FcmService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.UseCors();
}

var supportedCultures = new[] { "en-US", "tr-TR" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[1]) // tr-TR
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

app.UseRequestLocalization(localizationOptions);

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();