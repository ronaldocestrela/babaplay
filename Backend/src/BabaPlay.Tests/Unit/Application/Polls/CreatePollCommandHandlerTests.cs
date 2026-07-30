using System;
using System.Collections.Generic;
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

public class CreatePollCommandHandlerTests
{
    private readonly Mock<IPollRepository> _pollRepo = new();
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<ITenantContext> _tenantContext = new();
    private readonly CreatePollCommandHandler _handler;

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid AuthorId = Guid.NewGuid();

    public CreatePollCommandHandlerTests()
    {
        _tenantContext.Setup(x => x.TenantId).Returns(TenantId);
        _userRepo
            .Setup(x => x.FindByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserAuthDto(AuthorId.ToString(), "admin@babaplay.com", true));

        _handler = new CreatePollCommandHandler(_pollRepo.Object, _userRepo.Object, _tenantContext.Object);
    }

    [Fact]
    public async Task Handle_WithEmptyTitle_ShouldReturnInvalidTitleError()
    {
        var command = new CreatePollCommand(AuthorId, string.Empty, "Descrição", null, ["Opção 1", "Opção 2"]);

        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_TITLE");
    }

    [Fact]
    public async Task Handle_WithLessThanTwoOptions_ShouldReturnMinimumOptionsError()
    {
        var command = new CreatePollCommand(AuthorId, "Título Enquete", "Descrição", null, ["Apenas 1 Opção"]);

        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("MINIMUM_OPTIONS_REQUIRED");
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreatePollAndReturnDto()
    {
        var command = new CreatePollCommand(
            AuthorId,
            "Qual o uniforme 2026?",
            "Votação para novo manto",
            null,
            ["Azul e Branco", "Preto e Vermelho"]);

        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Title.Should().Be("Qual o uniforme 2026?");
        result.Value.Options.Should().HaveCount(2);
        result.Value.TotalVotes.Should().Be(0);

        _pollRepo.Verify(x => x.AddAsync(It.IsAny<Poll>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
