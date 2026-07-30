namespace BabaPlay.Domain.Entities;

/// <summary>
/// Represents one selectable option within a poll.
/// </summary>
public sealed class PollOption
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid PollId { get; private set; }
    public string Text { get; private set; } = string.Empty;
    public int Order { get; private set; }
    public int VotesCount { get; private set; }

    // Navigation
    public Poll Poll { get; private set; } = null!;

    private PollOption() { }

    public static PollOption Create(Guid pollId, string text, int order)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Text is required.", nameof(text));

        return new PollOption
        {
            Id = Guid.NewGuid(),
            PollId = pollId,
            Text = text.Trim(),
            Order = order,
            VotesCount = 0
        };
    }

    internal void IncrementVotes()
    {
        VotesCount++;
    }
}
