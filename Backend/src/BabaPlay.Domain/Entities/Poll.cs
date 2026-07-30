namespace BabaPlay.Domain.Entities;

/// <summary>
/// Represents an interactive poll created by the association's board/admins for member voting.
/// Polls are tenant-scoped and contain a collection of choices (PollOptions) and user votes (PollVotes).
/// </summary>
public sealed class Poll : EntityBase
{
    public Guid TenantId { get; private set; }
    public Guid AuthorId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateTime? ExpiresAtUtc { get; private set; }
    public bool IsClosed { get; private set; }
    public bool IsActive { get; private set; }

    private readonly List<PollOption> _options = [];
    public IReadOnlyCollection<PollOption> Options => _options.AsReadOnly();

    private readonly List<PollVote> _votes = [];
    public IReadOnlyCollection<PollVote> Votes => _votes.AsReadOnly();

    private Poll() { }

    public static Poll Create(
        Guid tenantId,
        Guid authorId,
        string title,
        string? description = null,
        DateTime? expiresAtUtc = null)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("TenantId is required.", nameof(tenantId));

        if (authorId == Guid.Empty)
            throw new ArgumentException("AuthorId is required.", nameof(authorId));

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));

        return new Poll
        {
            TenantId = tenantId,
            AuthorId = authorId,
            Title = title.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            ExpiresAtUtc = expiresAtUtc?.ToUniversalTime(),
            IsClosed = false,
            IsActive = true,
        };
    }

    public PollOption AddOption(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Option text is required.", nameof(text));

        var option = PollOption.Create(Id, text.Trim(), _options.Count + 1);
        _options.Add(option);
        return option;
    }

    public PollVote Vote(Guid optionId, string userId)
    {
        if (IsClosed || (ExpiresAtUtc.HasValue && ExpiresAtUtc.Value <= DateTime.UtcNow))
            throw new InvalidOperationException("Cannot vote in a closed or expired poll.");

        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("UserId is required.", nameof(userId));

        var option = _options.FirstOrDefault(o => o.Id == optionId);
        if (option is null)
            throw new InvalidOperationException("Option not found in this poll.");

        if (_votes.Any(v => v.UserId == userId))
            throw new InvalidOperationException("User has already voted in this poll.");

        var vote = PollVote.Create(Id, optionId, userId);
        _votes.Add(vote);
        option.IncrementVotes();
        MarkUpdated();
        return vote;
    }

    public void Close()
    {
        if (IsClosed)
            return;

        IsClosed = true;
        MarkUpdated();
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        IsClosed = true;
        MarkUpdated();
    }
}
