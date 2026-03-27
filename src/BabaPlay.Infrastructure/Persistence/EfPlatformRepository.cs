using BabaPlay.SharedKernel.Entities;
using BabaPlay.SharedKernel.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Infrastructure.Persistence;

public sealed class EfPlatformRepository<T> : IPlatformRepository<T> where T : BaseEntity
{
    private readonly PlatformDbContext _db;

    public EfPlatformRepository(PlatformDbContext db) => _db = db;

    public IQueryable<T> Query() => _db.Set<T>().AsQueryable();

    public Task<T?> GetByIdAsync(string id, CancellationToken cancellationToken = default) =>
        _db.Set<T>().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default) =>
        await _db.Set<T>().AddAsync(entity, cancellationToken);

    public void Update(T entity) => _db.Set<T>().Update(entity);

    public void Remove(T entity) => _db.Set<T>().Remove(entity);
}
