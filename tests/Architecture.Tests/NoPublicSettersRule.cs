using Mono.Cecil;
using NetArchTest.Rules;

namespace Architecture.Tests;

public sealed class NoPublicSettersRule : ICustomRule
{
    public bool MeetsRule(TypeDefinition type)
    {
        ArgumentNullException.ThrowIfNull(type);

        return !type.Properties.Any(property => property.SetMethod is { IsPublic: true });
    }
}
