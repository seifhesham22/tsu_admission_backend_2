using Identity.Infrastructure;
using Identity.Infrastructure.Persistence;
using Identity.Infrastructure.Seeding;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Shared.Auth;
using Shared.Infrastructure.Health;
using Shared.Infrastructure.Middleware;
using Shared.Infrastructure.Swagger;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console());

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddSwaggerWithBearer("Identity API");

builder.Services.AddIdentityInfrastructure(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration).AddTempTokenScheme();
builder.Services.AddServiceHealthChecks<AppIdentityDbContext>();

var corsOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
    {
        if (corsOrigins.Length == 0)
        {
            policy.SetIsOriginAllowed(_ => true);
        }
        else
        {
            policy.WithOrigins(corsOrigins);
        }

        policy.AllowAnyHeader().AllowAnyMethod();
    }));

var app = builder.Build();

app.UseSharedExceptionHandling();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapServiceHealthChecks();

await using (var scope = app.Services.CreateAsyncScope())
{
    if (app.Configuration.GetValue<bool>("Database:MigrateOnStartup"))
    {
        var context = scope.ServiceProvider.GetRequiredService<AppIdentityDbContext>();
        await context.Database.MigrateAsync();
    }

    var seeder = scope.ServiceProvider.GetRequiredService<IdentitySeeder>();
    await seeder.SeedAsync();
}

await app.RunAsync();

public partial class Program;
