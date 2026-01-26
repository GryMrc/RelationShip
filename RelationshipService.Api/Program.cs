using Microsoft.EntityFrameworkCore;
using RelationshipService.Application;
using RelationshipService.Application.ServiceContracts;
using RelationshipService.Infra;
using RelationshipService.Application.Services;
using Scalar.AspNetCore;

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