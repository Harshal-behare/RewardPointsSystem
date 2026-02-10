using System;
using System.Collections.Generic;

namespace RewardPointsSystem.Application.DTOs.Common
{
    /// <summary>
    /// Response for validation errors with detailed field-level errors
    /// </summary>
    public class ValidationErrorResponse : ErrorResponse
    {
        /// <summary>
        /// Dictionary of field names to their validation error messages
        /// </summary>
        public Dictionary<string, string[]> Errors { get; set; } = new Dictionary<string, string[]>();

        /// <summary>
        /// Creates a validation error response
        /// </summary>
        public static ValidationErrorResponse Create(Dictionary<string, string[]> errors, string path = null)
        {
            return new ValidationErrorResponse
            {
                Success = false,
                Message = "One or more validation errors occurred.",
                StatusCode = 422, // Unprocessable Entity
                Errors = errors,
                Path = path,
                Timestamp = DateTime.UtcNow
            };
        }
    }
}
