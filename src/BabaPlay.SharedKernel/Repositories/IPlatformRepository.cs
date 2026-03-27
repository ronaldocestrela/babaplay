using BabaPlay.SharedKernel.Entities;

namespace BabaPlay.SharedKernel.Repositories;

public interface IPlatformRepository<T> where T : BaseEntity
{
    IQueryable<T> Query();
    Task<T?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    void Update(T entity);
    void Remove(T entity);
}
