using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Queries.Tenants;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.Tenants;

public class ValidateAssociationInviteQueryHandlerTests
{
    private readonly Mock<IAssociationInviteRepository> _associationInviteRepository = new();
    private readonly Mock<ITenantRepository> _tenantRepository = new();
    private readonly Mock<IUserRepository> _userRepository = new();

    private readonly ValidateAssociationInviteQueryHandler _handler;

    public ValidateAssociationInviteQueryHandlerTests()
    {
        _handler = new ValidateAssociationInviteQueryHandler(
            _associationInviteRepository.Object,
            _tenantRepository.Object,
            _userRepository.Object);
    }

    [Fact]
    public async Task Handle_WhenExpiredInvite_ShouldReturnExpiredError()
    {
        _associationInviteRepository
            .Setup(x => x.GetByTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AssociationInviteData(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "invitee@club.com",
                "INVITEE@CLUB.COM",
                "hash",
                DateTime.UtcNow.AddMinutes(-1),
                DateTime.UtcNow.AddHours(-24),
                "owner-1",
                null,
                null,
                null));

        var result = await _handler.HandleAsync(new ValidateAssociationInviteQuery("token"));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("ASSOCIATION_INVITE_TOKEN_EXPIRED");
    }
}
