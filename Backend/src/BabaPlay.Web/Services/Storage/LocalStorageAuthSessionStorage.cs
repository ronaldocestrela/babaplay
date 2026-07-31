using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace BabaPlay.Web.Services.Storage;

public sealed class LocalStorageAuthSessionStorage : IAuthSessionStorage
{
    public const string StorageKey = "babaplay-auth-session";

    private readonly IJSRuntime _jsRuntime;

    public LocalStorageAuthSessionStorage(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<AuthSessionSnapshot?> LoadAsync(CancellationToken cancellationToken = default)
    {
        var json = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", cancellationToken, StorageKey);
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<AuthSessionSnapshot>(json);
    }

    public async Task SaveAsync(AuthSessionSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(snapshot);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", cancellationToken, StorageKey, json);
    }

    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", cancellationToken, StorageKey);
    }
}
