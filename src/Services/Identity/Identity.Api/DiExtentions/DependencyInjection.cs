using Identity.Api.Admin;
using Identity.Api.Persistence.Models;
using Identity.Api.Persistence;
using Identity.Api.Persistence.Seeding;
using Identity.Api.Token;
using Identity.Api.Users;
using MassTransit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Messaging;

namespace Identity.Api.DiExtentions;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<SeedOptions>()
            .Bind(configuration.GetSection(SeedOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddOptions<RabbitMqOptions>()
            .Bind(configuration.GetSection(RabbitMqOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddDbContext<AppIdentityDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("IdentityDatabase"),
                npgsql => npgsql.EnableRetryOnFailure()));

        services
            .AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.SignIn.RequireConfirmedEmail = false;
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddEntityFrameworkStores<AppIdentityDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAdminUserService, AdminUserService>();
        services.AddScoped<IdentitySeeder>();

        services.AddIdentityMessaging(configuration);

        return services;
    }

    public static AuthenticationBuilder AddTempTokenScheme(this AuthenticationBuilder builder) =>
        builder.AddScheme<AuthenticationSchemeOptions, TempTokenAuthenticationHandler>(
            TempTokenAuthenticationHandler.SchemeName,
            _ => { });

    private static void AddIdentityMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        var rabbit = configuration.GetSection(RabbitMqOptions.SectionName).Get<RabbitMqOptions>()
            ?? new RabbitMqOptions();

        services.AddMassTransit(bus =>
        {
            bus.SetKebabCaseEndpointNameFormatter();

            bus.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(rabbit.Host, rabbit.Port, rabbit.VirtualHost, host =>
                {
                    host.Username(rabbit.Username);
                    host.Password(rabbit.Password);
                });

                cfg.UseMessageRetry(retry => retry.Interval(3, TimeSpan.FromSeconds(5)));
                cfg.ConfigureEndpoints(context);
            });
        });
    }
}
