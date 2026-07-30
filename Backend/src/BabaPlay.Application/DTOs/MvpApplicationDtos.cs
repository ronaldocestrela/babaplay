using System;
using System.Collections.Generic;

namespace BabaPlay.Application.DTOs;

public record MvpCandidateApplicationDto(
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

public record MvpResultApplicationDto(
    Guid MatchId,
    int TotalVotes,
    bool IsVotingOpen,
    Guid? UserVotedPlayerId,
    string? WinnerPlayerName,
    List<MvpCandidateApplicationDto> Candidates);
