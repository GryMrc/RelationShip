using Microsoft.EntityFrameworkCore;
using RelationshipService.Application;
using RelationshipService.Infra;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<RelationShipDbContext>(options =>
    options.UseNpgsql(connectionString,
      npgsql =>
      {
          npgsql.MigrationsAssembly(typeof(RelationShipDbContext).Assembly.FullName);
      }
    ));

builder.Services.AddScoped<IRelationShipDbContext>(provider =>
    provider.GetRequiredService<RelationShipDbContext>());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();