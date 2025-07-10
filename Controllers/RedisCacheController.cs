using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace RedisCaching.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RedisCacheController : ControllerBase
    {
        private readonly IDistributedCache _distributedCache;
        private readonly IConnectionMultiplexer _redisConnection;
        private readonly IConfiguration _configuration;

        public RedisCacheController(
            IDistributedCache distributedCache,
            IConnectionMultiplexer redisConnection,
            IConfiguration configuration,
            ILogger<RedisCacheController> logger)
        {
            _distributedCache = distributedCache;
            _redisConnection = redisConnection;
            _configuration = configuration;
        }

        private bool IsRedisConnected()
        {
            try
            {
                return _redisConnection.IsConnected;
            }
            catch
            {
                return false;
            }
        }

        [HttpGet("status")]
        public IActionResult GetRedisStatus()
        {
            return Ok(new { Status = IsRedisConnected() ? "Connected" : "Redis server down" });
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllCachedKeysAndValues()
        {
            if (!IsRedisConnected())
            {
                return StatusCode(503, new { message = "Redis server is currently unavailable" });
            }

            try
            {
                var server = _redisConnection.GetServer(_redisConnection.GetEndPoints().First());
                var keys = server.Keys().ToArray();
                string instanceName = _configuration["Redis:InstanceName"] ?? string.Empty;
                var cacheEntries = new List<KeyValuePair<string, string>>();

                foreach (var key in keys)
                {
                    var keyWithoutPrefix = key.ToString().Replace($"{instanceName}", "");
                    var value = await _distributedCache.GetStringAsync(keyWithoutPrefix);
                    cacheEntries.Add(new KeyValuePair<string, string>(keyWithoutPrefix, value ?? "null"));
                }

                return Ok(cacheEntries);
            }
            catch (RedisConnectionException ex)
            {
                return StatusCode(503, new { message = "Redis server is currently unavailable", details = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to retrieve cache entries", error = ex.Message });
            }
        }

        [HttpGet("{key}")]
        public async Task<IActionResult> GetCacheEntryByKey(string key)
        {
            if (!IsRedisConnected())
            {
                return StatusCode(503, new { message = "Redis server is currently unavailable" });
            }

            try
            {
                var value = await _distributedCache.GetStringAsync(key);
                return value == null
                    ? NotFound(new { message = "Cache entry not found." })
                    : Ok(new { Key = key, Value = value });
            }
            catch (RedisConnectionException ex)
            {
                return StatusCode(503, new { message = "Redis server is currently unavailable", details = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to retrieve cache entry", error = ex.Message });
            }
        }

        [HttpDelete("all")]
        public async Task<IActionResult> ClearAllCacheEntries()
        {
            if (!IsRedisConnected())
            {
                return StatusCode(503, new { message = "Redis server is currently unavailable" });
            }

            try
            {
                var server = _redisConnection.GetServer(_redisConnection.GetEndPoints().First());
                string instanceName = _configuration["Redis:InstanceName"] ?? string.Empty;

                foreach (var key in server.Keys())
                {
                    var keyWithoutPrefix = key.ToString().Replace($"{instanceName}:", "");
                    await _distributedCache.RemoveAsync(keyWithoutPrefix);
                }

                return Ok(new { message = "All cache entries cleared." });
            }
            catch (RedisConnectionException ex)
            {
                return StatusCode(503, new { message = "Redis server is currently unavailable", details = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to clear cache entries", error = ex.Message });
            }
        }

        [HttpDelete("{key}")]
        public async Task<IActionResult> ClearCacheEntryByKey(string key)
        {
            if (!IsRedisConnected())
            {
                return StatusCode(503, new { message = "Redis server is currently unavailable" });
            }

            try
            {
                await _distributedCache.RemoveAsync(key);
                return Ok(new { message = $"Cache entry '{key}' cleared." });
            }
            catch (RedisConnectionException ex)
            {
                return StatusCode(503, new { message = "Redis server is currently unavailable", details = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to clear cache entry", error = ex.Message });
            }
        }
    }
}