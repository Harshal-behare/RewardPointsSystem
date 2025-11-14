using Microsoft.AspNetCore.Mvc;
using RewardPointsSystem.Application.DTOs.Common;

namespace RewardPointsSystem.Api.Controllers
{
    /// <summary>
    /// Base controller providing common functionality for all API controllers
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    public abstract class BaseApiController : ControllerBase
    {
        /// <summary>
        /// Returns a successful response with data
        /// </summary>
        protected IActionResult Success<T>(T data, string message = null)
        {
            var response = new ApiResponse<T>
            {
                Success = true,
                Data = data,
                Message = message,
                Timestamp = DateTime.UtcNow
            };
            return Ok(response);
        }

        /// <summary>
        /// Returns a created response (201) with data
        /// </summary>
        protected IActionResult Created<T>(T data, string message = "Resource created successfully")
        {
            var response = new ApiResponse<T>
            {
                Success = true,
                Data = data,
                Message = message,
                Timestamp = DateTime.UtcNow
            };
            return StatusCode(StatusCodes.Status201Created, response);
        }

        /// <summary>
        /// Returns an error response
        /// </summary>
        protected IActionResult Error(string message, int statusCode = StatusCodes.Status400BadRequest)
        {
            var response = new ErrorResponse
            {
                Success = false,
                Message = message,
                StatusCode = statusCode,
                Timestamp = DateTime.UtcNow,
                Path = HttpContext.Request.Path
            };
            return StatusCode(statusCode, response);
        }

        /// <summary>
        /// Returns a not found response (404)
        /// </summary>
        protected IActionResult NotFoundError(string message = "Resource not found")
        {
            return Error(message, StatusCodes.Status404NotFound);
        }

        /// <summary>
        /// Returns an unauthorized response (401)
        /// </summary>
        protected IActionResult UnauthorizedError(string message = "Unauthorized access")
        {
            return Error(message, StatusCodes.Status401Unauthorized);
        }

        /// <summary>
        /// Returns a forbidden response (403)
        /// </summary>
        protected IActionResult ForbiddenError(string message = "Forbidden - insufficient permissions")
        {
            return Error(message, StatusCodes.Status403Forbidden);
        }

        /// <summary>
        /// Returns a conflict response (409)
        /// </summary>
        protected IActionResult ConflictError(string message)
        {
            return Error(message, StatusCodes.Status409Conflict);
        }

        /// <summary>
        /// Returns a paginated response
        /// </summary>
        protected IActionResult PagedSuccess<T>(PagedResponse<T> pagedData, string message = null)
        {
            var response = new ApiResponse<PagedResponse<T>>
            {
                Success = true,
                Data = pagedData,
                Message = message,
                Timestamp = DateTime.UtcNow
            };
            return Ok(response);
        }
    }
}
