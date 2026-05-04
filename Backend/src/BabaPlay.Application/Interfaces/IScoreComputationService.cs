using BabaPlay.Domain.ValueObjects;

namespace BabaPlay.Application.Interfaces;

public interface IScoreComputationService
{
    int ComputeTotal(ScoreBreakdown breakdown);
}