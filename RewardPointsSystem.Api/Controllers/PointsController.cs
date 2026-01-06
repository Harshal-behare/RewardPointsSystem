using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RewardPointsSystem.Application.DTOs.Common;
using RewardPointsSystem.Application.DTOs.Points;
using RewardPointsSystem.Application.Interfaces;

namespace RewardPointsSystem.Api.Controllers
{
    /// <summary>
    /// Manages points accounts and transactions
    /// </summary>
    [Authorize]
    public class PointsController : BaseApiController
    {
        private readonly IUserPointsAccountService _accountService;
        private readonly IUserPointsTransactionService _transactionService;
        private readonly IPointsAwardingService _awardingService;
        private readonly ILogger<PointsController> _logger;

        public PointsController(
            IUserPointsAccountService accountService,
            IUserPointsTransactionService transactionService,
            IPointsAwardingService awardingService,
            ILogger<PointsController> logger)
        {
            _accountService = accountService;
            _transactionService = transactionService;
            _awardingService = awardingService;
            _logger = logger;
        }

        /// <summary>
        /// Get user's points account
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <response code="200">Returns user's points account</response>
        /// <response code="404">User or account not found</response>
        [HttpGet("accounts/{userId}")]
        [ProducesResponseType(typeof(ApiResponse<PointsAccountResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserPointsAccount(Guid userId)
        {
            try
            {
                var account = await _accountService.GetAccountAsync(userId);
                // TODO: Map to PointsAccountResponseDto with user details
                
                return Success(account);
            }
            catch (KeyNotFoundException)
            {
                return NotFoundError($"Points account not found for user {userId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving points account for user {UserId}", userId);
                return Error("Failed to retrieve points account");
            }
        }

        /// <summary>
        /// Get user's transaction history (paginated)
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <response code="200">Returns paginated transactions</response>
        [HttpGet("transactions/{userId}")]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<TransactionResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserTransactions(Guid userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var transactions = await _transactionService.GetUserTransactionsAsync(userId);
                
                // Apply pagination
                var totalCount = transactions.Count();
                var pagedTransactions = transactions
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(t => new TransactionResponseDto
                    {
                        Id = t.Id,
                        UserId = t.UserId,
                        TransactionType = t.TransactionType.ToString(),
                        UserPoints = t.UserPoints,
                        Description = t.Description,
                        EventId = t.TransactionSource == RewardPointsSystem.Domain.Entities.Accounts.TransactionOrigin.Event ? t.SourceId : (Guid?)null,
                        RedemptionId = t.TransactionSource == RewardPointsSystem.Domain.Entities.Accounts.TransactionOrigin.Redemption ? t.SourceId : (Guid?)null,
                        Timestamp = t.Timestamp
                    });

                var response = PagedSuccess(pagedTransactions, totalCount, page, pageSize);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transactions for user {UserId}", userId);
                return Error("Failed to retrieve transactions");
            }
        }

        /// <summary>
        /// Get all transactions (Admin only)
        /// </summary>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <response code="200">Returns paginated transactions</response>
        [HttpGet("transactions")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<TransactionResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllTransactions([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            try
            {
                var transactions = await _transactionService.GetAllTransactionsAsync();
                
                // Apply pagination
                var totalCount = transactions.Count();
                var pagedTransactions = transactions
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(t => new TransactionResponseDto
                    {
                        Id = t.Id,
                        UserId = t.UserId,
                        TransactionType = t.TransactionType.ToString(),
                        UserPoints = t.UserPoints,
                        Description = t.Description,
                        EventId = t.TransactionSource == RewardPointsSystem.Domain.Entities.Accounts.TransactionOrigin.Event ? t.SourceId : (Guid?)null,
                        RedemptionId = t.TransactionSource == RewardPointsSystem.Domain.Entities.Accounts.TransactionOrigin.Redemption ? t.SourceId : (Guid?)null,
                        Timestamp = t.Timestamp
                    });

                var response = PagedSuccess(pagedTransactions, totalCount, page, pageSize);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all transactions");
                return Error("Failed to retrieve transactions");
            }
        }

        /// <summary>
        /// Award points to user (Admin only)
        /// </summary>
        /// <param name="dto">Points award data</param>
        /// <response code="200">Points awarded successfully</response>
        /// <response code="404">User not found</response>
        /// <response code="422">Validation failed</response>
        [HttpPost("award")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> AwardPoints([FromBody] AddPointsDto dto)
        {
            try
            {
                await _awardingService.AwardPointsAsync(dto.UserId, dto.Points, dto.Description, dto.EventId);
                return Success<object>(null, $"Successfully awarded {dto.Points} points");
            }
            catch (KeyNotFoundException)
            {
                return NotFoundError($"User with ID {dto.UserId} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error awarding points");
                return Error("Failed to award points");
            }
        }

        /// <summary>
        /// Deduct points from user (Admin only)
        /// </summary>
        /// <param name="dto">Points deduction data</param>
        /// <response code="200">Points deducted successfully</response>
        /// <response code="400">Insufficient balance</response>
        /// <response code="404">User not found</response>
        [HttpPost("deduct")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeductPoints([FromBody] DeductPointsDto dto)
        {
            try
            {
                var account = await _accountService.GetAccountAsync(dto.UserId);
                if (!account.HasSufficientBalance(dto.Points))
                    return Error($"Insufficient balance. Current balance: {account.CurrentBalance}, Required: {dto.Points}", 400);

                account.DebitPoints(dto.Points, Guid.Empty); // TODO: Get admin user ID from claims
                await _accountService.UpdateAccountAsync(account);

                return Success<object>(null, $"Successfully deducted {dto.Points} points");
            }
            catch (KeyNotFoundException)
            {
                return NotFoundError($"User with ID {dto.UserId} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deducting points");
                return Error("Failed to deduct points");
            }
        }

        /// <summary>
        /// Get points leaderboard
        /// </summary>
        /// <param name="top">Number of top users to return</param>
        /// <response code="200">Returns leaderboard</response>
        [HttpGet("leaderboard")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<PointsAccountResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetLeaderboard([FromQuery] int top = 10)
        {
            try
            {
                var accounts = await _accountService.GetTopAccountsAsync(top);
                // TODO: Map to PointsAccountResponseDto with user details
                
                return Success(accounts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving leaderboard");
                return Error("Failed to retrieve leaderboard");
            }
        }

        /// <summary>
        /// Get points system summary (Admin only)
        /// </summary>
        /// <response code="200">Returns points statistics</response>
        [HttpGet("summary")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPointsSummary()
        {
            try
            {
                var allAccounts = await _accountService.GetAllAccountsAsync();
                
                var summary = new
                {
                    TotalUsers = allAccounts.Count(),
                    TotalPointsDistributed = allAccounts.Sum(a => a.TotalEarned),
                    TotalPointsRedeemed = allAccounts.Sum(a => a.TotalRedeemed),
                    TotalPointsInCirculation = allAccounts.Sum(a => a.CurrentBalance),
                    AverageBalance = allAccounts.Average(a => a.CurrentBalance)
                };

                return Success(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving points summary");
                return Error("Failed to retrieve points summary");
            }
        }
    }
}
