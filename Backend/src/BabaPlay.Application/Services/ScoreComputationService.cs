using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.ValueObjects;

namespace BabaPlay.Application.Services;

public sealed class ScoreComputationService : IScoreComputationService
{
    public int ComputeTotal(ScoreBreakdown breakdown)
        => breakdown.CalculateTotal();
}