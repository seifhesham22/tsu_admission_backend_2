using Identity.Infrastructure.Identity.Authentication;
using Identity.Infrastructure.Identity.Contracts;
using Identity.Infrastructure.Identity.Models;
using Identity.Infrastructure.Identity.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Auth;
using System.ComponentModel.DataAnnotations;

namespace Identity.Infrastructure.Seeding;

public sealed class IdentitySeeder
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly SeedOptions _options;
    private readonly ILogger<IdentitySeeder> _logger;

    public IdentitySeeder(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IOptions<SeedOptions> options,
        ILogger<IdentitySeeder> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _options = options.Value;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        foreach (var role in Roles.All)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new ApplicationRole(role));
            }
        }

        if (!_options.Enabled)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(_options.AdminEmail) ||
            string.IsNullOrWhiteSpace(_options.AdminPassword))
        {
            _logger.LogWarning(
                "Seeding is enabled but Seed:AdminEmail or Seed:AdminPassword is not configured; skipping admin creation.");
            return;
        }

        if (await _userManager.FindByEmailAsync(_options.AdminEmail) is not null)
        {
            return;
        }

        var admin = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = _options.AdminEmail,
            UserName = _options.AdminEmail,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(admin, _options.AdminPassword);
        if (!result.Succeeded)
        {
            _logger.LogError(
                "Failed to seed the admin account: {Errors}",
                string.Join(", ", result.Errors.Select(x => x.Description)));
            return;
        }

        await _userManager.AddToRoleAsync(admin, Roles.Admin);
        _logger.LogInformation("Seeded the initial admin account.");
    }
}
