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
    private readonly IQueryHandler<GetFinancialOverviewQuery, Result<FinancialOverviewResponse>> _getFinancialOverviewHandler;
    private readonly IQueryHandler<GetInvoicesQuery, Result<IReadOnlyList<InvoiceResponse>>> _getInvoicesHandler;
    private readonly ICommandHandler<GeneratePixPaymentCommand, Result<PixPaymentDetailsResponse>> _generatePixHandler;
    private readonly ICommandHandler<ConfirmPixPaymentCommand, Result<MonthlyFeePaymentResponse>> _confirmPixHandler;
    private readonly IQueryHandler<GetDefaultersListQuery, Result<DefaultersListResponse>> _getDefaultersHandler;
    private readonly ICommandHandler<SendPaymentReminderCommand, Result> _sendReminderHandler;
    private readonly IQueryHandler<GetFinancialStatementQuery, Result<FinancialStatementResponse>> _getFinancialStatementHandler;
    private readonly ICommandHandler<CreateFundraiserCommand, Result<FundraiserResponse>> _createFundraiserHandler;
    private readonly ICommandHandler<ContributeToFundraiserCommand, Result<FundraiserResponse>> _contributeFundraiserHandler;
    private readonly IQueryHandler<GetFundraisersQuery, Result<IReadOnlyList<FundraiserResponse>>> _getFundraisersHandler;

    public FinancialController(
        ICommandHandler<CreateCashTransactionCommand, Result<CashTransactionResponse>> createCashTransactionHandler,
        ICommandHandler<CreatePlayerMonthlyFeeCommand, Result<PlayerMonthlyFeeResponse>> createMonthlyFeeHandler,
        ICommandHandler<RegisterMonthlyFeePaymentCommand, Result<MonthlyFeePaymentResponse>> registerPaymentHandler,
        ICommandHandler<ReverseMonthlyFeePaymentCommand, Result<MonthlyFeePaymentResponse>> reversePaymentHandler,
        IQueryHandler<GetCashFlowQuery, Result<CashFlowResponse>> getCashFlowHandler,
        IQueryHandler<GetDelinquencyQuery, Result<DelinquencyResponse>> getDelinquencyHandler,
        IQueryHandler<GetMonthlySummaryQuery, Result<MonthlySummaryResponse>> getMonthlySummaryHandler,
        IQueryHandler<GetPlayerStatementQuery, Result<PlayerStatementResponse>> getPlayerStatementHandler,
        IQueryHandler<GetFinancialOverviewQuery, Result<FinancialOverviewResponse>> getFinancialOverviewHandler,
        IQueryHandler<GetInvoicesQuery, Result<IReadOnlyList<InvoiceResponse>>> getInvoicesHandler,
        ICommandHandler<GeneratePixPaymentCommand, Result<PixPaymentDetailsResponse>> generatePixHandler,
        ICommandHandler<ConfirmPixPaymentCommand, Result<MonthlyFeePaymentResponse>> confirmPixHandler,
        IQueryHandler<GetDefaultersListQuery, Result<DefaultersListResponse>> getDefaultersHandler,
        ICommandHandler<SendPaymentReminderCommand, Result> sendReminderHandler,
        IQueryHandler<GetFinancialStatementQuery, Result<FinancialStatementResponse>> getFinancialStatementHandler,
        ICommandHandler<CreateFundraiserCommand, Result<FundraiserResponse>> createFundraiserHandler,
        ICommandHandler<ContributeToFundraiserCommand, Result<FundraiserResponse>> contributeFundraiserHandler,
        IQueryHandler<GetFundraisersQuery, Result<IReadOnlyList<FundraiserResponse>>> getFundraisersHandler)
    {
        _createCashTransactionHandler = createCashTransactionHandler;
        _createMonthlyFeeHandler = createMonthlyFeeHandler;
        _registerPaymentHandler = registerPaymentHandler;
        _reversePaymentHandler = reversePaymentHandler;
        _getCashFlowHandler = getCashFlowHandler;
        _getDelinquencyHandler = getDelinquencyHandler;
        _getMonthlySummaryHandler = getMonthlySummaryHandler;
        _getPlayerStatementHandler = getPlayerStatementHandler;
        _getFinancialOverviewHandler = getFinancialOverviewHandler;
        _getInvoicesHandler = getInvoicesHandler;
        _generatePixHandler = generatePixHandler;
        _confirmPixHandler = confirmPixHandler;
        _getDefaultersHandler = getDefaultersHandler;
        _sendReminderHandler = sendReminderHandler;
        _getFinancialStatementHandler = getFinancialStatementHandler;
        _createFundraiserHandler = createFundraiserHandler;
        _contributeFundraiserHandler = contributeFundraiserHandler;
        _getFundraisersHandler = getFundraisersHandler;
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

    [HttpGet("overview")]
    [Authorize(Policy = AuthorizationPolicyNames.FinancialRead)]
    [ProducesResponseType(typeof(FinancialOverviewResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> GetOverview([FromQuery] int? year, [FromQuery] int? month, CancellationToken ct)
    {
        var result = await _getFinancialOverviewHandler.HandleAsync(new GetFinancialOverviewQuery(year, month), ct);
        if (!result.IsSuccess)
            return UnprocessableEntity(ToProblem(StatusCodes.Status422UnprocessableEntity, result));

        return Ok(result.Value);
    }

    [HttpGet("invoices")]
    [Authorize(Policy = AuthorizationPolicyNames.FinancialRead)]
    [ProducesResponseType(typeof(IReadOnlyList<InvoiceResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> GetInvoices(
        [FromQuery] int? year,
        [FromQuery] int? month,
        [FromQuery] Guid? playerId,
        [FromQuery] MonthlyFeeStatus? status,
        CancellationToken ct)
    {
        var result = await _getInvoicesHandler.HandleAsync(new GetInvoicesQuery(year, month, playerId, status), ct);
        if (!result.IsSuccess)
            return UnprocessableEntity(ToProblem(StatusCodes.Status422UnprocessableEntity, result));

        return Ok(result.Value);
    }

    [HttpPost("invoices/{id:guid}/pay-pix")]
    [Authorize(Policy = AuthorizationPolicyNames.FinancialRead)]
    [ProducesResponseType(typeof(PixPaymentDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> GeneratePix(Guid id, CancellationToken ct)
    {
        var result = await _generatePixHandler.HandleAsync(new GeneratePixPaymentCommand(id), ct);
        if (!result.IsSuccess)
        {
            var statusCode = result.ErrorCode == "MONTHLY_FEE_NOT_FOUND" ? StatusCodes.Status404NotFound : StatusCodes.Status422UnprocessableEntity;
            return StatusCode(statusCode, ToProblem(statusCode, result));
        }

        return Ok(result.Value);
    }

    [HttpPost("invoices/{id:guid}/confirm-pix")]
    [Authorize(Policy = AuthorizationPolicyNames.FinancialWrite)]
    [ProducesResponseType(typeof(MonthlyFeePaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ConfirmPix(Guid id, [FromBody] ConfirmPixRequest? request, CancellationToken ct)
    {
        var result = await _confirmPixHandler.HandleAsync(new ConfirmPixPaymentCommand(id, request?.TxId), ct);
        if (!result.IsSuccess)
        {
            var statusCode = result.ErrorCode == "MONTHLY_FEE_NOT_FOUND" ? StatusCodes.Status404NotFound : StatusCodes.Status422UnprocessableEntity;
            return StatusCode(statusCode, ToProblem(statusCode, result));
        }

        return Ok(result.Value);
    }

    [HttpGet("defaulters")]
    [Authorize(Policy = AuthorizationPolicyNames.FinancialRead)]
    [ProducesResponseType(typeof(DefaultersListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> GetDefaulters([FromQuery] DateTime? referenceUtc, CancellationToken ct)
    {
        var result = await _getDefaultersHandler.HandleAsync(new GetDefaultersListQuery(referenceUtc), ct);
        if (!result.IsSuccess)
            return UnprocessableEntity(ToProblem(StatusCodes.Status422UnprocessableEntity, result));

        return Ok(result.Value);
    }

    [HttpPost("defaulters/{playerId:guid}/remind")]
    [Authorize(Policy = AuthorizationPolicyNames.FinancialWrite)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> SendReminder(Guid playerId, CancellationToken ct)
    {
        var result = await _sendReminderHandler.HandleAsync(new SendPaymentReminderCommand(playerId), ct);
        if (!result.IsSuccess)
        {
            var statusCode = result.ErrorCode == "PLAYER_NOT_FOUND" ? StatusCodes.Status404NotFound : StatusCodes.Status422UnprocessableEntity;
            return StatusCode(statusCode, ToProblem(statusCode, result));
        }

        return Ok();
    }

    [HttpGet("statement")]
    [Authorize(Policy = AuthorizationPolicyNames.FinancialRead)]
    [ProducesResponseType(typeof(FinancialStatementResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> GetStatement([FromQuery] int? year, [FromQuery] int? month, CancellationToken ct)
    {
        var result = await _getFinancialStatementHandler.HandleAsync(new GetFinancialStatementQuery(year, month), ct);
        if (!result.IsSuccess)
            return UnprocessableEntity(ToProblem(StatusCodes.Status422UnprocessableEntity, result));

        return Ok(result.Value);
    }

    [HttpGet("fundraisers")]
    [Authorize(Policy = AuthorizationPolicyNames.FinancialRead)]
    [ProducesResponseType(typeof(IReadOnlyList<FundraiserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> GetFundraisers([FromQuery] bool? onlyActive, CancellationToken ct)
    {
        var result = await _getFundraisersHandler.HandleAsync(new GetFundraisersQuery(onlyActive), ct);
        if (!result.IsSuccess)
            return UnprocessableEntity(ToProblem(StatusCodes.Status422UnprocessableEntity, result));

        return Ok(result.Value);
    }

    [HttpPost("fundraisers")]
    [Authorize(Policy = AuthorizationPolicyNames.FinancialWrite)]
    [ProducesResponseType(typeof(FundraiserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateFundraiser([FromBody] CreateFundraiserRequest request, CancellationToken ct)
    {
        var result = await _createFundraiserHandler.HandleAsync(new CreateFundraiserCommand(request.Title, request.Description, request.TargetAmount, request.DeadlineUtc), ct);
        if (!result.IsSuccess)
            return UnprocessableEntity(ToProblem(StatusCodes.Status422UnprocessableEntity, result));

        return CreatedAtAction(nameof(GetFundraisers), new { id = result.Value!.Id }, result.Value);
    }

    [HttpPost("fundraisers/{id:guid}/contribute")]
    [Authorize(Policy = AuthorizationPolicyNames.FinancialWrite)]
    [ProducesResponseType(typeof(FundraiserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ContributeFundraiser(Guid id, [FromBody] ContributeFundraiserRequest request, CancellationToken ct)
    {
        var result = await _contributeFundraiserHandler.HandleAsync(new ContributeToFundraiserCommand(id, request.Amount, request.PlayerId, request.ContributorName), ct);
        if (!result.IsSuccess)
        {
            var statusCode = result.ErrorCode == "FUNDRAISER_NOT_FOUND" ? StatusCodes.Status404NotFound : StatusCodes.Status422UnprocessableEntity;
            return StatusCode(statusCode, ToProblem(statusCode, result));
        }

        return Ok(result.Value);
    }

    private static ProblemDetails ToProblem<T>(int statusCode, Result<T> result)
        => new()
        {
            Status = statusCode,
            Title = result.ErrorCode,
            Detail = result.ErrorMessage,
        };

    private static ProblemDetails ToProblem(int statusCode, Result result)
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

public sealed record ConfirmPixRequest(string? TxId);

public sealed record CreateFundraiserRequest(
    string Title,
    string Description,
    decimal TargetAmount,
    DateTime? DeadlineUtc);

public sealed record ContributeFundraiserRequest(
    decimal Amount,
    Guid? PlayerId,
    string? ContributorName);
