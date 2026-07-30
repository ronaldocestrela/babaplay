using System;
using System.Threading;
using System.Threading.Tasks;
using BabaPlay.Application.Commands.Polls;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace BabaPlay.Tests.Unit.Application.Polls;

public class SubmitPollVoteCommandHandlerTests
{
    private readonly Mock<IPollRepository> _pollRepo = new();
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<ITenantContext> _tenantContext = new();
    private readonly SubmitPollVoteCommandHandler _handler;

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid AuthorId = Guid.NewGuid();
    private const string UserId = "user-123";

    public SubmitPollVoteCommandHandlerTests()
    {
        _tenantContext.Setup(x => x.TenantId).Returns(TenantId);
        _userRepo
            .Setup(x => x.FindByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserAuthDto(AuthorId.ToString(), "autor@babaplay.com", true));

        _handler = new SubmitPollVoteCommandHandler(_pollRepo.Object, _userRepo.Object, _tenantContext.Object);
    }

    [Fact]
    public async Task Handle_WhenPollNotFound_ShouldReturnNotFoundError()
    {
        var pollId = Guid.NewGuid();
        _pollRepo
            .Setup(x => x.GetByIdAsync(pollId, TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Poll?)null);

        var command = new SubmitPollVoteCommand(pollId, Guid.NewGuid(), UserId);

        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task Handle_WhenAlreadyVoted_ShouldReturnAlreadyVotedError()
    {
        var poll = Poll.Create(TenantId, AuthorId, "Enquete");
        var opt = poll.AddOption("Opção 1");

        _pollRepo
            .Setup(x => x.GetByIdAsync(poll.Id, TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(poll);

        _pollRepo
            .Setup(x => x.HasUserVotedAsync(poll.Id, UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new SubmitPollVoteCommand(poll.Id, opt.Id, UserId);

        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("ALREADY_VOTED");
    }

    [Fact]
    public async Task Handle_WithValidVote_ShouldRegisterVoteAndUpdatePercentages()
    {
        var poll = Poll.Create(TenantId, AuthorId, "Enquete");
        var opt1 = poll.AddOption("Opção 1");
        var opt2 = poll.AddOption("Opção 2");

        _pollRepo
            .Setup(x => x.GetByIdAsync(poll.Id, TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(poll);

        _pollRepo
            .Setup(x => x.HasUserVotedAsync(poll.Id, UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var command = new SubmitPollVoteCommand(poll.Id, opt1.Id, UserId);

        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.HasVoted.Should().BeTrue();
        result.Value.VotedOptionId.Should().Be(opt1.Id);
        result.Value.TotalVotes.Should().Be(1);
        result.Value.Options[0].Percentage.Should().Be(100.0);

        _pollRepo.Verify(x => x.AddVoteAsync(It.IsAny<PollVote>(), It.IsAny<CancellationToken>()), Times.Once);
        _pollRepo.Verify(x => x.UpdateAsync(poll, It.IsAny<CancellationToken>()), Times.Once);
    }
}
