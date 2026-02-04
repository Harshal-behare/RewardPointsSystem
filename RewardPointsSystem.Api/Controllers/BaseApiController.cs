using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using RewardPointsSystem.Application.Common;
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

        /// <summary>
        /// Returns a paginated response with explicit parameters
        /// </summary>
        protected PagedResponse<T> PagedSuccess<T>(IEnumerable<T> items, int totalCount, int pageNumber, int pageSize)
        {
            return PagedResponse<T>.Create(items, pageNumber, pageSize, totalCount);
        }

        /// <summary>
        /// Returns a validation error response (422)
        /// </summary>
        protected IActionResult ValidationError(IEnumerable<string> errors)
        {
            var response = new ValidationErrorResponse
            {
                Success = false,
                Message = "Validation failed",
                Errors = new Dictionary<string, string[]> { { "General", errors.ToArray() } },
                StatusCode = StatusCodes.Status422UnprocessableEntity,
                Timestamp = DateTime.UtcNow,
                Path = HttpContext.Request.Path
            };
            return StatusCode(StatusCodes.Status422UnprocessableEntity, response);
        }

        /// <summary>
        /// Gets the current authenticated user's ID from claims
        /// </summary>
        protected Guid? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return null;

            return userId;
        }

        #region Result Pattern Mapping

        /// <summary>
        /// Maps a Result object to an appropriate HTTP response.
        /// This is the standard way to convert Application layer results to HTTP responses.
        /// </summary>
        protected IActionResult ToActionResult(Result result, string? successMessage = null)
        {
            if (result.IsSuccess)
                return Success<object?>(null, successMessage);

            return MapErrorToResponse(result);
        }

        /// <summary>
        /// Maps a Result&lt;T&gt; object to an appropriate HTTP response.
        /// </summary>
        protected IActionResult ToActionResult<T>(Result<T> result, string? successMessage = null)
        {
            if (result.IsSuccess)
                return Success(result.Data!, successMessage);

            return MapErrorToResponse(result);
        }

        /// <summary>
        /// Maps a Result&lt;T&gt; object to a 201 Created response on success.
        /// </summary>
        protected IActionResult ToCreatedResult<T>(Result<T> result, string? successMessage = null)
        {
            if (result.IsSuccess)
                return Created(result.Data!, successMessage ?? "Resource created successfully");

            return MapErrorToResponse(result);
        }

        /// <summary>
        /// Maps Result errors to appropriate HTTP status codes and responses.
        /// </summary>
        private IActionResult MapErrorToResponse(Result result)
        {
            return result.ErrorType switch
            {
                ErrorType.Validation when result.ValidationErrors.Any() => ValidationError(result.ValidationErrors),
                ErrorType.Validation => Error(result.ErrorMessage ?? "Validation failed", StatusCodes.Status422UnprocessableEntity),
                ErrorType.NotFound => NotFoundError(result.ErrorMessage ?? "Resource not found"),
                ErrorType.Conflict => ConflictError(result.ErrorMessage ?? "Conflict occurred"),
                ErrorType.Unauthorized => UnauthorizedError(result.ErrorMessage ?? "Unauthorized"),
                ErrorType.Forbidden => ForbiddenError(result.ErrorMessage ?? "Access denied"),
                ErrorType.BusinessRule => Error(result.ErrorMessage ?? "Business rule violation", StatusCodes.Status400BadRequest),
                ErrorType.ExternalService => Error(result.ErrorMessage ?? "External service error", StatusCodes.Status502BadGateway),
                ErrorType.Internal => Error(result.ErrorMessage ?? "Internal error", StatusCodes.Status500InternalServerError),
                _ => Error(result.ErrorMessage ?? "An error occurred", StatusCodes.Status400BadRequest)
            };
        }

        #endregion
    }
}
