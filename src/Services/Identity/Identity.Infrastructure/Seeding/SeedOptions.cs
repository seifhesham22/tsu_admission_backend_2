using System.ComponentModel.DataAnnotations;

namespace Identity.Infrastructure.Seeding;

public sealed class SeedOptions
{
    public const string SectionName = "Seed";

    public bool Enabled { get; set; }

    [EmailAddress]
    public string? AdminEmail { get; set; }

    public string? AdminPassword { get; set; }
}
