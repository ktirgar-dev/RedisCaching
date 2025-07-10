using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using RedisCaching.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;

public class RedisCacheService
{
    private readonly IDistributedCache _cache;
    private readonly RedisSettings _settings;

    public RedisCacheService(IDistributedCache cache, IOptions<RedisSettings> settings)
    {
        _cache = cache;
        _settings = settings.Value;
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> fallback)
    {
        if (!_settings.Enabled) return await fallback();

        try
        {
            var cachedData = await _cache.GetStringAsync(key);
            if (!string.IsNullOrEmpty(cachedData))
                return JsonSerializer.Deserialize<T>(cachedData);

            var data = await fallback();
            if (data != null)
            {
                await _cache.SetStringAsync(
                          key,
                          JsonSerializer.Serialize(data),
                          new DistributedCacheEntryOptions
                          {
                              SlidingExpiration = TimeSpan.FromMinutes(_settings.CacheDurationMinutes)
                          });
            }
            return data;
        }
        catch
        {
            return await fallback();
        }
    }

    public async Task RemoveAsync(string key)
    {
        if (!_settings.Enabled) return;

        try { await _cache.RemoveAsync(key); }
        catch { /* Log if needed */ }
    }
}