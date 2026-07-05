using System.Collections.Concurrent;

namespace ProyectoIdentity.Helpers
{
    public static class RateLimiterManual
    {
        private static readonly ConcurrentDictionary<string, (int Count, DateTime Window)> _cache = new();

        public static bool IsBlocked(string key, int maxAttempts = 5, int windowMinutes = 1)
        {
            var now = DateTime.UtcNow;
            var entry = _cache.GetOrAdd(key, _ => (0, now));

            // Si la ventana expiró, reinicia
            if ((now - entry.Window).TotalMinutes >= windowMinutes)
                entry = (0, now);

            entry = (entry.Count + 1, entry.Window);
            _cache[key] = entry;

            return entry.Count > maxAttempts;
        }
    }
}
