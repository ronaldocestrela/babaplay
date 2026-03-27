using BabaPlay.Modules.Financial.Entities;
using BabaPlay.SharedKernel.Repositories;
using BabaPlay.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Modules.Financial.Services;

public sealed class MembershipService
{
    private readonly ITenantRepository<Membership> _memberships;
    private readonly ITenantRepository<Payment> _payments;
    private readonly ITenantUnitOfWork _uow;

    public MembershipService(
        ITenantRepository<Membership> memberships,
        ITenantRepository<Payment> payments,
        ITenantUnitOfWork uow)
    {
        _memberships = memberships;
        _payments = payments;
        _uow = uow;
    }

    public async Task<Result<IReadOnlyList<Membership>>> ListForAssociateAsync(string associateId, CancellationToken ct)
    {
        var list = await _memberships.Query().Where(m => m.AssociateId == associateId).OrderByDescending(m => m.Year).ThenByDescending(m => m.Month)
            .ToListAsync(ct);
        return Result.Success<IReadOnlyList<Membership>>(list);
    }

    public async Task<Result<Membership>> CreateAsync(string associateId, int year, int month, decimal amount, CancellationToken ct)
    {
        if (year < 2000 || month is < 1 or > 12) return Result.Invalid<Membership>("Invalid period.");
        if (await _memberships.Query().AnyAsync(m => m.AssociateId == associateId && m.Year == year && m.Month == month, ct))
            return Result.Conflict<Membership>("Membership already exists for this period.");

        var m = new Membership { AssociateId = associateId, Year = year, Month = month, Amount = amount };
        await _memberships.AddAsync(m, ct);
        await _uow.SaveChangesAsync(ct);
        return Result.Success(m);
    }

    public async Task<Result<Payment>> RegisterPaymentAsync(string membershipId, decimal amount, string method, CancellationToken ct)
    {
        var membership = await _memberships.GetByIdAsync(membershipId, ct);
        if (membership is null) return Result.NotFound<Payment>("Membership not found.");
        var payment = new Payment { MembershipId = membershipId, Amount = amount, Method = method, PaidAt = DateTime.UtcNow };
        await _payments.AddAsync(payment, ct);
        membership.Status = MembershipStatus.Paid;
        membership.UpdatedAt = DateTime.UtcNow;
        _memberships.Update(membership);
        await _uow.SaveChangesAsync(ct);
        return Result.Success(payment);
    }
}
