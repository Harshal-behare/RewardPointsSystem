using System;

namespace RewardPointsSystem.Application.DTOs.Common
{
    /// <summary>
    /// Standard error response structure
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// Always false for error responses
        /// </summary>
        public bool Success { get; set; } = false;

        /// <summary>
        /// Error message describing what went wrong
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// HTTP status code
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Timestamp when the error occurred
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// The request path that caused the error
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Trace ID for tracking the error
        /// </summary>
        public string TraceId { get; set; }
    }
}
