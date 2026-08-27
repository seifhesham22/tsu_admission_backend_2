using Microsoft.AspNetCore.Identity;

namespace Identity.Api.Persistence.Models;

public sealed class ApplicationRole : IdentityRole<Guid>
{
    public ApplicationRole()
    {
    }

    public ApplicationRole(string name) : base(name)
    {
    }
}
