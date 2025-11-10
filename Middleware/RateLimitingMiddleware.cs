using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Backend.Middleware
{
    /// <summary>
    /// Middleware to implement rate limiting for Riot API calls
    /// Prevents exceeding Riot API rate limits (20 requests/second, 100 requests/2 minutes)
    /// </summary>
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        
        // Track requests per IP address
        private static readonly ConcurrentDictionary<string, RequestTracker> _requestTrackers = new();
        
        // Rate limit settings (conservative to stay under Riot API limits)
        private const int MaxRequestsPerSecond = 15; // Under Riot's 20/sec limit
        private const int MaxRequestsPer2Minutes = 80; // Under Riot's 100/2min limit
        private static readonly TimeSpan OneSecondWindow = TimeSpan.FromSeconds(1);
        private static readonly TimeSpan TwoMinuteWindow = TimeSpan.FromMinutes(2);

        public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string clientId = GetClientIdentifier(context);
            RequestTracker tracker = _requestTrackers.GetOrAdd(clientId, _ => new RequestTracker());

            // Check rate limits
            if (!tracker.AllowRequest())
            {
                _logger.LogWarning("Rate limit exceeded for client {ClientId}", clientId);
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.Headers["Retry-After"] = "60";
                
                await context.Response.WriteAsJsonAsync(new
                {
                    success = false,
                    error = new
                    {
                        message = "Rate limit exceeded. Please try again later.",
                        statusCode = 429,
                        timestamp = DateTime.UtcNow
                    }
                });
                return;
            }

            await _next(context);
        }

        private static string GetClientIdentifier(HttpContext context)
        {
            // Use IP address as client identifier
            // In production, you might want to use authenticated user ID
            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        private class RequestTracker
        {
            private readonly ConcurrentQueue<DateTime> _shortTermRequests = new();
            private readonly ConcurrentQueue<DateTime> _longTermRequests = new();
            private readonly object _lock = new();

            public bool AllowRequest()
            {
                lock (_lock)
                {
                    DateTime now = DateTime.UtcNow;
                    
                    // Clean up old requests
                    CleanupOldRequests(now);
                    
                    // Check short-term limit (per second)
                    if (_shortTermRequests.Count >= MaxRequestsPerSecond)
                    {
                        return false;
                    }
                    
                    // Check long-term limit (per 2 minutes)
                    if (_longTermRequests.Count >= MaxRequestsPer2Minutes)
                    {
                        return false;
                    }
                    
                    // Allow request and track it
                    _shortTermRequests.Enqueue(now);
                    _longTermRequests.Enqueue(now);
                    
                    return true;
                }
            }

            private void CleanupOldRequests(DateTime now)
            {
                // Remove requests older than 1 second
                while (_shortTermRequests.TryPeek(out DateTime timestamp) && 
                       now - timestamp > OneSecondWindow)
                {
                    _shortTermRequests.TryDequeue(out _);
                }
                
                // Remove requests older than 2 minutes
                while (_longTermRequests.TryPeek(out DateTime timestamp) && 
                       now - timestamp > TwoMinuteWindow)
                {
                    _longTermRequests.TryDequeue(out _);
                }
            }
        }
    }
}
