using Admission.Domain.Catalogue;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Admission.Infrastructure.Persistence.Configurations;

public sealed class EducationLevelCombinationConfiguration : IEntityTypeConfiguration<EducationLevelCombination>
{
    public void Configure(EntityTypeBuilder<EducationLevelCombination> builder)
    {
        builder.ToTable("education_level_combinations");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.FirstLevelId, x.SecondLevelId }).IsUnique();
    }
}
