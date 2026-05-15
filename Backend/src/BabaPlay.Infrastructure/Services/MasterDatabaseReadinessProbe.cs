using BabaPlay.Application.Interfaces;
using BabaPlay.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Infrastructure.Services;

public sealed class MasterDatabaseReadinessProbe : IApiReadinessProbe
{
    private readonly MasterDbContext _masterDbContext;

    public MasterDatabaseReadinessProbe(MasterDbContext masterDbContext)
    {
        _masterDbContext = masterDbContext;
    }

    public async Task<bool> IsMasterDatabaseReadyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _masterDbContext.Database.CanConnectAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return false;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }
}