using System;
using System.Collections.Generic;

namespace BabaPlay.Web.Models;

public record SubmitMvpVoteDto(
    Guid MatchId,
    Guid VoterPlayerId,
    Guid CandidatePlayerId);

public record MvpCandidateDto(
    Guid PlayerId,
    string Name,
    string? PhotoUrl,
    string PositionName,
    string TeamName,
    int GoalsCount,
    int AssistsCount,
    int VotesCount,
    double Percentage,
    bool IsUserCandidate);

public record MvpResultDto(
    Guid MatchId,
    int TotalVotes,
    bool IsVotingOpen,
    Guid? UserVotedPlayerId,
    string? WinnerPlayerName,
    List<MvpCandidateDto> Candidates);
