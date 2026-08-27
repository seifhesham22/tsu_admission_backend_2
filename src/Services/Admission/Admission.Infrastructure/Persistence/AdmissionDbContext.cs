using Admission.Domain.Admissions;
using Admission.Domain.Applicants;
using Admission.Domain.Catalogue;
using Admission.Domain.Managers;
using Microsoft.EntityFrameworkCore;

namespace Admission.Infrastructure.Persistence;

public sealed class AdmissionDbContext : DbContext
{
    public DbSet<Applicant> Applicants => Set<Applicant>();

    public DbSet<ApplicantDocument> Documents => Set<ApplicantDocument>();

    public DbSet<ApplicantAdmission> Admissions => Set<ApplicantAdmission>();

    public DbSet<AdmissionProgram> AdmissionPrograms => Set<AdmissionProgram>();

    public DbSet<Manager> Managers => Set<Manager>();

    public DbSet<Faculty> Faculties => Set<Faculty>();

    public DbSet<EducationLevel> EducationLevels => Set<EducationLevel>();

    public DbSet<EducationProgram> EducationPrograms => Set<EducationProgram>();

    public DbSet<EducationDocumentType> EducationDocumentTypes => Set<EducationDocumentType>();

    public DbSet<EducationLevelCombination> EducationLevelCombinations => Set<EducationLevelCombination>();

    public AdmissionDbContext(DbContextOptions<AdmissionDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AdmissionDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
