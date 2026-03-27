using System.Collections.Concurrent;

namespace BabaPlay.Infrastructure.Security;

public sealed class AllowedOriginsCache
{
    private readonly ConcurrentDictionary<string, byte> _origins = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyCollection<string> Origins => _origins.Keys.ToList();

    public void ReplaceAll(IEnumerable<string> origins)
    {
        _origins.Clear();
        foreach (var o in origins.Where(x => !string.IsNullOrWhiteSpace(x)))
            _origins[o.Trim()] = 1;
    }

    public bool Contains(string origin) => !string.IsNullOrWhiteSpace(origin) && _origins.ContainsKey(origin);
}
