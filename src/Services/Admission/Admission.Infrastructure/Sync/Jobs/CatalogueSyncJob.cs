using Admission.Application.Sync.Dtos;
using Admission.Application.Sync.Contracts;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Admission.Infrastructure.Sync.Jobs;

[DisallowConcurrentExecution]
public sealed class CatalogueSyncJob : IJob
{
    private readonly ICatalogueSyncService _syncService;
    private readonly ILogger<CatalogueSyncJob> _logger;

    public CatalogueSyncJob(ICatalogueSyncService syncService, ILogger<CatalogueSyncJob> logger)
    {
        _syncService = syncService;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        try
        {
            await _syncService.SyncAllAsync(context.CancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Scheduled catalogue synchronisation failed.");
            throw new JobExecutionException(exception, refireImmediately: false);
        }
    }
}
