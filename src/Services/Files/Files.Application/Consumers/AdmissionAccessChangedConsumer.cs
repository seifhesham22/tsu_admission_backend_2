using Contracts.IntegrationEvents;
using Files.Application.Persistence.Contracts;
using Files.Application.Storage.Contracts;
using Files.Domain;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Files.Application.Consumers;

public sealed class AdmissionAccessChangedConsumer : IConsumer<AdmissionAccessChanged>
{
    private readonly IAdmissionAccessRepository _access;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AdmissionAccessChangedConsumer> _logger;

    public AdmissionAccessChangedConsumer(
        IAdmissionAccessRepository access,
        IUnitOfWork unitOfWork,
        ILogger<AdmissionAccessChangedConsumer> logger)
    {
        _access = access;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AdmissionAccessChanged> context)
    {
        var message = context.Message;
        var cancellationToken = context.CancellationToken;

        var status = message.Status == AdmissionAccessStatus.Closed
            ? AccessStatus.Closed
            : AccessStatus.Open;

        var existing = await _access.GetAsync(message.ApplicantId, cancellationToken);

        if (existing is null)
        {
            _access.Add(AdmissionAccess.Create(
                message.ApplicantId,
                message.ApplicantAuthId,
                message.AssignedManagerAuthId,
                status,
                message.OccurredAtUtc));

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return;
        }

        var applied = existing.TryApply(
            message.ApplicantAuthId,
            message.AssignedManagerAuthId,
            status,
            message.OccurredAtUtc);

        if (!applied)
        {
            _logger.LogInformation(
                "Discarded stale AdmissionAccessChanged for applicant {ApplicantId} occurring at {OccurredAtUtc}.",
                message.ApplicantId,
                message.OccurredAtUtc);
            return;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
