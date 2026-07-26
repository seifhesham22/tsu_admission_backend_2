using Contracts.IntegrationEvents;
using Identity.Application.Admin.Contracts;
using Identity.Application.Admin.Dtos;
using Identity.Application.Admin;
using Identity.Infrastructure.Identity.Models;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shared.Auth;
using Shared.Kernel.Exceptions;
using Shared.Kernel.Pagination;

namespace Identity.Infrastructure.Identity.Services;

public sealed class AdminUserService : IAdminUserService
{
    private static readonly string[] AssignableRoles =
    {
        Roles.RegularManager,
        Roles.HeadManager
    };

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IPublishEndpoint _publishEndpoint;

    public AdminUserService(
        UserManager<ApplicationUser> userManager,
        IPublishEndpoint publishEndpoint)
    {
        _userManager = userManager;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<StaffUserResponse> CreateAsync(
        CreateStaffUserRequest request,
        string role,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!AssignableRoles.Contains(role, StringComparer.Ordinal))
        {
            throw new DomainRuleException($"Role '{role}' cannot be assigned through this endpoint.");
        }

        if (await _userManager.FindByEmailAsync(request.Email) is not null)
        {
            throw new ConflictException($"The email '{request.Email}' is already registered.");
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            UserName = request.Email,
            TwoFactorEnabled = true
        };

        EnsureSucceeded(
            await _userManager.CreateAsync(user, request.Password),
            "Unable to create the staff account.");

        EnsureSucceeded(
            await _userManager.AddToRoleAsync(user, role),
            "Unable to assign the requested role.");

        await _publishEndpoint.Publish(
            new UserRegistered
            {
                UserId = user.Id,
                Email = request.Email,
                UserName = request.UserName,
                Role = role,
                OccurredAtUtc = DateTime.UtcNow
            },
            cancellationToken);

        await _publishEndpoint.Publish(
            new SendEmailNotification
            {
                To = request.Email,
                Subject = "Your admission staff account",
                Body = "An account has been created for you. Please sign in and change your password.",
                OccurredAtUtc = DateTime.UtcNow
            },
            cancellationToken);

        return new StaffUserResponse(user.Id, request.Email, request.UserName, role);
    }

    public async Task UpdateAsync(
        Guid userId,
        UpdateStaffUserRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new NotFoundException("The user account was not found.");

        var currentRoles = await _userManager.GetRolesAsync(user);
        if (!currentRoles.Any(role => AssignableRoles.Contains(role, StringComparer.Ordinal)))
        {
            throw new DomainRuleException("Only manager accounts can be modified through this endpoint.");
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            user.Email = request.Email;
            user.UserName = request.Email;
            EnsureSucceeded(await _userManager.UpdateAsync(user), "Unable to update the account email.");
        }

        var newRole = request.Role;
        if (!string.IsNullOrWhiteSpace(newRole))
        {
            if (!AssignableRoles.Contains(newRole, StringComparer.Ordinal))
            {
                throw new DomainRuleException($"Role '{newRole}' cannot be assigned through this endpoint.");
            }

            EnsureSucceeded(
                await _userManager.RemoveFromRolesAsync(user, currentRoles),
                "Unable to clear the existing roles.");

            EnsureSucceeded(
                await _userManager.AddToRoleAsync(user, newRole),
                "Unable to assign the requested role.");
        }

        await _publishEndpoint.Publish(
            new UserEdited
            {
                UserId = user.Id,
                Email = request.Email,
                Role = newRole,
                OccurredAtUtc = DateTime.UtcNow
            },
            cancellationToken);
    }

    public async Task DeleteAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new NotFoundException("The user account was not found.");

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Any(role => AssignableRoles.Contains(role, StringComparer.Ordinal)))
        {
            throw new DomainRuleException("Only manager accounts can be deleted through this endpoint.");
        }

        EnsureSucceeded(await _userManager.DeleteAsync(user), "Unable to delete the account.");

        await _publishEndpoint.Publish(
            new UserDeleted
            {
                UserId = userId,
                OccurredAtUtc = DateTime.UtcNow
            },
            cancellationToken);
    }

    public async Task<PagedResult<StaffUserResponse>> GetStaffAsync(
        PageRequest page,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(page);

        var managers = await _userManager.GetUsersInRoleAsync(Roles.RegularManager);
        var headManagers = await _userManager.GetUsersInRoleAsync(Roles.HeadManager);

        var headManagerIds = headManagers.Select(x => x.Id).ToHashSet();

        var all = managers
            .Concat(headManagers)
            .DistinctBy(x => x.Id)
            .OrderBy(x => x.Email)
            .ToList();

        var items = all
            .Skip(page.Skip)
            .Take(page.PageSize)
            .Select(x => new StaffUserResponse(
                x.Id,
                x.Email ?? string.Empty,
                x.UserName ?? string.Empty,
                headManagerIds.Contains(x.Id) ? Roles.HeadManager : Roles.RegularManager))
            .ToList();

        return PagedResult<StaffUserResponse>.Create(items, all.Count, page);
    }

    public IReadOnlyList<string> GetAssignableRoles() => AssignableRoles;

    private static void EnsureSucceeded(IdentityResult result, string message)
    {
        if (result.Succeeded)
        {
            return;
        }

        var details = string.Join(" ", result.Errors.Select(x => x.Description));
        throw new DomainRuleException($"{message} {details}".Trim());
    }
}
