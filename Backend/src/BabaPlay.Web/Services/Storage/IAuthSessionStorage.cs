using System.Threading;
using System.Threading.Tasks;

namespace BabaPlay.Web.Services.Storage;

public interface IAuthSessionStorage
{
    Task<AuthSessionSnapshot?> LoadAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(AuthSessionSnapshot snapshot, CancellationToken cancellationToken = default);
    Task ClearAsync(CancellationToken cancellationToken = default);
}
