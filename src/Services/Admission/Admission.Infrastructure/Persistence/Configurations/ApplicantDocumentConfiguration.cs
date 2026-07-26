using Admission.Domain.Applicants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Admission.Infrastructure.Persistence.Configurations;

public sealed class ApplicantDocumentConfiguration : IEntityTypeConfiguration<ApplicantDocument>
{
    public void Configure(EntityTypeBuilder<ApplicantDocument> builder)
    {
        builder.ToTable("applicant_documents");
        builder.HasKey(x => x.Id);

        builder.HasDiscriminator<string>("document_kind")
            .HasValue<Passport>("passport")
            .HasValue<EducationDocument>("education");

        builder.Property(x => x.ApplicantId).IsRequired();
        builder.HasIndex(x => x.ApplicantId);

        builder.Property(x => x.LastModifiedUtc).IsRequired();

        builder
            .HasOne(x => x.Applicant)
            .WithMany(x => x.Documents)
            .HasForeignKey(x => x.ApplicantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
