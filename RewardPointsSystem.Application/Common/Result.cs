namespace RewardPointsSystem.Application.Common;

/// <summary>
/// Represents the type of error that occurred during an operation.
/// This provides a standardized way to categorize errors across the Application layer.
/// </summary>
public enum ErrorType
{
    /// <summary>No error occurred</summary>
    None = 0,

    /// <summary>Validation failed (e.g., invalid input)</summary>
    Validation = 1,

    /// <summary>Resource was not found</summary>
    NotFound = 2,

    /// <summary>Operation would cause a conflict (e.g., duplicate)</summary>
    Conflict = 3,

    /// <summary>User is not authorized to perform this operation</summary>
    Unauthorized = 4,

    /// <summary>User lacks permission for this operation</summary>
    Forbidden = 5,

    /// <summary>Business rule violation</summary>
    BusinessRule = 6,

    /// <summary>External service failure</summary>
    ExternalService = 7,

    /// <summary>Unexpected internal error</summary>
    Internal = 8
}

/// <summary>
/// Represents the result of an operation that does not return data.
/// Use this for commands/mutations that only need to indicate success/failure.
/// </summary>
public class Result
{
    public bool IsSuccess { get; protected set; }
    public bool IsFailure => !IsSuccess;
    public ErrorType ErrorType { get; protected set; }
    public string? ErrorMessage { get; protected set; }
    public IReadOnlyList<string> ValidationErrors { get; protected set; } = Array.Empty<string>();

    protected Result() { }

    protected Result(bool isSuccess, ErrorType errorType, string? errorMessage, IEnumerable<string>? validationErrors = null)
    {
        IsSuccess = isSuccess;
        ErrorType = errorType;
        ErrorMessage = errorMessage;
        ValidationErrors = validationErrors?.ToList().AsReadOnly() ?? (IReadOnlyList<string>)Array.Empty<string>();
    }

    /// <summary>Creates a successful result</summary>
    public static Result Success() => new(true, ErrorType.None, null);

    /// <summary>Creates a failure result with the specified error type and message</summary>
    public static Result Failure(ErrorType errorType, string message) => 
        new(false, errorType, message);

    /// <summary>Creates a validation failure result</summary>
    public static Result ValidationFailure(string message) => 
        new(false, ErrorType.Validation, message);

    /// <summary>Creates a validation failure result with multiple errors</summary>
    public static Result ValidationFailure(IEnumerable<string> errors) => 
        new(false, ErrorType.Validation, "Validation failed", errors);

    /// <summary>Creates a not found failure result</summary>
    public static Result NotFound(string message) => 
        new(false, ErrorType.NotFound, message);

    /// <summary>Creates a conflict failure result</summary>
    public static Result Conflict(string message) => 
        new(false, ErrorType.Conflict, message);

    /// <summary>Creates an unauthorized failure result</summary>
    public static Result Unauthorized(string message = "Unauthorized") => 
        new(false, ErrorType.Unauthorized, message);

    /// <summary>Creates a forbidden failure result</summary>
    public static Result Forbidden(string message = "Access denied") => 
        new(false, ErrorType.Forbidden, message);

    /// <summary>Creates a business rule failure result</summary>
    public static Result BusinessRuleViolation(string message) => 
        new(false, ErrorType.BusinessRule, message);
}

/// <summary>
/// Represents the result of an operation that returns data of type T.
/// Use this for queries or commands that need to return data on success.
/// </summary>
/// <typeparam name="T">The type of data returned on success</typeparam>
public class Result<T> : Result
{
    public T? Data { get; private set; }

    private Result() { }

    private Result(bool isSuccess, T? data, ErrorType errorType, string? errorMessage, IEnumerable<string>? validationErrors = null)
        : base(isSuccess, errorType, errorMessage, validationErrors)
    {
        Data = data;
    }

    /// <summary>Creates a successful result with data</summary>
    public static Result<T> Success(T data) => 
        new(true, data, ErrorType.None, null);

    /// <summary>Creates a failure result with the specified error type and message</summary>
    public new static Result<T> Failure(ErrorType errorType, string message) => 
        new(false, default, errorType, message);

    /// <summary>Creates a validation failure result</summary>
    public new static Result<T> ValidationFailure(string message) => 
        new(false, default, ErrorType.Validation, message);

    /// <summary>Creates a validation failure result with multiple errors</summary>
    public new static Result<T> ValidationFailure(IEnumerable<string> errors) => 
        new(false, default, ErrorType.Validation, "Validation failed", errors);

    /// <summary>Creates a not found failure result</summary>
    public new static Result<T> NotFound(string message) => 
        new(false, default, ErrorType.NotFound, message);

    /// <summary>Creates a conflict failure result</summary>
    public new static Result<T> Conflict(string message) => 
        new(false, default, ErrorType.Conflict, message);

    /// <summary>Creates an unauthorized failure result</summary>
    public new static Result<T> Unauthorized(string message = "Unauthorized") => 
        new(false, default, ErrorType.Unauthorized, message);

    /// <summary>Creates a forbidden failure result</summary>
    public new static Result<T> Forbidden(string message = "Access denied") => 
        new(false, default, ErrorType.Forbidden, message);

    /// <summary>Creates a business rule failure result</summary>
    public new static Result<T> BusinessRuleViolation(string message) => 
        new(false, default, ErrorType.BusinessRule, message);

    /// <summary>
    /// Converts a non-generic Result to a generic Result&lt;T&gt; preserving the error.
    /// </summary>
    public static Result<T> FromResult(Result result)
    {
        if (result.IsSuccess)
            throw new InvalidOperationException("Cannot convert successful Result to Result<T> without data");
        
        return new Result<T>(false, default, result.ErrorType, result.ErrorMessage, result.ValidationErrors);
    }
}
