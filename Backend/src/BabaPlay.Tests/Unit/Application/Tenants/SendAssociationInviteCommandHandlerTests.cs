using BabaPlay.Application.Commands.Tenants;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.Tenants;

public class SendAssociationInviteCommandHandlerTests
{
    private readonly Mock<ITenantRepository> _tenantRepository = new();
    private readonly Mock<IUserTenantRepository> _userTenantRepository = new();
    private readonly Mock<IAssociationInviteRepository> _associationInviteRepository = new();
    private readonly Mock<IEmailDispatchQueue> _emailDispatchQueue = new();

    private readonly SendAssociationInviteCommandHandler _handler;

    public SendAssociationInviteCommandHandlerTests()
    {
        _handler = new SendAssociationInviteCommandHandler(
            _tenantRepository.Object,
            _userTenantRepository.Object,
            _associationInviteRepository.Object,
            _emailDispatchQueue.Object);
    }

    [Fact]
    public async Task Handle_WhenRequesterIsNotOwner_ShouldReturnForbidden()
    {
        _tenantRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TenantInfoDto(Guid.NewGuid(), "Club", "club", true, string.Empty, "Ready"));

        _userTenantRepository
            .Setup(x => x.IsOwnerAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.HandleAsync(new SendAssociationInviteCommand(
            Guid.NewGuid(),
            "user-1",
            "invitee@club.com",
            "http://localhost:5173/invite/accept",
            24));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("FORBIDDEN");
        _associationInviteRepository.Verify(x => x.AddAsync(It.IsAny<AssociationInviteData>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldCreateInviteAndQueueEmail()
    {
        var tenantId = Guid.NewGuid();

        _tenantRepository
            .Setup(x => x.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TenantInfoDto(tenantId, "Club", "club", true, string.Empty, "Ready"));

        _userTenantRepository
            .Setup(x => x.IsOwnerAsync("owner-1", tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _associationInviteRepository
            .Setup(x => x.GetActiveByTenantAndEmailAsync(tenantId, "invitee@club.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((AssociationInviteData?)null);

        var result = await _handler.HandleAsync(new SendAssociationInviteCommand(
            tenantId,
            "owner-1",
            "invitee@club.com",
            "http://localhost:5173/invite/accept",
            24));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.TenantId.Should().Be(tenantId);
        result.Value.Email.Should().Be("invitee@club.com");

        _associationInviteRepository.Verify(x => x.AddAsync(
            It.Is<AssociationInviteData>(d => d.TenantId == tenantId && d.Email == "invitee@club.com"),
            It.IsAny<CancellationToken>()), Times.Once);

        _emailDispatchQueue.Verify(x => x.EnqueueAsync(
            It.Is<EmailMessage>(m => m.To == "invitee@club.com" && m.Subject.Contains("Convite", StringComparison.OrdinalIgnoreCase)),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
