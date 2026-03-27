namespace BabaPlay.SharedKernel.Repositories;

public interface ITenantUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
