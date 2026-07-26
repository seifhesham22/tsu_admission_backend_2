using Admission.Domain.Catalogue;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Admission.Infrastructure.Persistence.Configurations;

public sealed class EducationProgramConfiguration : IEntityTypeConfiguration<EducationProgram>
{
    public void Configure(EntityTypeBuilder<EducationProgram> builder)
    {
        builder.ToTable("education_programs");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(500);
        builder.Property(x => x.Code).HasMaxLength(100);
        builder.Property(x => x.Language).HasMaxLength(100);
        builder.Property(x => x.EducationForm).HasMaxLength(100);

        builder.HasIndex(x => x.OneCId).IsUnique();
        builder.HasIndex(x => x.FacultyId);
        builder.HasIndex(x => x.EducationLevelId);

        builder
            .HasOne(x => x.Faculty)
            .WithMany(x => x.Programs)
            .HasForeignKey(x => x.FacultyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.EducationLevel)
            .WithMany()
            .HasForeignKey(x => x.EducationLevelId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
