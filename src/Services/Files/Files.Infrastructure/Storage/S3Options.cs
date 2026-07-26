using System.ComponentModel.DataAnnotations;

namespace Files.Infrastructure.Storage;

public sealed class S3Options
{
    public const string SectionName = "S3";

    [Required(AllowEmptyStrings = false)]
    public string ServiceUrl { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string Region { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string AccessKey { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string SecretKey { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string BucketName { get; set; } = string.Empty;
}
