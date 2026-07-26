using Admission.Domain.Applicants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Admission.Infrastructure.Persistence.Configurations;

public sealed class EducationDocumentConfiguration : IEntityTypeConfiguration<EducationDocument>
{
    public void Configure(EntityTypeBuilder<EducationDocument> builder)
    {
        builder
            .HasOne(x => x.DocumentType)
            .WithMany()
            .HasForeignKey(x => x.DocumentTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
