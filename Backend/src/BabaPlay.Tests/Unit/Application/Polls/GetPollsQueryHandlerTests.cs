using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Queries.Polls;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace BabaPlay.Tests.Unit.Application.Polls;

public class GetPollsQueryHandlerTests
{
    private readonly Mock<IPollRepository> _pollRepo = new();
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<ITenantContext> _tenantContext = new();
    private readonly GetPollsQueryHandler _handler;

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid AuthorId = Guid.NewGuid();
    private const string UserId = "user-abc";

    public GetPollsQueryHandlerTests()
    {
        _tenantContext.Setup(x => x.TenantId).Returns(TenantId);
        _userRepo
            .Setup(x => x.FindByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserAuthDto(AuthorId.ToString(), "diretoria@babaplay.com", true));

        _handler = new GetPollsQueryHandler(_pollRepo.Object, _userRepo.Object, _tenantContext.Object);
    }

    [Fact]
    public async Task Handle_WithActivePolls_ShouldReturnMappedDtosWithPercentages()
    {
        var poll = Poll.Create(TenantId, AuthorId, "Escolha de Local");
        var opt1 = poll.AddOption("Campo A");
        var opt2 = poll.AddOption("Campo B");
        poll.Vote(opt1.Id, UserId);

        _pollRepo
            .Setup(x => x.GetByTenantAsync(TenantId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync([poll]);

        var query = new GetPollsQuery(OnlyActive: true, RequestingUserId: UserId);

        var result = await _handler.HandleAsync(query);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value![0].Title.Should().Be("Escolha de Local");
        result.Value[0].HasVoted.Should().BeTrue();
        result.Value[0].VotedOptionId.Should().Be(opt1.Id);
        result.Value[0].TotalVotes.Should().Be(1);
        result.Value[0].Options[0].Percentage.Should().Be(100.0);
        result.Value[0].Options[1].Percentage.Should().Be(0.0);
    }

    [Fact]
    public async Task Handle_WhenEmpty_ShouldReturnEmptyList()
    {
        _pollRepo
            .Setup(x => x.GetByTenantAsync(TenantId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var query = new GetPollsQuery();

        var result = await _handler.HandleAsync(query);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}
