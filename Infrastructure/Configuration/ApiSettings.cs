using System.Collections.Generic;

namespace Infrastructure.Configuration
{
    public class ApiSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public int RateLimitRequests { get; set; } = 100;
        public int RateLimitWindowMinutes { get; set; } = 60;
        public List<string> AllowedOrigins { get; set; } = new();
    }
}
