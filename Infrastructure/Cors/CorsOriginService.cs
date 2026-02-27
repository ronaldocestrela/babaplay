using Application.Features.Cors;
using Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Infrastructure.Cors;

public class CorsOriginService(SharedDbContext context, IMemoryCache cache) : ICorsOriginService
{
    private readonly SharedDbContext _context = context;
    private readonly IMemoryCache _cache = cache;
    private const string CACHE_KEY = "CORS_ALLOWED_ORIGINS";

    // semaphore to protect cache population when multiple threads
    // try to read simultaneously and the cache is cold.
    private static readonly SemaphoreSlim _cacheLock = new(1, 1);

    public void ClearCache()
    {
        _cache.Remove(CACHE_KEY);
    }

    public async Task<List<string>> GetAllowedOriginsAsync()
    {
        if (_cache.TryGetValue(CACHE_KEY, out List<string>? origins))
            return origins!;

        await _cacheLock.WaitAsync();
        try
        {
            // double-check after obtaining lock
            if (_cache.TryGetValue(CACHE_KEY, out origins))
                return origins!;

            origins = await _context.CorsOrigins
                .Where(x => x.IsActive)
                .Select(x => x.Origin)
                .ToListAsync();

            _cache.Set(CACHE_KEY, origins, TimeSpan.FromMinutes(10));
            return origins;
        }
        finally
        {
            _cacheLock.Release();
        }
    }
}
