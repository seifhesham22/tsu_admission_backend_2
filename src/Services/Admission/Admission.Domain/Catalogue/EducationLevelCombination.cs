using Admission.Domain.Abstractions;

namespace Admission.Domain.Catalogue;

public sealed class EducationLevelCombination : Entity
{
    public Guid FirstLevelId { get; private set; }

    public Guid SecondLevelId { get; private set; }

    public bool IsAllowed { get; private set; }

    private EducationLevelCombination()
    {
    }

    private EducationLevelCombination(Guid firstLevelId, Guid secondLevelId, bool isAllowed)
    {
        FirstLevelId = firstLevelId;
        SecondLevelId = secondLevelId;
        IsAllowed = isAllowed;
    }

    public static EducationLevelCombination Create(Guid firstLevelId, Guid secondLevelId, bool isAllowed)
    {
        var (low, high) = Normalize(firstLevelId, secondLevelId);
        return new EducationLevelCombination(low, high, isAllowed);
    }

    public static (Guid First, Guid Second) Normalize(Guid a, Guid b) =>
        a.CompareTo(b) <= 0 ? (a, b) : (b, a);

    public bool Matches(Guid a, Guid b)
    {
        var (low, high) = Normalize(a, b);
        return FirstLevelId == low && SecondLevelId == high;
    }
}
