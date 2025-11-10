namespace Backend.Models.DTOs.Response
{
    public class HealthDetailsResponse
    {
        public required string Status { get; set; }
        public DateTime Timestamp { get; set; }
        public required string Service { get; set; }
        public required string Version { get; set; }
        public required HealthChecksResponse Checks { get; set; }
    }

    public class HealthChecksResponse
    {
        public required RiotApiKeyHealthResponse RiotApiKey { get; set; }
        public required EnvironmentHealthResponse Environment { get; set; }
    }

    public class RiotApiKeyHealthResponse
    {
        public required string Status { get; set; }
        public required string Message { get; set; }
    }

    public class EnvironmentHealthResponse
    {
        public required string Status { get; set; }
        public required string DotnetVersion { get; set; }
        public required string OsVersion { get; set; }
    }
}
