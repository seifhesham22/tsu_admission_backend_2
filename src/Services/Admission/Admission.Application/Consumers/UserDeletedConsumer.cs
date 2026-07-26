using Admission.Application.Admissions.Contracts;
using Admission.Application.Messaging.Contracts;
using Admission.Application.Persistence.Contracts;
using Contracts.IntegrationEvents;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Admission.Application.Consumers;

public sealed class UserDeletedConsumer : IConsumer<UserDeleted>
{
    private readonly IManagerRepository _managers;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserDeletedConsumer> _logger;

    public UserDeletedConsumer(
        IManagerRepository managers,
        IUnitOfWork unitOfWork,
        ILogger<UserDeletedConsumer> logger)
    {
        _managers = managers;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserDeleted> context)
    {
        var message = context.Message;
        var cancellationToken = context.CancellationToken;

        var manager = await _managers.GetByAuthIdAsync(message.UserId, cancellationToken);
        if (manager is null)
        {
            _logger.LogInformation(
                "UserDeleted received for auth id {AuthId} with no manager record; treating as already applied.",
                message.UserId);
            return;
        }

        _managers.Remove(manager);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
