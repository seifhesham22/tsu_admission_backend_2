using Admission.Domain.Admissions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Admission.Infrastructure.Persistence.Configurations;

public sealed class AdmissionProgramConfiguration : IEntityTypeConfiguration<AdmissionProgram>
{
    public void Configure(EntityTypeBuilder<AdmissionProgram> builder)
    {
        builder.ToTable("admission_programs");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Priority)
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();

        builder
            .HasOne<ApplicantAdmission>()
            .WithMany(x => x.Programs)
            .HasForeignKey(x => x.ApplicantAdmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.EducationProgram)
            .WithMany()
            .HasForeignKey(x => x.EducationProgramId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasIndex(x => new { x.ApplicantAdmissionId, x.EducationProgramId })
            .IsUnique();
    }
}
