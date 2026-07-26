using Admission.Application.Sync.Dtos;
namespace Admission.Application.Sync.Contracts;

public interface ICatalogueSyncService
{
    Task<CatalogueSyncResult> SyncAllAsync(CancellationToken cancellationToken = default);
}
