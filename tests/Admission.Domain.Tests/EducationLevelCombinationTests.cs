using Admission.Domain.Catalogue;
using FluentAssertions;
using Xunit;

namespace Admission.Domain.Tests;

public sealed class EducationLevelCombinationTests
{
    [Fact]
    public void Matches_is_direction_independent()
    {
        var levelA = Guid.NewGuid();
        var levelB = Guid.NewGuid();

        var combination = EducationLevelCombination.Create(levelA, levelB, isAllowed: true);

        combination.Matches(levelA, levelB).Should().BeTrue();
        combination.Matches(levelB, levelA).Should().BeTrue();
    }

    [Fact]
    public void Matches_returns_false_for_an_unrelated_pair()
    {
        var combination = EducationLevelCombination.Create(Guid.NewGuid(), Guid.NewGuid(), isAllowed: true);

        combination.Matches(Guid.NewGuid(), Guid.NewGuid()).Should().BeFalse();
    }
}
