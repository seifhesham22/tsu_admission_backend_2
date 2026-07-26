using Admission.Domain.Applicants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Admission.Infrastructure.Persistence.Configurations;

public sealed class ApplicantConfiguration : IEntityTypeConfiguration<Applicant>
{
    public void Configure(EntityTypeBuilder<Applicant> builder)
    {
        builder.ToTable("applicants");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.AuthId).IsRequired();
        builder.HasIndex(x => x.AuthId).IsUnique();

        builder.Property(x => x.FullName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Email).IsRequired().HasMaxLength(256);
        builder.Property(x => x.Citizenship).HasMaxLength(100);
        builder.Property(x => x.PhoneNumber).HasMaxLength(32);
        builder.Property(x => x.LastModifiedUtc).IsRequired();

        builder.Property(x => x.Gender).HasConversion<string>().HasMaxLength(16);

        builder.Ignore(x => x.Passport);
        builder.Ignore(x => x.EducationDocument);
        builder.Ignore(x => x.DomainEvents);

        builder.Navigation(x => x.Documents).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(x => x.Admissions).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
