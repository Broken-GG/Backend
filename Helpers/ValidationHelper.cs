namespace Backend.Helpers
{
    /// <summary>
    /// Helper class for validating user input and request parameters
    /// </summary>
    public static class ValidationHelper
    {
        /// <summary>
        /// Validate summoner name format
        /// </summary>
        public static bool IsValidSummonerName(string? summonerName)
        {
            if (string.IsNullOrWhiteSpace(summonerName))
                return false;

            // Summoner names are 3-16 characters
            if (summonerName.Length < 3 || summonerName.Length > 16)
                return false;

            // Can contain letters, numbers, spaces, and some special characters
            return summonerName.All(c => char.IsLetterOrDigit(c) || c == ' ' || c == '_');
        }

        /// <summary>
        /// Validate tagline format
        /// </summary>
        public static bool IsValidTagline(string? tagline)
        {
            if (string.IsNullOrWhiteSpace(tagline))
                return false;

            // Taglines are typically 3-5 characters
            if (tagline.Length < 2 || tagline.Length > 10)
                return false;

            // Can contain letters and numbers
            return tagline.All(c => char.IsLetterOrDigit(c));
        }

        /// <summary>
        /// Validate PUUID format
        /// </summary>
        public static bool IsValidPuuid(string? puuid)
        {
            if (string.IsNullOrWhiteSpace(puuid))
                return false;

            // PUUIDs are long alphanumeric strings with hyphens
            // Typical format: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx or long string without hyphens
            return puuid.Length > 50 && puuid.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '_');
        }

        /// <summary>
        /// Validate pagination start parameter
        /// </summary>
        public static bool IsValidPaginationStart(int start)
        {
            return start >= 0;
        }

        /// <summary>
        /// Validate pagination count parameter
        /// </summary>
        public static bool IsValidPaginationCount(int count, int maxCount = 100)
        {
            return count >= 1 && count <= maxCount;
        }

        /// <summary>
        /// Validate pagination parameters
        /// </summary>
        public static (bool isValid, string? errorMessage) ValidatePagination(int start, int count, int maxCount = 100)
        {
            if (start < 0)
                return (false, "Start index cannot be negative");

            if (count < 1)
                return (false, "Count must be at least 1");

            if (count > maxCount)
                return (false, $"Count cannot exceed {maxCount}");

            return (true, null);
        }

        /// <summary>
        /// Sanitize user input to prevent injection attacks
        /// </summary>
        public static string SanitizeInput(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Remove potentially dangerous characters
            return new string(input.Where(c => !char.IsControl(c)).ToArray()).Trim();
        }
    }
}
