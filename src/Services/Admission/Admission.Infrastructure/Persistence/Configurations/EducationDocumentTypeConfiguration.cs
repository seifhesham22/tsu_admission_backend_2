using Admission.Domain.Catalogue;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Admission.Infrastructure.Persistence.Configurations;

public sealed class EducationDocumentTypeConfiguration : IEntityTypeConfiguration<EducationDocumentType>
{
    public void Configure(EntityTypeBuilder<EducationDocumentType> builder)
    {
        builder.ToTable("education_document_types");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(300);
        builder.HasIndex(x => x.OneCId).IsUnique();

        builder
            .HasOne(x => x.CurrentEducationLevel)
            .WithMany()
            .HasForeignKey(x => x.CurrentEducationLevelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasMany(x => x.NextEducationLevels)
            .WithMany(x => x.ApplicableDocumentTypes)
            .UsingEntity(join => join.ToTable("education_document_type_next_levels"));

        builder.Navigation(x => x.NextEducationLevels)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
