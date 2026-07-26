using Contracts.IntegrationEvents;
using MassTransit;
using Microsoft.Extensions.Logging;
using Notifications.Worker.Email;

namespace Notifications.Worker.Consumers;

public sealed class SendEmailNotificationConsumer : IConsumer<SendEmailNotification>
{
    private readonly IEmailSender _emailSender;
    private readonly ILogger<SendEmailNotificationConsumer> _logger;

    public SendEmailNotificationConsumer(
        IEmailSender emailSender,
        ILogger<SendEmailNotificationConsumer> logger)
    {
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SendEmailNotification> context)
    {
        var message = context.Message;

        await _emailSender.SendAsync(
            message.To,
            message.Subject,
            message.Body,
            context.CancellationToken);

        _logger.LogInformation(
            "Sent notification email to {Recipient} with subject {Subject}.",
            message.To,
            message.Subject);
    }
}
