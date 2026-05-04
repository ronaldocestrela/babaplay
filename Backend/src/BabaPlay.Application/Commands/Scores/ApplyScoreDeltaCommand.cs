using BabaPlay.Application.Common;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Scores;

public sealed record ApplyScoreDeltaCommand(
    Guid SourceEventId,
    Guid PlayerId,
    int AttendanceDelta,
    int WinsDelta,
    int DrawsDelta,
    int GoalsDelta,
    int YellowCardsDelta,
    int RedCardsDelta)
    : ICommand<Result>;