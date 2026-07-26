using Admission.Application.Admissions.Contracts;
using Admission.Application.Messaging.Contracts;
using Admission.Application.Persistence.Contracts;
using Admission.Domain.Managers;
using Contracts.IntegrationEvents;
using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.Auth;

namespace Admission.Application.Consumers;

public sealed class UserEditedConsumer : IConsumer<UserEdited>
{
    private readonly IManagerRepository _managers;
    private readonly IApplicantRepository _applicants;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserEditedConsumer> _logger;

    public UserEditedConsumer(
        IManagerRepository managers,
        IApplicantRepository applicants,
        IUnitOfWork unitOfWork,
        ILogger<UserEditedConsumer> logger)
    {
        _managers = managers;
        _applicants = applicants;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserEdited> context)
    {
        var message = context.Message;
        var cancellationToken = context.CancellationToken;

        var manager = await _managers.GetByAuthIdAsync(message.UserId, cancellationToken);
        if (manager is not null)
        {
            manager.SyncFromIdentity(message.Email, MapRole(message.Role));
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return;
        }

        var applicant = await _applicants.GetByAuthIdAsync(message.UserId, cancellationToken);
        if (applicant is not null)
        {
            applicant.SyncFromIdentity(message.Email);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return;
        }

        _logger.LogInformation(
            "UserEdited received for unknown auth id {AuthId}; nothing to update.",
            message.UserId);
    }

    private static ManagerRole? MapRole(string? role) => role switch
    {
        Roles.RegularManager => ManagerRole.RegularManager,
        Roles.HeadManager => ManagerRole.HeadManager,
        _ => null
    };
}
