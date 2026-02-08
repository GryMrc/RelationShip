using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using RelationshipWebSocketServer.Hubs;
using RelationshipWebSocketServer.Services;
using RelationshipWebSocketServer.Consumers;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// 1. Add SignalR
builder.Services.AddSignalR();

// 2. Add HttpClient for RelationshipService.Api
builder.Services.AddHttpClient("RelationshipApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["RelationshipApi:BaseUrl"] ?? "http://localhost:5181");
});

// 3. Add Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "MatcherService",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "MatcherService",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"] ?? "your-very-secure-secret-key-change-me-later"))
        };

        // For SignalR, the token is passed in the query string
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chat"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// 4. Add MassTransit (RabbitMQ)
builder.Services.AddSingleton<IClientConnectionManager, ClientConnectionManager>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<MatchEventConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMq:Host"] ?? "localhost", "/");
        
        cfg.ReceiveEndpoint("relationship-ws-queue", e =>
        {
            e.ConfigureConsumer<MatchEventConsumer>(context);
        });
    });
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<ChatHub>("/chat");

app.MapGet("/", () => "Relationship WebSocket Server Running");

app.Run();
