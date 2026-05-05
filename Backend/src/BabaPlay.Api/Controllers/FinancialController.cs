using BabaPlay.Application.Commands.Financial;
using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Queries.Financial;
using BabaPlay.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BabaPlay.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Policy = AuthorizationPolicyNames.TenantMember)]
public sealed class FinancialController : ControllerBase
{
    private readonly ICommandHandler<CreateCashTransactionCommand, Result<CashTransactionResponse>> _createCashTransactionHandler;
    private readonly ICommandHandler<CreatePlayerMonthlyFeeCommand, Result<PlayerMonthlyFeeResponse>> _createMonthlyFeeHandler;
    private readonly ICommandHandler<RegisterMonthlyFeePaymentCommand, Result<MonthlyFeePaymentResponse>> _registerPaymentHandler;
    private readonly ICommandHandler<ReverseMonthlyFeePaymentCommand, Result<MonthlyFeePaymentResponse>> _reversePaymentHandler;
    private readonly IQueryHandler<GetCashFlowQuery, Result<CashFlowResponse>> _getCashFlowHandler;
    private readonly IQueryHandler<GetDelinquencyQuery, Result<DelinquencyResponse>> _getDelinquencyHandler;
    private readonly IQueryHandler<GetMonthlySummaryQuery, Result<MonthlySummaryResponse>> _getMonthlySummaryHandler;
    private readonly IQueryHandler<GetPlayerStatementQuery, Result<PlayerStatementResponse>> _getPlayerStatementHandler;

    public FinancialController(
        ICommandHandler<CreateCashTransactionCommand, Result<CashTransactionResponse>> createCashTransactionHandler,
        ICommandHandler<CreatePlayerMonthlyFeeCommand, Result<PlayerMonthlyFeeResponse>> createMonthlyFeeHandler,
        ICommandHandler<RegisterMonthlyFeePaymentCommand, Result<MonthlyFeePaymentResponse>> registerPaymentHandler,
        ICommandHandler<ReverseMonthlyFeePaymentCommand, Result<MonthlyFeePaymentResponse>> reversePaymentHandler,
        IQueryHandler<GetCashFlowQuery, Result<CashFlowResponse>> getCashFlowHandler,
        IQueryHandler<GetDelinquencyQuery, Result<DelinquencyResponse>> getDelinquencyHandler,
        IQueryHandler<GetMonthlySummaryQuery, Result<MonthlySummaryResponse>> getMonthlySummaryHandler,
        IQueryHandler<GetPlayerStatementQuery, Result<PlayerStatementResponse>> getPlayerStatementHandler)
    {
        _createCashTransactionHandler = createCashTransactionHandler;
        _createMonthlyFeeHandler = createMonthlyFeeHandler;
        _registerPaymentHandler = registerPaymentHandler;
        _reversePaymentHandler = reversePaymentHandler;
        _getCashFlowHandler = getCashFlowHandler;
        _getDelinquencyHandler = getDelinquencyHandler;
        _getMonthlySummaryHandler = getMonthlySummaryHandler;
        _getPlayerStatementHandler = getPlayerStatementHandler;
    }

    [HttpPost("cash-transaction")]
    [Authorize(Policy = AuthorizationPolicyNames.FinancialWrite)]
    [ProducesResponseType(typeof(CashTransactionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateCashTransaction([FromBody] CreateCashTransactionRequest request, CancellationToken ct)
    {
        var result = await _createCashTransactionHandler.HandleAsync(
            new CreateCashTransactionCommand(request.Type, request.Amount, request.OccurredOnUtc, request.Description, request.PlayerId),
            ct);

        if (!result.IsSuccess)
            return UnprocessableEntity(ToProblem(StatusCodes.Status422UnprocessableEntity, result));

        return CreatedAtAction(nameof(GetCashFlow), new { fromUtc = request.OccurredOnUtc.Date, toUtc = request.OccurredOnUtc.Date.AddDays(1).AddTicks(-1) }, result.Value);
    }

    [HttpPost("monthly-fee")]
    [Authorize(Policy = AuthorizationPolicyNames.FinancialWrite)]
    [ProducesResponseType(typeof(PlayerMonthlyFeeResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateMonthlyFee([FromBody] CreatePlayerMonthlyFeeRequest request, CancellationToken ct)
    {
        var result = await _createMonthlyFeeHandler.HandleAsync(
            new CreatePlayerMonthlyFeeCommand(
                request.PlayerId,
                request.Year,
                request.Month,
                request.Amount,
                request.DueDateUtc,
                request.Notes),
            ct);

        if (!result.IsSuccess)
        {
            var statusCode = result.ErrorCode switch
            {
                "PLAYER_NOT_FOUND" => StatusCodes.Status404NotFound,
                "MONTHLY_FEE_ALREADY_EXISTS" => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status422UnprocessableEntity,
            };

            return StatusCode(statusCode, ToProblem(statusCode, result));
        }

        return CreatedAtAction(nameof(GetPlayerStatement), new
        {
            playerId = result.Value!.PlayerId,
            fromUtc = request.DueDateUtc.Date.AddMonths(-1),
            toUtc = request.DueDateUtc.Date.AddMonths(1)
        }, result.Value);
    }

    [HttpPost("monthly-fee-payment")]
    [Authorize(Policy = AuthorizationPolicyNames.FinancialWrite)]
    [ProducesResponseType(typeof(MonthlyFeePaymentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RegisterPayment([FromBody] RegisterMonthlyFeePaymentRequest request, CancellationToken ct)
    {
        var result = await _registerPaymentHandler.HandleAsync(
            new RegisterMonthlyFeePaymentCommand(request.MonthlyFeeId, request.Amount, request.PaidAtUtc, request.Notes),
            ct);

        if (!result.IsSuccess)
        {
            var statusCode = result.ErrorCode == "MONTHLY_FEE_NOT_FOUND"
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status422UnprocessableEntity;

            return StatusCode(statusCode, ToProblem(statusCode, result));
        }

        return CreatedAtAction(nameof(RegisterPayment), null, result.Value);
    }

    [HttpPost("monthly-fee-payment/{paymentId:guid}/reverse")]
    [Authorize(Policy = AuthorizationPolicyNames.FinancialApprove)]
    [ProducesResponseType(typeof(MonthlyFeePaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ReversePayment(Guid paymentId, [FromBody] ReverseMonthlyFeePaymentRequest request, CancellationToken ct)
    {
        var result = await _reversePaymentHandler.HandleAsync(
            new ReverseMonthlyFeePaymentCommand(paymentId, request.ReversedAtUtc),
            ct);

        if (!result.IsSuccess)
        {
            var statusCode = result.ErrorCode switch
            {
                "MONTHLY_FEE_PAYMENT_NOT_FOUND" or "MONTHLY_FEE_NOT_FOUND" => StatusCodes.Status404NotFound,
                _ => StatusCodes.Status422UnprocessableEntity,
            };

            return StatusCode(statusCode, ToProblem(statusCode, result));
        }

        return Ok(result.Value);
    }

    [HttpGet("cash-flow")]
    [Authorize(Policy = AuthorizationPolicyNames.FinancialRead)]
    [ProducesResponseType(typeof(CashFlowResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> GetCashFlow([FromQuery] DateTime fromUtc, [FromQuery] DateTime toUtc, CancellationToken ct)
    {
        var result = await _getCashFlowHandler.HandleAsync(new GetCashFlowQuery(fromUtc, toUtc), ct);
        if (!result.IsSuccess)
            return UnprocessableEntity(ToProblem(StatusCodes.Status422UnprocessableEntity, result));

        return Ok(result.Value);
    }

    [HttpGet("delinquency")]
    [Authorize(Policy = AuthorizationPolicyNames.FinancialRead)]
    [ProducesResponseType(typeof(DelinquencyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> GetDelinquency([FromQuery] DateTime referenceUtc, CancellationToken ct)
    {
        var result = await _getDelinquencyHandler.HandleAsync(new GetDelinquencyQuery(referenceUtc), ct);
        if (!result.IsSuccess)
            return UnprocessableEntity(ToProblem(StatusCodes.Status422UnprocessableEntity, result));

        return Ok(result.Value);
    }

    [HttpGet("monthly-summary")]
    [Authorize(Policy = AuthorizationPolicyNames.FinancialRead)]
    [ProducesResponseType(typeof(MonthlySummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> GetMonthlySummary([FromQuery] int year, [FromQuery] int month, CancellationToken ct)
    {
        var result = await _getMonthlySummaryHandler.HandleAsync(new GetMonthlySummaryQuery(year, month), ct);
        if (!result.IsSuccess)
            return UnprocessableEntity(ToProblem(StatusCodes.Status422UnprocessableEntity, result));

        return Ok(result.Value);
    }

    [HttpGet("player/{playerId:guid}/statement")]
    [Authorize(Policy = AuthorizationPolicyNames.FinancialRead)]
    [ProducesResponseType(typeof(PlayerStatementResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> GetPlayerStatement(Guid playerId, [FromQuery] DateTime fromUtc, [FromQuery] DateTime toUtc, CancellationToken ct)
    {
        var result = await _getPlayerStatementHandler.HandleAsync(new GetPlayerStatementQuery(playerId, fromUtc, toUtc), ct);
        if (!result.IsSuccess)
            return UnprocessableEntity(ToProblem(StatusCodes.Status422UnprocessableEntity, result));

        return Ok(result.Value);
    }

    private static ProblemDetails ToProblem<T>(int statusCode, Result<T> result)
        => new()
        {
            Status = statusCode,
            Title = result.ErrorCode,
            Detail = result.ErrorMessage,
        };
}

public sealed record CreateCashTransactionRequest(
    Guid? PlayerId,
    CashTransactionType Type,
    decimal Amount,
    string Description,
    DateTime OccurredOnUtc);

public sealed record CreatePlayerMonthlyFeeRequest(
    Guid PlayerId,
    int Year,
    int Month,
    decimal Amount,
    DateTime DueDateUtc,
    string? Notes);

public sealed record RegisterMonthlyFeePaymentRequest(
    Guid MonthlyFeeId,
    decimal Amount,
    DateTime PaidAtUtc,
    string? Notes);

public sealed record ReverseMonthlyFeePaymentRequest(DateTime ReversedAtUtc);
