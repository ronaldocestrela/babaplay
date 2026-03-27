namespace BabaPlay.SharedKernel.Repositories;

public interface IPlatformUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
