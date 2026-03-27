using BabaPlay.Modules.Financial.Entities;
using BabaPlay.SharedKernel.Repositories;
using BabaPlay.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Modules.Financial.Services;

public sealed class CategoryService
{
    private readonly ITenantRepository<Category> _repo;
    private readonly ITenantUnitOfWork _uow;

    public CategoryService(ITenantRepository<Category> repo, ITenantUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<Result<IReadOnlyList<Category>>> ListAsync(CancellationToken ct)
    {
        var list = await _repo.Query().OrderBy(c => c.Name).ToListAsync(ct);
        return Result.Success<IReadOnlyList<Category>>(list);
    }

    public async Task<Result<Category>> CreateAsync(string name, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(name)) return Result.Invalid<Category>("Name is required.");
        var c = new Category { Name = name.Trim() };
        await _repo.AddAsync(c, ct);
        await _uow.SaveChangesAsync(ct);
        return Result.Success(c);
    }
}
