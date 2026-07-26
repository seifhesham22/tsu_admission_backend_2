using System.ComponentModel.DataAnnotations;

namespace Notifications.Worker.Email;

public sealed class SmtpOptions
{
    public const string SectionName = "Smtp";

    [Required(AllowEmptyStrings = false)]
    public string Host { get; set; } = string.Empty;

    [Range(1, 65535)]
    public int Port { get; set; } = 587;

    [Required(AllowEmptyStrings = false)]
    public string Username { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string Password { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    [EmailAddress]
    public string FromEmail { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string FromName { get; set; } = string.Empty;

    public bool EnableSsl { get; set; } = true;
}
