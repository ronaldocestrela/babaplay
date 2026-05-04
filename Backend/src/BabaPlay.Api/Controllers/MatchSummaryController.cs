using BabaPlay.Application.Commands.MatchSummaries;
using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Queries.MatchSummaries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BabaPlay.Api.Controllers;

/// <summary>Generates and retrieves PDF match summaries for the current tenant.</summary>
[ApiController]
[Route("api/v1/match-summary")]
[Authorize(Policy = AuthorizationPolicyNames.TenantMember)]
public sealed class MatchSummaryController : ControllerBase
{
    private readonly ICommandHandler<GenerateMatchSummaryCommand, Result<MatchSummaryResponse>> _generateHandler;
    private readonly ICommandHandler<DeleteMatchSummaryCommand, Result> _deleteHandler;
    private readonly IQueryHandler<GetMatchSummaryQuery, Result<MatchSummaryResponse>> _getByIdHandler;
    private readonly IQueryHandler<GetMatchSummaryByMatchQuery, Result<MatchSummaryResponse>> _getByMatchHandler;
    private readonly IQueryHandler<GetMatchSummaryFileQuery, Result<MatchSummaryFileResponse>> _getFileHandler;

    public MatchSummaryController(
        ICommandHandler<GenerateMatchSummaryCommand, Result<MatchSummaryResponse>> generateHandler,
        ICommandHandler<DeleteMatchSummaryCommand, Result> deleteHandler,
        IQueryHandler<GetMatchSummaryQuery, Result<MatchSummaryResponse>> getByIdHandler,
        IQueryHandler<GetMatchSummaryByMatchQuery, Result<MatchSummaryResponse>> getByMatchHandler,
        IQueryHandler<GetMatchSummaryFileQuery, Result<MatchSummaryFileResponse>> getFileHandler)
    {
        _generateHandler = generateHandler;
        _deleteHandler = deleteHandler;
        _getByIdHandler = getByIdHandler;
        _getByMatchHandler = getByMatchHandler;
        _getFileHandler = getFileHandler;
    }

    /// <summary>Generates a PDF summary for a completed match.</summary>
    [HttpPost]
    [Authorize(Policy = AuthorizationPolicyNames.MatchesWrite)]
    [ProducesResponseType(typeof(MatchSummaryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Generate([FromBody] GenerateMatchSummaryRequest request, CancellationToken ct)
    {
        var result = await _generateHandler.HandleAsync(new GenerateMatchSummaryCommand(request.MatchId), ct);

        if (!result.IsSuccess)
        {
            var statusCode = result.ErrorCode switch
            {
                "MATCH_NOT_FOUND" => StatusCodes.Status404NotFound,
                "MATCH_SUMMARY_ALREADY_EXISTS" => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status422UnprocessableEntity,
            };

            return StatusCode(statusCode, new ProblemDetails
            {
                Status = statusCode,
                Title = result.ErrorCode,
                Detail = result.ErrorMessage,
            });
        }

        return CreatedAtAction(nameof(GetByMatchId), new { matchId = result.Value!.MatchId }, result.Value);
    }

    /// <summary>Gets summary metadata by summary id.</summary>
    [HttpGet("{summaryId:guid}")]
    [Authorize(Policy = AuthorizationPolicyNames.MatchesRead)]
    [ProducesResponseType(typeof(MatchSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid summaryId, CancellationToken ct)
    {
        var result = await _getByIdHandler.HandleAsync(new GetMatchSummaryQuery(summaryId), ct);

        if (!result.IsSuccess)
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = result.ErrorCode,
                Detail = result.ErrorMessage,
            });

        return Ok(result.Value);
    }

    /// <summary>Gets summary metadata by match id.</summary>
    [HttpGet("match/{matchId:guid}")]
    [Authorize(Policy = AuthorizationPolicyNames.MatchesRead)]
    [ProducesResponseType(typeof(MatchSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByMatchId(Guid matchId, CancellationToken ct)
    {
        var result = await _getByMatchHandler.HandleAsync(new GetMatchSummaryByMatchQuery(matchId), ct);

        if (!result.IsSuccess)
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = result.ErrorCode,
                Detail = result.ErrorMessage,
            });

        return Ok(result.Value);
    }

    /// <summary>Downloads the PDF file by summary id.</summary>
    [HttpGet("{summaryId:guid}/file")]
    [Authorize(Policy = AuthorizationPolicyNames.MatchesRead)]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Download(Guid summaryId, CancellationToken ct)
    {
        var result = await _getFileHandler.HandleAsync(new GetMatchSummaryFileQuery(summaryId), ct);

        if (!result.IsSuccess)
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = result.ErrorCode,
                Detail = result.ErrorMessage,
            });

        return File(result.Value!.Content, result.Value.ContentType, result.Value.FileName);
    }

    /// <summary>Deletes a summary by id (soft delete + storage file delete).</summary>
    [HttpDelete("{summaryId:guid}")]
    [Authorize(Policy = AuthorizationPolicyNames.MatchesWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Delete(Guid summaryId, CancellationToken ct)
    {
        var result = await _deleteHandler.HandleAsync(new DeleteMatchSummaryCommand(summaryId), ct);

        if (!result.IsSuccess)
        {
            var statusCode = result.ErrorCode switch
            {
                "MATCH_SUMMARY_NOT_FOUND" => StatusCodes.Status404NotFound,
                _ => StatusCodes.Status422UnprocessableEntity,
            };

            return StatusCode(statusCode, new ProblemDetails
            {
                Status = statusCode,
                Title = result.ErrorCode,
                Detail = result.ErrorMessage,
            });
        }

        return NoContent();
    }
}

public sealed record GenerateMatchSummaryRequest(Guid MatchId);
