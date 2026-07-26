using MassTransit;
using Notifications.Worker.Consumers;
using Notifications.Worker.Email;
using Serilog;
using Shared.Infrastructure.Messaging;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSerilog((services, configuration) => configuration
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console());

builder.Services
    .AddOptions<SmtpOptions>()
    .Bind(builder.Configuration.GetSection(SmtpOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services
    .AddOptions<RabbitMqOptions>()
    .Bind(builder.Configuration.GetSection(RabbitMqOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton<IEmailSender, SmtpEmailSender>();

var rabbit = builder.Configuration.GetSection(RabbitMqOptions.SectionName).Get<RabbitMqOptions>()
    ?? new RabbitMqOptions();

builder.Services.AddMassTransit(bus =>
{
    bus.SetKebabCaseEndpointNameFormatter();
    bus.AddConsumer<SendEmailNotificationConsumer>();

    bus.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbit.Host, rabbit.Port, rabbit.VirtualHost, host =>
        {
            host.Username(rabbit.Username);
            host.Password(rabbit.Password);
        });

        cfg.UseMessageRetry(retry => retry.Interval(5, TimeSpan.FromSeconds(10)));
        cfg.ConfigureEndpoints(context);
    });
});

var host = builder.Build();
await host.RunAsync();
