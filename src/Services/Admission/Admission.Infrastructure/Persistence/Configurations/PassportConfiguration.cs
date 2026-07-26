using Admission.Domain.Applicants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Admission.Infrastructure.Persistence.Configurations;

public sealed class PassportConfiguration : IEntityTypeConfiguration<Passport>
{
    public void Configure(EntityTypeBuilder<Passport> builder)
    {
        builder.Property(x => x.Series).HasMaxLength(32);
        builder.Property(x => x.PlaceOfBirth).HasMaxLength(200);
        builder.Property(x => x.IssuedBy).HasMaxLength(200);
    }
}
