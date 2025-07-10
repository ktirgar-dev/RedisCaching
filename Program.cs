using Microsoft.EntityFrameworkCore;
using RedisCaching.Data;
using RedisCaching.Models;
using StackExchange.Redis;

namespace RedisCaching
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure Entity Framework Core with SQL Server
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // =============================================
            // Redis Configuration
            // =============================================

            // Bind Redis settings from configuration
            builder.Services.Configure<RedisSettings>(builder.Configuration.GetSection("Redis"));

            // Configure distributed Redis cache
            builder.Services.AddStackExchangeRedisCache(options =>
            {
                // Base Redis connection string
                options.Configuration = builder.Configuration["Redis:ConnectionString"];

                // Optional key prefix to prevent collisions in multi-tenant environments
                options.InstanceName = builder.Configuration["Redis:InstanceName"];
            });

            // Configure Redis ConnectionMultiplexer (advanced operations)
            builder.Services.AddSingleton<IConnectionMultiplexer>(provider =>
            {
                var configuration = ConfigurationOptions.Parse(
                    builder.Configuration["Redis:ConnectionString"]);

                // Continue operating without Redis while attempting to reconnect
                configuration.AbortOnConnectFail = false;

                // Wait up to 2 seconds for initial connection
                configuration.ConnectTimeout = 2000;

                // Retry connection every 1 second
                configuration.ReconnectRetryPolicy = new LinearRetry(1000);

                // Number of initial connection attempts
                configuration.ConnectRetry = 3;

                return ConnectionMultiplexer.Connect(configuration);
            });

            // Register our custom Redis service wrapper
            builder.Services.AddSingleton<RedisCacheService>();

            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = null;
                });

            // Add API Explorer services (needed for Swagger)
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}