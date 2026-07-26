using Admission.Application.Admissions.Services;
using Files.Application.Files.Services;
using FluentAssertions;
using Identity.Application.Users.Dtos;
using NetArchTest.Rules;
using System.Reflection;
using Xunit;

namespace Architecture.Tests;

public sealed class ServiceBoundaryTests
{
    private static readonly Assembly AdmissionDomain = typeof(global::Admission.Domain.Applicants.Applicant).Assembly;
    private static readonly Assembly AdmissionApplication = typeof(global::Admission.Application.Admissions.Services.AdmissionService).Assembly;
    private static readonly Assembly AdmissionInfrastructure = typeof(global::Admission.Infrastructure.DependencyInjection).Assembly;

    private static readonly Assembly FilesDomain = typeof(global::Files.Domain.StoredFile).Assembly;
    private static readonly Assembly FilesApplication = typeof(global::Files.Application.Files.Services.FileService).Assembly;
    private static readonly Assembly FilesInfrastructure = typeof(global::Files.Infrastructure.DependencyInjection).Assembly;

    private static readonly Assembly IdentityApplication = typeof(global::Identity.Application.Users.Dtos.RegisterRequest).Assembly;
    private static readonly Assembly IdentityInfrastructure = typeof(global::Identity.Infrastructure.DependencyInjection).Assembly;

    private static readonly Assembly Contracts = typeof(global::Contracts.IntegrationEvents.UserRegistered).Assembly;

    public static TheoryData<Assembly, string> AdmissionAssemblies => new()
    {
        { AdmissionDomain, "Admission.Domain" },
        { AdmissionApplication, "Admission.Application" },
        { AdmissionInfrastructure, "Admission.Infrastructure" }
    };

    public static TheoryData<Assembly, string> FilesAssemblies => new()
    {
        { FilesDomain, "Files.Domain" },
        { FilesApplication, "Files.Application" },
        { FilesInfrastructure, "Files.Infrastructure" }
    };

    public static TheoryData<Assembly, string> IdentityAssemblies => new()
    {
        { IdentityApplication, "Identity.Application" },
        { IdentityInfrastructure, "Identity.Infrastructure" }
    };

    public static TheoryData<Assembly, string> DomainAssemblies => new()
    {
        { AdmissionDomain, "Admission.Domain" },
        { FilesDomain, "Files.Domain" }
    };

    [Theory]
    [MemberData(nameof(AdmissionAssemblies))]
    public void AdmissionMustNotReferenceOtherServices(Assembly assembly, string name)
    {
        var forbidden = new[] { "Files.", "Identity.", "Notifications." };
        AssertNoReferenceTo(assembly, name, forbidden);
    }

    [Theory]
    [MemberData(nameof(FilesAssemblies))]
    public void FilesMustNotReferenceOtherServices(Assembly assembly, string name)
    {
        var forbidden = new[] { "Admission.", "Identity.", "Notifications." };
        AssertNoReferenceTo(assembly, name, forbidden);
    }

    [Theory]
    [MemberData(nameof(IdentityAssemblies))]
    public void IdentityMustNotReferenceOtherServices(Assembly assembly, string name)
    {
        var forbidden = new[] { "Admission.", "Files.", "Notifications." };
        AssertNoReferenceTo(assembly, name, forbidden);
    }

    [Theory]
    [MemberData(nameof(DomainAssemblies))]
    public void DomainMustNotDependOnInfrastructureLibraries(Assembly assembly, string name)
    {
        var forbidden = new[]
        {
            "Microsoft.EntityFrameworkCore",
            "MassTransit",
            "Microsoft.AspNetCore",
            "Npgsql",
            "Amazon"
        };

        var referenced = assembly
            .GetReferencedAssemblies()
            .Select(x => x.Name ?? string.Empty)
            .ToList();

        var violations = referenced
            .Where(reference => forbidden.Any(prefix =>
                reference.StartsWith(prefix, StringComparison.Ordinal)))
            .ToList();

        violations.Should().BeEmpty(
            "{0} is a domain assembly and must stay free of infrastructure dependencies", name);
    }

    [Fact]
    public void ContractsMustNotReferenceAnyProjectAssembly()
    {
        var forbidden = new[] { "Admission.", "Files.", "Identity.", "Notifications.", "Shared." };

        var referenced = Contracts
            .GetReferencedAssemblies()
            .Select(x => x.Name ?? string.Empty)
            .Where(name => forbidden.Any(prefix => name.StartsWith(prefix, StringComparison.Ordinal)))
            .ToList();

        referenced.Should().BeEmpty("Contracts must be a dependency-free integration surface");
    }

    [Fact]
    public void ApplicationLayersMustNotDependOnEntityFramework()
    {
        var assemblies = new[]
        {
            (AdmissionApplication, "Admission.Application"),
            (FilesApplication, "Files.Application")
        };

        foreach (var (assembly, name) in assemblies)
        {
            var referenced = assembly
                .GetReferencedAssemblies()
                .Select(x => x.Name ?? string.Empty)
                .Where(x => x.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal))
                .ToList();

            referenced.Should().BeEmpty(
                "{0} must depend on persistence abstractions rather than EF Core", name);
        }
    }

    [Fact]
    public void DomainEntitiesMustNotExposePublicSetters()
    {
        var result = Types.InAssembly(AdmissionDomain)
            .That()
            .ResideInNamespaceStartingWith("Admission.Domain")
            .And()
            .AreClasses()
            .And()
            .DoNotHaveNameEndingWith("Policy")
            .Should()
            .MeetCustomRule(new NoPublicSettersRule())
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "domain entities must protect their invariants. Offenders: {0}",
            string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>()));
    }

    private static void AssertNoReferenceTo(Assembly assembly, string name, string[] forbiddenPrefixes)
    {
        var violations = assembly
            .GetReferencedAssemblies()
            .Select(x => x.Name ?? string.Empty)
            .Where(reference => forbiddenPrefixes.Any(prefix =>
                reference.StartsWith(prefix, StringComparison.Ordinal)))
            .ToList();

        violations.Should().BeEmpty(
            "{0} must not reference another service's assemblies; use Contracts instead", name);
    }
}
