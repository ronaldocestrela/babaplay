namespace BabaPlay.Application.Interfaces;

public interface IApiReadinessProbe
{
    Task<bool> IsMasterDatabaseReadyAsync(CancellationToken cancellationToken = default);
}