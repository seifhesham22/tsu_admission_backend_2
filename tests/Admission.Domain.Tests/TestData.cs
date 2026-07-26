using Admission.Domain.Catalogue;
using Admission.Domain.Managers;

namespace Admission.Domain.Tests;

internal static class TestData
{
    public static EducationProgram Program(Guid? levelId = null) =>
        EducationProgram.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            levelId ?? Guid.NewGuid(),
            "Software Engineering",
            "09.03.04",
            "English",
            "Full-time");

    public static Manager RegularManager() =>
        Manager.Create(
            Guid.NewGuid(),
            $"{Guid.NewGuid():N}@example.com",
            "Regular Manager",
            ManagerRole.RegularManager);

    public static Manager HeadManager() =>
        Manager.Create(
            Guid.NewGuid(),
            $"{Guid.NewGuid():N}@example.com",
            "Head Manager",
            ManagerRole.HeadManager);
}
