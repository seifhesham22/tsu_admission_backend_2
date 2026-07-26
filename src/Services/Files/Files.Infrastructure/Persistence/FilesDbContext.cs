using Files.Domain;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Files.Infrastructure.Persistence;

public sealed class FilesDbContext : DbContext
{
    public DbSet<StoredFile> Files => Set<StoredFile>();

    public DbSet<AdmissionAccess> AdmissionAccess => Set<AdmissionAccess>();

    public FilesDbContext(DbContextOptions<FilesDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StoredFile>(entity =>
        {
            entity.ToTable("stored_files");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.OriginalFileName).IsRequired().HasMaxLength(400);
            entity.Property(x => x.StorageKey).IsRequired().HasMaxLength(400);
            entity.Property(x => x.OwnerRole).IsRequired().HasMaxLength(32);

            entity.Property(x => x.Kind)
                .HasConversion<string>()
                .HasMaxLength(32)
                .IsRequired();

            entity.HasIndex(x => x.ApplicantId);
            entity.HasIndex(x => x.StorageKey).IsUnique();
        });

        modelBuilder.Entity<AdmissionAccess>(entity =>
        {
            entity.ToTable("admission_access");
            entity.HasKey(x => x.ApplicantId);

            entity.Property(x => x.ApplicantId).ValueGeneratedNever();
            entity.Property(x => x.ApplicantAuthId).IsRequired();
            entity.Property(x => x.UpdatedAtUtc).IsRequired();

            entity.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(16)
                .IsRequired();

            entity.Ignore(x => x.IsOpen);

            entity.HasIndex(x => x.ApplicantAuthId);
            entity.HasIndex(x => x.AssignedManagerAuthId);
        });

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();

        base.OnModelCreating(modelBuilder);
    }
}
