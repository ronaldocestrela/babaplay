namespace BabaPlay.Domain.Entities;

/// <summary>
/// Junction entity recording a user's vote for a specific option in a poll.
/// </summary>
public sealed class PollVote
{
    public Guid PollId { get; private set; }
    public Guid OptionId { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public DateTime VotedAtUtc { get; private set; }

    // Navigation
    public Poll Poll { get; private set; } = null!;
    public PollOption Option { get; private set; } = null!;

    private PollVote() { }

    public static PollVote Create(Guid pollId, Guid optionId, string userId)
    {
        if (pollId == Guid.Empty)
            throw new ArgumentException("PollId is required.", nameof(pollId));

        if (optionId == Guid.Empty)
            throw new ArgumentException("OptionId is required.", nameof(optionId));

        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("UserId is required.", nameof(userId));

        return new PollVote
        {
            PollId = pollId,
            OptionId = optionId,
            UserId = userId.Trim(),
            VotedAtUtc = DateTime.UtcNow
        };
    }
}
