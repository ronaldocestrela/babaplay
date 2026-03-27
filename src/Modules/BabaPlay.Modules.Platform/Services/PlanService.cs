using BabaPlay.Modules.Platform.Entities;
using BabaPlay.SharedKernel.Repositories;
using BabaPlay.SharedKernel.Results;

namespace BabaPlay.Modules.Platform.Services;

public sealed class PlanService
{
    private readonly IPlatformRepository<Plan> _plans;
    private readonly IPlatformUnitOfWork _uow;

    public PlanService(IPlatformRepository<Plan> plans, IPlatformUnitOfWork uow)
    {
        _plans = plans;
        _uow = uow;
    }

    public async Task<Result<IReadOnlyList<Plan>>> ListAsync(CancellationToken ct)
    {
        var list = await Task.FromResult(_plans.Query().OrderBy(p => p.Name).ToList());
        return Result.Success<IReadOnlyList<Plan>>(list);
    }

    public async Task<Result<Plan>> GetAsync(string id, CancellationToken ct)
    {
        var plan = await _plans.GetByIdAsync(id, ct);
        return plan is null ? Result.NotFound<Plan>("Plan not found.") : Result.Success(plan);
    }

    public async Task<Result<Plan>> CreateAsync(
        string name,
        string? description,
        decimal monthlyPrice,
        int? maxAssociates,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(name)) return Result.Invalid<Plan>("Name is required.");
        var plan = new Plan
        {
            Name = name.Trim(),
            Description = description,
            MonthlyPrice = monthlyPrice,
            MaxAssociates = maxAssociates
        };
        await _plans.AddAsync(plan, ct);
        await _uow.SaveChangesAsync(ct);
        return Result.Success(plan);
    }

    public async Task<Result<Plan>> UpdateAsync(
        string id,
        string name,
        string? description,
        decimal monthlyPrice,
        int? maxAssociates,
        CancellationToken ct)
    {
        var plan = await _plans.GetByIdAsync(id, ct);
        if (plan is null) return Result.NotFound<Plan>("Plan not found.");
        plan.Name = name.Trim();
        plan.Description = description;
        plan.MonthlyPrice = monthlyPrice;
        plan.MaxAssociates = maxAssociates;
        plan.UpdatedAt = DateTime.UtcNow;
        _plans.Update(plan);
        await _uow.SaveChangesAsync(ct);
        return Result.Success(plan);
    }

    public async Task<Result> DeleteAsync(string id, CancellationToken ct)
    {
        var plan = await _plans.GetByIdAsync(id, ct);
        if (plan is null) return Result.Failure("Plan not found.", ResultStatus.NotFound);
        _plans.Remove(plan);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
