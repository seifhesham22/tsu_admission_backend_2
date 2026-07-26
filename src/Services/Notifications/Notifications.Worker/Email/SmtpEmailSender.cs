using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace Notifications.Worker.Email;

public sealed class SmtpEmailSender : IEmailSender, IDisposable
{
    private readonly SmtpOptions _options;
    private readonly SmtpClient _client;

    public SmtpEmailSender(IOptions<SmtpOptions> options)
    {
        _options = options.Value;
        _client = new SmtpClient(_options.Host, _options.Port)
        {
            EnableSsl = _options.EnableSsl,
            Credentials = new NetworkCredential(_options.Username, _options.Password)
        };
    }

    public async Task SendAsync(
        string to,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        using var message = new MailMessage
        {
            From = new MailAddress(_options.FromEmail, _options.FromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = false
        };

        message.To.Add(to);

        await _client.SendMailAsync(message, cancellationToken);
    }

    public void Dispose() => _client.Dispose();
}
