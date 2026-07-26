using Admission.Domain.Admissions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Admission.Infrastructure.Persistence.Configurations;

public sealed class ApplicantAdmissionConfiguration : IEntityTypeConfiguration<ApplicantAdmission>
{
    public void Configure(EntityTypeBuilder<ApplicantAdmission> builder)
    {
        builder.ToTable("applicant_admissions");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ApplicantId).IsRequired();
        builder.HasIndex(x => x.ApplicantId).IsUnique();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.LastModifiedUtc).IsRequired();
        builder.Property(x => x.Version).IsRowVersion();

        builder.HasIndex(x => x.ManagerId);
        builder.HasIndex(x => x.Status);

        builder.Ignore(x => x.DomainEvents);
        builder.Ignore(x => x.IsClosed);

        builder
            .HasOne(x => x.Applicant)
            .WithMany(x => x.Admissions)
            .HasForeignKey(x => x.ApplicantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.Manager)
            .WithMany()
            .HasForeignKey(x => x.ManagerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Navigation(x => x.Programs).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
