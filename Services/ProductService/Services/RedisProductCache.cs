/*
 * Redis cache-aside implementation for products.
 * If Redis is unavailable during local development, the service fails open and
 * lets ProductService continue reading from SQL Server.
 */
using OrderFlow.ProductService.Models;
using StackExchange.Redis;
using System.Net.Sockets;
using System.Text.Json;

namespace OrderFlow.ProductService.Services;

public sealed class RedisProductCache(IConfiguration configuration, ILogger<RedisProductCache> logger) : IProductCache
{
    private const string ProductListCacheKey = "products:list";
    private IConnectionMultiplexer? connection;
    private bool redisUnavailable;

    public async Task<IReadOnlyList<Product>?> GetProductsAsync()
    {
        var database = await TryGetDatabaseAsync();
        if (database is null) return null;

        try
        {
            var cached = await database.StringGetAsync(ProductListCacheKey);
            return cached.HasValue
                ? JsonSerializer.Deserialize<IReadOnlyList<Product>>(cached.ToString())
                : null;
        }
        catch (RedisException ex)
        {
            MarkRedisUnavailable(ex);
            return null;
        }
    }

    public async Task SetProductsAsync(IReadOnlyList<Product> products)
    {
        var database = await TryGetDatabaseAsync();
        if (database is null) return;

        try
        {
            await database.StringSetAsync(ProductListCacheKey, JsonSerializer.Serialize(products), TimeSpan.FromMinutes(2));
        }
        catch (RedisException ex)
        {
            MarkRedisUnavailable(ex);
        }
    }

    public async Task InvalidateProductsAsync()
    {
        var database = await TryGetDatabaseAsync();
        if (database is null) return;

        try
        {
            await database.KeyDeleteAsync(ProductListCacheKey);
        }
        catch (RedisException ex)
        {
            MarkRedisUnavailable(ex);
        }
    }

    private async Task<IDatabase?> TryGetDatabaseAsync()
    {
        if (redisUnavailable) return null;

        try
        {
            connection ??= await ConnectAsync();
            return connection.GetDatabase();
        }
        catch (Exception ex) when (ex is RedisException or SocketException)
        {
            MarkRedisUnavailable(ex);
            return null;
        }
    }

    private void MarkRedisUnavailable(Exception ex)
    {
        redisUnavailable = true;
        logger.LogWarning(ex, "Redis is unavailable. ProductService will read products directly from SQL Server.");
    }

    private async Task<IConnectionMultiplexer> ConnectAsync()
    {
        var connectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";
        var options = ConfigurationOptions.Parse(connectionString);
        options.AbortOnConnectFail = false;
        options.ConnectRetry = 0;
        options.ConnectTimeout = 300;
        options.SyncTimeout = 300;

        return await ConnectionMultiplexer.ConnectAsync(options);
    }
}
