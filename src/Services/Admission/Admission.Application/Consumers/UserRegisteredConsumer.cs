using Admission.Application.Admissions.Contracts;
using Admission.Application.Messaging.Contracts;
using Admission.Application.Persistence.Contracts;
using Admission.Domain.Admissions;
using Admission.Domain.Applicants;
using Admission.Domain.Managers;
using Contracts.IntegrationEvents;
using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.Auth;

namespace Admission.Application.Consumers;

public sealed class UserRegisteredConsumer : IConsumer<UserRegistered>
{
    private readonly IApplicantRepository _applicants;
    private readonly IManagerRepository _managers;
    private readonly IAdmissionRepository _admissions;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserRegisteredConsumer> _logger;

    public UserRegisteredConsumer(
        IApplicantRepository applicants,
        IManagerRepository managers,
        IAdmissionRepository admissions,
        IUnitOfWork unitOfWork,
        ILogger<UserRegisteredConsumer> logger)
    {
        _applicants = applicants;
        _managers = managers;
        _admissions = admissions;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserRegistered> context)
    {
        var message = context.Message;
        var cancellationToken = context.CancellationToken;

        switch (message.Role)
        {
            case Roles.Applicant:
                await CreateApplicantAsync(message, cancellationToken);
                break;

            case Roles.RegularManager:
                await CreateManagerAsync(message, ManagerRole.RegularManager, cancellationToken);
                break;

            case Roles.HeadManager:
                await CreateManagerAsync(message, ManagerRole.HeadManager, cancellationToken);
                break;

            default:
                _logger.LogInformation(
                    "Ignoring UserRegistered for role {Role} and user {UserId}.",
                    message.Role,
                    message.UserId);
                return;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task CreateApplicantAsync(UserRegistered message, CancellationToken cancellationToken)
    {
        if (await _applicants.ExistsForAuthIdAsync(message.UserId, cancellationToken))
        {
            _logger.LogInformation(
                "Applicant for auth id {AuthId} already exists; skipping duplicate delivery.",
                message.UserId);
            return;
        }

        var applicant = Applicant.Register(message.UserId, message.Email, message.UserName);
        _applicants.Add(applicant);
        _admissions.Add(ApplicantAdmission.Open(applicant.Id));
    }

    private async Task CreateManagerAsync(
        UserRegistered message,
        ManagerRole role,
        CancellationToken cancellationToken)
    {
        if (await _managers.ExistsForAuthIdAsync(message.UserId, cancellationToken))
        {
            _logger.LogInformation(
                "Manager for auth id {AuthId} already exists; skipping duplicate delivery.",
                message.UserId);
            return;
        }

        _managers.Add(Manager.Create(message.UserId, message.Email, message.UserName, role));
    }
}
