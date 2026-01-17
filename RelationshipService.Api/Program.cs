using Microsoft.EntityFrameworkCore;
using RelationshipService.Application;
using RelationshipService.Application.Services.User;
using RelationshipService.Infra;
using RelationshipService.Infra.Services.User;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();

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

builder.Services.AddScoped<IUserProfileService, UserProfileService>();
builder.Services.AddScoped<IUserLocationService, UserLocationService>();
builder.Services.AddScoped<IUserPreferencesService, UserPreferencesService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();