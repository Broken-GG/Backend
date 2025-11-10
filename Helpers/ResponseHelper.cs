using Newtonsoft.Json;

namespace Backend.Helpers
{
    /// <summary>
    /// Helper class for common response formatting
    /// </summary>
    public static class ResponseHelper
    {
        /// <summary>
        /// Create a standardized error response
        /// </summary>
        public static object CreateErrorResponse(string message, string? details = null, string? errorCode = null)
        {
            return new
            {
                success = false,
                error = new
                {
                    message,
                    details,
                    errorCode,
                    timestamp = DateTime.UtcNow
                }
            };
        }

        /// <summary>
        /// Create a standardized success response
        /// </summary>
        public static object CreateSuccessResponse(object data, string? message = null)
        {
            return new
            {
                success = true,
                data,
                message,
                timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Create a paginated response
        /// </summary>
        public static object CreatePaginatedResponse<T>(T[] items, int page, int pageSize, int? totalCount = null)
        {
            return new
            {
                success = true,
                data = items,
                pagination = new
                {
                    page,
                    pageSize,
                    count = items.Length,
                    totalCount
                },
                timestamp = DateTime.UtcNow
            };
        }
    }
}
