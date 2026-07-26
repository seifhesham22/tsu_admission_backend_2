using Admission.Domain.Managers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Admission.Infrastructure.Persistence.Configurations;

public sealed class ManagerConfiguration : IEntityTypeConfiguration<Manager>
{
    public void Configure(EntityTypeBuilder<Manager> builder)
    {
        builder.ToTable("managers");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.AuthId).IsRequired();
        builder.HasIndex(x => x.AuthId).IsUnique();

        builder.Property(x => x.FullName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Email).IsRequired().HasMaxLength(256);

        builder.Property(x => x.Role)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Ignore(x => x.DomainEvents);
        builder.Ignore(x => x.CanOwnAdmissions);
        builder.Ignore(x => x.IsHeadManager);

        builder
            .HasOne(x => x.Faculty)
            .WithMany()
            .HasForeignKey(x => x.FacultyId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
