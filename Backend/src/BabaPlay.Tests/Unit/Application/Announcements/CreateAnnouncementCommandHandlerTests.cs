using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BabaPlay.Application.Commands.Announcements;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace BabaPlay.Tests.Unit.Application.Announcements;

public class CreateAnnouncementCommandHandlerTests
{
    private readonly Mock<IAnnouncementRepository> _announcementRepo = new();
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<ITenantContext> _tenantContext = new();
    private readonly CreateAnnouncementCommandHandler _handler;

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid AuthorId = Guid.NewGuid();

    public CreateAnnouncementCommandHandlerTests()
    {
        _tenantContext.Setup(x => x.TenantId).Returns(TenantId);
        _userRepo
            .Setup(x => x.FindByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserAuthDto(AuthorId.ToString(), "admin@babaplay.com", true));

        _handler = new CreateAnnouncementCommandHandler(
            _announcementRepo.Object,
            _userRepo.Object,
            _tenantContext.Object);
    }

    [Fact]
    public async Task Handle_WithEmptyTitle_ShouldReturnInvalidTitleError()
    {
        var command = new CreateAnnouncementCommand(AuthorId, string.Empty, "Conteúdo válido");

        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_TITLE");
    }

    [Fact]
    public async Task Handle_WithEmptyContent_ShouldReturnInvalidContentError()
    {
        var command = new CreateAnnouncementCommand(AuthorId, "Título válido", "   ");

        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_CONTENT");
    }

    [Fact]
    public async Task Handle_WithEmptyAuthorId_ShouldReturnInvalidAuthorError()
    {
        var command = new CreateAnnouncementCommand(Guid.Empty, "Título", "Conteúdo");

        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_AUTHOR");
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateAndPublishAnnouncementAndReturnDto()
    {
        var command = new CreateAnnouncementCommand(
            AuthorId,
            "Mudança de Horário",
            "O baba começa às 9h a partir de sábado.",
            ExpiresAtUtc: null,
            Tags: "importante");

        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Title.Should().Be("Mudança de Horário");
        result.Value.IsPublished.Should().BeTrue();
        result.Value.IsNew.Should().BeTrue();
        result.Value.IsRead.Should().BeFalse();
        result.Value.Tags.Should().Be("importante");

        _announcementRepo.Verify(
            x => x.AddAsync(It.IsAny<Announcement>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldResolveAuthorName()
    {
        var command = new CreateAnnouncementCommand(AuthorId, "Aviso", "Conteúdo do aviso");

        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeTrue();
        result.Value!.AuthorName.Should().Be("admin@babaplay.com");
    }
}
