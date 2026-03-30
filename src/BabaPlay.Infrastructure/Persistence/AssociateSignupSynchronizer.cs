using BabaPlay.Modules.Associates.Entities;
using BabaPlay.SharedKernel.Results;
using BabaPlay.SharedKernel.Security;

namespace BabaPlay.Infrastructure.Persistence;

public sealed class AssociateSignupSynchronizer : IAssociateSignupSynchronizer
{
    private readonly TenantDbContext _db;

    public AssociateSignupSynchronizer(TenantDbContext db) => _db = db;

    public async Task<Result<string>> CreateAsync(string name, string email, string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Invalid<string>("Name is required.");
        if (string.IsNullOrWhiteSpace(email))
            return Result.Invalid<string>("Email is required.");
        if (string.IsNullOrWhiteSpace(userId))
            return Result.Invalid<string>("User id is required.");

        var associate = new Associate
        {
            Name = name.Trim(),
            Email = email.Trim(),
            UserId = userId
        };

        _db.Associates.Add(associate);
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
            return Result.Success(associate.Id);
        }
        catch (Exception ex)
        {
            return Result.Fail<string>($"Failed to create associate for signup: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(string associateId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(associateId))
            return Result.Failure("Associate id is required.", ResultStatus.Invalid);

        var associate = await _db.Associates.FindAsync([associateId], cancellationToken);
        if (associate is null)
            return Result.Success();

        _db.Associates.Remove(associate);
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to rollback associate: {ex.Message}", ResultStatus.Error);
        }
    }
}