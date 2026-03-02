using Application.Features.Cors;
using Application.Features.Cors.Models;
using Domain.Entities;
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

    public async Task<List<CorsOriginResponse>> GetAllAsync()
    {
        // no caching for administrative list
        var entities = await _context.CorsOrigins.ToListAsync();
        return entities.Select(x => new CorsOriginResponse(x.Id, x.Origin, x.IsActive)).ToList();
    }

    public async Task<CorsOriginResponse?> GetByIdAsync(string id)
    {
        var x = await _context.CorsOrigins.FindAsync(id);
        if (x == null) return null;
        return new CorsOriginResponse(x.Id, x.Origin, x.IsActive);
    }

    public async Task<CorsOriginResponse> CreateAsync(CreateCorsOriginRequest request)
    {
        // prevent duplicates
        if (await _context.CorsOrigins.AnyAsync(x => x.Origin == request.Origin))
            throw new Application.Exceptions.ConflictException(["Origin already registered."]);

        var entity = new Domain.Entities.CorsOrigin
        {
            Origin = request.Origin,
            IsActive = true
        };

        _context.CorsOrigins.Add(entity);
        await _context.SaveChangesAsync();
        ClearCache();
        return new CorsOriginResponse(entity.Id, entity.Origin, entity.IsActive);
    }

    public async Task<CorsOriginResponse> UpdateAsync(string id, UpdateCorsOriginRequest request)
    {
        var existing = await _context.CorsOrigins.FindAsync(id);
        if (existing == null)
            throw new Application.Exceptions.NotFoundException(["Cors origin record not found."]);

        // duplicate check on other id
        if (await _context.CorsOrigins.AnyAsync(x => x.Origin == request.Origin && x.Id != id))
            throw new Application.Exceptions.ConflictException(["Another entry with the same origin already exists."]);

        existing.Origin = request.Origin;
        existing.IsActive = request.IsActive;
        await _context.SaveChangesAsync();
        ClearCache();
        return new CorsOriginResponse(existing.Id, existing.Origin, existing.IsActive);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var existing = await _context.CorsOrigins.FindAsync(id);
        if (existing == null) return false;

        _context.CorsOrigins.Remove(existing);
        await _context.SaveChangesAsync();
        ClearCache();
        return true;
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
