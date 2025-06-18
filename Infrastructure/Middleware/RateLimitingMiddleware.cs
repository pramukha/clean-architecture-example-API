using Infrastructure.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System;

namespace Infrastructure.Middleware
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        private readonly ApiSettings _apiSettings;
        private static readonly ConcurrentDictionary<string, TokenBucket> _tokenBuckets = new();

        public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger, ApiSettings apiSettings)
        {
            _next = next;
            _logger = logger;
            _apiSettings = apiSettings;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var tokenBucket = _tokenBuckets.GetOrAdd(ipAddress, _ => new TokenBucket(_apiSettings.RateLimitRequests, TimeSpan.FromMinutes(_apiSettings.RateLimitWindowMinutes)));

            if (!tokenBucket.TryConsume())
            {
                _logger.LogWarning("Rate limit exceeded for IP: {IpAddress}", ipAddress);
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.Response.WriteAsync("Too many requests. Please try again later.");
                return;
            }

            await _next(context);
        }

        private class TokenBucket
        {
            private readonly int _capacity;
            private readonly TimeSpan _refillTime;
            private int _tokens;
            private DateTime _lastRefill;
            private readonly object _lock = new();

            public TokenBucket(int capacity, TimeSpan refillTime)
            {
                _capacity = capacity;
                _refillTime = refillTime;
                _tokens = capacity;
                _lastRefill = DateTime.UtcNow;
            }

            public bool TryConsume()
            {
                lock (_lock)
                {
                    RefillTokens();
                    if (_tokens > 0)
                    {
                        _tokens--;
                        return true;
                    }
                    return false;
                }
            }

            private void RefillTokens()
            {
                var now = DateTime.UtcNow;
                var timePassed = now - _lastRefill;
                var tokensToAdd = (int)(timePassed.TotalSeconds / _refillTime.TotalSeconds * _capacity);
                if (tokensToAdd > 0)
                {
                    _tokens = Math.Min(_capacity, _tokens + tokensToAdd);
                    _lastRefill = now;
                }
            }
        }
    }
}
