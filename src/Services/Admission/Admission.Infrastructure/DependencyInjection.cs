using Admission.Application.Admissions.Contracts;
using Admission.Application.Admissions.Dtos;
using Admission.Application.Admissions.Services;
using Admission.Application.Applicants.Contracts;
using Admission.Application.Applicants.Dtos;
using Admission.Application.Applicants.Services;
using Admission.Application.Catalogue.Contracts;
using Admission.Application.Catalogue.Dtos;
using Admission.Application.Catalogue.Services;
using Admission.Application.Consumers;
using Admission.Application.Managers.Contracts;
using Admission.Application.Managers.Dtos;
using Admission.Application.Managers.Services;
using Admission.Application.Messaging.Contracts;
using Admission.Application.Options;
using Admission.Application.Persistence.Contracts;
using Admission.Application.Sync.Contracts;
using Admission.Application.Sync.Dtos;
using Admission.Infrastructure.Messaging;
using Admission.Infrastructure.Persistence.Queries;
using Admission.Infrastructure.Persistence.Repositories;
using Admission.Infrastructure.Persistence;
using Admission.Infrastructure.Sync.Jobs;
using Admission.Infrastructure.Sync.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Shared.Infrastructure.Messaging;
using System.Net.Http.Headers;
using System.Text;

namespace Admission.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAdmissionInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<AdmissionOptions>()
            .Bind(configuration.GetSection(AdmissionOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddOptions<OneCOptions>()
            .Bind(configuration.GetSection(OneCOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddOptions<CatalogueSyncJobOptions>()
            .Bind(configuration.GetSection(CatalogueSyncJobOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddOptions<RabbitMqOptions>()
            .Bind(configuration.GetSection(RabbitMqOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddDbContext<AdmissionDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("AdmissionDatabase"),
                npgsql => npgsql.EnableRetryOnFailure()));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IApplicantRepository, ApplicantRepository>();
        services.AddScoped<IAdmissionRepository, AdmissionRepository>();
        services.AddScoped<IManagerRepository, ManagerRepository>();
        services.AddScoped<ICatalogueRepository, CatalogueRepository>();

        services.AddScoped<ICatalogueQueries, CatalogueQueries>();
        services.AddScoped<IAdmissionQueries, AdmissionQueries>();
        services.AddScoped<IManagerQueries, ManagerQueries>();

        services.AddScoped<IAdmissionAccessGuard, AdmissionAccessGuard>();
        services.AddScoped<IAdmissionEventPublisher, AdmissionEventPublisher>();

        services.AddScoped<IApplicantProfileService, ApplicantProfileService>();
        services.AddScoped<IApplicantDocumentService, ApplicantDocumentService>();
        services.AddScoped<IAdmissionService, AdmissionService>();
        services.AddScoped<IManagerWorkloadService, ManagerWorkloadService>();
        services.AddScoped<IHeadManagerService, HeadManagerService>();
        services.AddScoped<ICatalogueService, CatalogueService>();
        services.AddScoped<ICatalogueSyncService, CatalogueSyncService>();

        services.AddOneCClient(configuration);
        services.AddAdmissionMessaging(configuration);
        services.AddCatalogueSyncJob(configuration);

        return services;
    }

    private static void AddOneCClient(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection(OneCOptions.SectionName).Get<OneCOptions>()
            ?? new OneCOptions();

        services.AddHttpClient<IOneCClient, OneCClient>(client =>
        {
            if (!string.IsNullOrWhiteSpace(options.BaseUrl))
            {
                client.BaseAddress = new Uri(
                    options.BaseUrl.EndsWith('/') ? options.BaseUrl : options.BaseUrl + "/");
            }

            client.Timeout = TimeSpan.FromSeconds(60);

            var credentials = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{options.Username}:{options.Password}"));

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", credentials);
        });
    }

    private static void AddAdmissionMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        var rabbit = configuration.GetSection(RabbitMqOptions.SectionName).Get<RabbitMqOptions>()
            ?? new RabbitMqOptions();

        services.AddMassTransit(bus =>
        {
            bus.SetKebabCaseEndpointNameFormatter();

            bus.AddConsumer<UserRegisteredConsumer>();
            bus.AddConsumer<UserEditedConsumer>();
            bus.AddConsumer<UserDeletedConsumer>();

            bus.AddEntityFrameworkOutbox<AdmissionDbContext>(outbox =>
            {
                outbox.UsePostgres();
                outbox.UseBusOutbox();
                outbox.QueryDelay = TimeSpan.FromSeconds(1);
            });

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

    private static void AddCatalogueSyncJob(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection(CatalogueSyncJobOptions.SectionName)
            .Get<CatalogueSyncJobOptions>() ?? new CatalogueSyncJobOptions();

        if (!options.Enabled)
        {
            return;
        }

        services.AddQuartz(quartz =>
        {
            var jobKey = JobKey.Create(nameof(CatalogueSyncJob));

            quartz
                .AddJob<CatalogueSyncJob>(jobKey, job => job.WithIdentity(jobKey))
                .AddTrigger(trigger => trigger
                    .ForJob(jobKey)
                    .StartAt(DateTimeOffset.UtcNow.AddMinutes(options.StartAfterMinutes))
                    .WithSimpleSchedule(schedule => schedule
                        .WithInterval(TimeSpan.FromDays(options.IntervalDays))
                        .RepeatForever()));
        });

        services.AddQuartzHostedService(quartz => quartz.WaitForJobsToComplete = true);
    }
}
