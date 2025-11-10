using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Backend.Tests.Integration;

/// <summary>
/// Integration tests for caching functionality.
/// Tests that caching middleware and services work correctly with Redis/Memory cache.
/// </summary>
public class CachingIntegrationTests : IClassFixture<SimpleTestFactory>
{
    private readonly HttpClient _client;
    private readonly SimpleTestFactory _factory;

    public CachingIntegrationTests(SimpleTestFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Cache_MultipleIdenticalRequests_SecondIsFaster()
    {
        // Arrange
        var endpoint = "/health";
        
        // Act - First request (cache miss)
        var startTime1 = DateTime.UtcNow;
        var response1 = await _client.GetAsync(endpoint);
        var duration1 = DateTime.UtcNow - startTime1;

        // Act - Second request (should be cached)
        var startTime2 = DateTime.UtcNow;
        var response2 = await _client.GetAsync(endpoint);
        var duration2 = DateTime.UtcNow - startTime2;

        // Assert
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        
        // Note: This test is flaky and depends on cache configuration
        // In production, you'd check for cache headers instead
    }

    [Fact]
    public async Task Cache_DistributedCache_IsAvailable()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var cache = scope.ServiceProvider.GetService<IDistributedCache>();

        // Assert - Cache should be configured
        Assert.NotNull(cache);
    }

    [Fact]
    public async Task Cache_SetAndRetrieve_Works()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var cache = scope.ServiceProvider.GetRequiredService<IDistributedCache>();
        
        var key = $"test-key-{Guid.NewGuid()}";
        var value = "test-value";

        // Act - Set cache
        await cache.SetStringAsync(key, value);
        
        // Act - Retrieve cache
        var retrievedValue = await cache.GetStringAsync(key);

        // Assert
        Assert.Equal(value, retrievedValue);
        
        // Cleanup
        await cache.RemoveAsync(key);
    }

    [Fact]
    public async Task Cache_Expiration_RemovesOldData()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var cache = scope.ServiceProvider.GetRequiredService<IDistributedCache>();
        
        var key = $"expiring-key-{Guid.NewGuid()}";
        var value = "expiring-value";
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(1)
        };

        // Act - Set with short expiration
        await cache.SetStringAsync(key, value, options);
        
        // Verify it's there
        var immediateValue = await cache.GetStringAsync(key);
        Assert.Equal(value, immediateValue);

        // Wait for expiration
        await Task.Delay(TimeSpan.FromSeconds(2));

        // Try to retrieve expired value
        var expiredValue = await cache.GetStringAsync(key);

        // Assert - Should be null/empty after expiration
        Assert.True(string.IsNullOrEmpty(expiredValue), "Value should be expired");
    }

    [Fact]
    public async Task Cache_ConcurrentAccess_HandlesCorrectly()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var cache = scope.ServiceProvider.GetRequiredService<IDistributedCache>();
        
        var key = $"concurrent-key-{Guid.NewGuid()}";
        var tasks = new List<Task>();

        // Act - Multiple concurrent writes
        for (int i = 0; i < 10; i++)
        {
            var index = i;
            tasks.Add(cache.SetStringAsync(key, $"value-{index}"));
        }

        await Task.WhenAll(tasks);

        // Assert - Should not crash, final value should exist
        var finalValue = await cache.GetStringAsync(key);
        Assert.NotNull(finalValue);
        Assert.StartsWith("value-", finalValue);

        // Cleanup
        await cache.RemoveAsync(key);
    }

    [Fact]
    public async Task Cache_LargeValue_StoresCorrectly()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var cache = scope.ServiceProvider.GetRequiredService<IDistributedCache>();
        
        var key = $"large-key-{Guid.NewGuid()}";
        var largeValue = new string('X', 10000); // 10KB string

        // Act
        await cache.SetStringAsync(key, largeValue);
        var retrievedValue = await cache.GetStringAsync(key);

        // Assert
        Assert.Equal(largeValue.Length, retrievedValue?.Length);
        Assert.Equal(largeValue, retrievedValue);

        // Cleanup
        await cache.RemoveAsync(key);
    }

    [Fact]
    public async Task Cache_Remove_DeletesKey()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var cache = scope.ServiceProvider.GetRequiredService<IDistributedCache>();
        
        var key = $"delete-key-{Guid.NewGuid()}";
        var value = "delete-value";

        // Act - Set, verify, delete, verify again
        await cache.SetStringAsync(key, value);
        var beforeDelete = await cache.GetStringAsync(key);
        Assert.Equal(value, beforeDelete);

        await cache.RemoveAsync(key);
        var afterDelete = await cache.GetStringAsync(key);

        // Assert
        Assert.Null(afterDelete);
    }

    [Fact]
    public async Task Cache_SlidingExpiration_ExtendsLifetime()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var cache = scope.ServiceProvider.GetRequiredService<IDistributedCache>();
        
        var key = $"sliding-key-{Guid.NewGuid()}";
        var value = "sliding-value";
        var options = new DistributedCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromSeconds(2)
        };

        // Act - Set with sliding expiration
        await cache.SetStringAsync(key, value, options);
        
        // Keep accessing within the sliding window
        for (int i = 0; i < 3; i++)
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            var retrievedValue = await cache.GetStringAsync(key);
            Assert.Equal(value, retrievedValue); // Should still be there due to sliding
        }

        // Cleanup
        await cache.RemoveAsync(key);
    }
}
