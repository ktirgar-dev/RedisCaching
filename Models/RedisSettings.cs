namespace RedisCaching.Models
{
    public class RedisSettings
    {
        public bool Enabled { get; set; }
        public string ConnectionString { get; set; }
        public int CacheDurationMinutes { get; set; }
    }
}
