using System.ComponentModel.DataAnnotations;

namespace Admission.Application.Options;

public sealed class CatalogueSyncJobOptions
{
    public const string SectionName = "CatalogueSyncJob";

    public bool Enabled { get; set; } = true;

    [Range(0, 1440)]
    public int StartAfterMinutes { get; set; } = 1;

    [Range(1, 365)]
    public int IntervalDays { get; set; } = 7;
}
