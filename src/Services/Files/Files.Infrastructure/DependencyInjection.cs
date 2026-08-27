using Amazon.S3;
using Files.Application.Consumers;
using Files.Application.Files.Contracts;
using Files.Application.Files.Services;
using Files.Application.Files;
using Files.Application.Persistence.Contracts;
using Files.Application.Storage.Contracts;
using Files.Infrastructure.Persistence.Repositories;
using Files.Infrastructure.Persistence;
using Files.Infrastructure.Storage;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.Messaging;

namespace Files.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddFilesInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<S3Options>()
            .Bind(configuration.GetSection(S3Options.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddOptions<RabbitMqOptions>()
            .Bind(configuration.GetSection(RabbitMqOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddDbContext<FilesDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("FilesDatabase"),
                npgsql => npgsql.EnableRetryOnFailure()));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IStoredFileRepository, StoredFileRepository>();
        services.AddScoped<IAdmissionAccessRepository, AdmissionAccessRepository>();
        services.AddScoped<IFileStorage, S3FileStorage>();
        services.AddScoped<IFileService, FileService>();

        services.AddSingleton<IAmazonS3>(provider =>
        {
            var options = configuration.GetSection(S3Options.SectionName).Get<S3Options>()
                ?? throw new InvalidOperationException("The S3 configuration section is missing.");

            var config = new AmazonS3Config
            {
                ServiceURL = options.ServiceUrl,
                ForcePathStyle = true,
                AuthenticationRegion = options.Region
            };

            return new AmazonS3Client(options.AccessKey, options.SecretKey, config);
        });

        services.AddFilesMessaging(configuration);

        return services;
    }

    private static void AddFilesMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        var rabbit = configuration.GetSection(RabbitMqOptions.SectionName).Get<RabbitMqOptions>()
            ?? new RabbitMqOptions();

        services.AddMassTransit(bus =>
        {
            bus.SetKebabCaseEndpointNameFormatter();

            bus.AddConsumer<AdmissionAccessChangedConsumer>();

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
