using System.ComponentModel.DataAnnotations;

namespace Shared.Infrastructure.Messaging;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    [Required(AllowEmptyStrings = false)]
    public string Host { get; set; } = "localhost";

    [Range(1, 65535)]
    public ushort Port { get; set; } = 5672;

    [Required(AllowEmptyStrings = false)]
    public string VirtualHost { get; set; } = "/";

    [Required(AllowEmptyStrings = false)]
    public string Username { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string Password { get; set; } = string.Empty;
}
