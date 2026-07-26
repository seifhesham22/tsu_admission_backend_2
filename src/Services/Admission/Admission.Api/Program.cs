using System.Text.Json.Serialization;
using Admission.Infrastructure;
using Admission.Infrastructure.Persistence;
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

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddProblemDetails();
builder.Services.AddSwaggerWithBearer("Admission API");
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAdmissionInfrastructure(builder.Configuration);
builder.Services.AddServiceHealthChecks<AdmissionDbContext>();

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

if (app.Configuration.GetValue<bool>("Database:MigrateOnStartup"))
{
    await using var scope = app.Services.CreateAsyncScope();
    var context = scope.ServiceProvider.GetRequiredService<AdmissionDbContext>();
    await context.Database.MigrateAsync();
}

await app.RunAsync();

public partial class Program;
