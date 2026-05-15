using BabaPlay.Application.Commands.Tenants;
using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.Tenants;

public class UpdateTenantSettingsCommandHandlerTests
{
    private readonly Mock<ITenantRepository> _tenantRepository = new();
    private readonly Mock<IUserTenantRepository> _userTenantRepository = new();
    private readonly Mock<ITenantLogoStorageService> _tenantLogoStorageService = new();
    private readonly UpdateTenantSettingsCommandHandler _handler;

    public UpdateTenantSettingsCommandHandlerTests()
    {
        _handler = new UpdateTenantSettingsCommandHandler(
            _tenantRepository.Object,
            _userTenantRepository.Object,
            _tenantLogoStorageService.Object);
    }

    [Fact]
    public async Task Handle_InvalidAssociationLatitude_ShouldReturnLatitudeInvalid()
    {
        // Arrange
        var cmd = CreateValidCommand() with { AssociationLatitude = 100 };
        _userTenantRepository
            .Setup(x => x.IsOwnerAsync(cmd.RequestedByUserId, cmd.TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.HandleAsync(cmd);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("TENANT_ASSOCIATION_LATITUDE_INVALID");
    }

    [Fact]
    public async Task Handle_InvalidAssociationLongitude_ShouldReturnLongitudeInvalid()
    {
        // Arrange
        var cmd = CreateValidCommand() with { AssociationLongitude = -181 };
        _userTenantRepository
            .Setup(x => x.IsOwnerAsync(cmd.RequestedByUserId, cmd.TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.HandleAsync(cmd);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("TENANT_ASSOCIATION_LONGITUDE_INVALID");
    }

    [Fact]
    public async Task Handle_ValidPayload_ShouldUpdateAndReturnCoordinates()
    {
        // Arrange
        var cmd = CreateValidCommand();

        _userTenantRepository
            .Setup(x => x.IsOwnerAsync(cmd.RequestedByUserId, cmd.TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _tenantRepository
            .Setup(x => x.UpdateAssociationSettingsAsync(
                cmd.TenantId,
                "Clube Atualizado",
                11,
                null,
                "Rua Atualizada",
                "321",
                "Centro",
                "Sao Paulo",
                "SP",
                "01000-000",
                -23.5505,
                -46.6333,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _tenantRepository
            .Setup(x => x.GetByIdAsync(cmd.TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TenantInfoDto(
                cmd.TenantId,
                "Clube Atualizado",
                "clube-atualizado",
                true,
                "conn",
                "Ready",
                11,
                "tenant-logos/abc/logo.png",
                "Rua Atualizada",
                "321",
                "Centro",
                "Sao Paulo",
                "SP",
                "01000-000",
                -23.5505,
                -46.6333));

        // Act
        var result = await _handler.HandleAsync(cmd);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.AssociationLatitude.Should().Be(-23.5505);
        result.Value.AssociationLongitude.Should().Be(-46.6333);
        _tenantRepository.Verify(x => x.UpdateAssociationSettingsAsync(
            cmd.TenantId,
            "Clube Atualizado",
            11,
            null,
            "Rua Atualizada",
            "321",
            "Centro",
            "Sao Paulo",
            "SP",
            "01000-000",
            -23.5505,
            -46.6333,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithLogo_ShouldPersistCloudUrlFromStorage()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var cmd = new UpdateTenantSettingsCommand(
            tenantId,
            "owner-user-id",
            "Clube Atualizado",
            11,
            new TenantLogoUploadRequest("logo.webp", "image/webp", [1, 2, 3]),
            "Rua Atualizada",
            "321",
            "Centro",
            "Sao Paulo",
            "SP",
            "01000-000",
            -23.5505,
            -46.6333);

        const string cloudUrl = "https://res.cloudinary.com/demo/image/upload/v1/tenant-logos/abc/new-logo.webp";

        _userTenantRepository
            .Setup(x => x.IsOwnerAsync(cmd.RequestedByUserId, cmd.TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _tenantLogoStorageService
            .Setup(x => x.SaveAsync(It.IsAny<TenantLogoSaveRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TenantLogoStoredFile(cloudUrl, "image/webp", 3));

        _tenantRepository
            .Setup(x => x.UpdateAssociationSettingsAsync(
                cmd.TenantId,
                "Clube Atualizado",
                11,
                cloudUrl,
                "Rua Atualizada",
                "321",
                "Centro",
                "Sao Paulo",
                "SP",
                "01000-000",
                -23.5505,
                -46.6333,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _tenantRepository
            .Setup(x => x.GetByIdAsync(cmd.TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TenantInfoDto(
                cmd.TenantId,
                "Clube Atualizado",
                "clube-atualizado",
                true,
                "conn",
                "Ready",
                11,
                cloudUrl,
                "Rua Atualizada",
                "321",
                "Centro",
                "Sao Paulo",
                "SP",
                "01000-000",
                -23.5505,
                -46.6333));

        // Act
        var result = await _handler.HandleAsync(cmd);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.LogoPath.Should().Be(cloudUrl);
        _tenantRepository.Verify(x => x.UpdateAssociationSettingsAsync(
            cmd.TenantId,
            "Clube Atualizado",
            11,
            cloudUrl,
            "Rua Atualizada",
            "321",
            "Centro",
            "Sao Paulo",
            "SP",
            "01000-000",
            -23.5505,
            -46.6333,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    private static UpdateTenantSettingsCommand CreateValidCommand()
    {
        return new UpdateTenantSettingsCommand(
            Guid.NewGuid(),
            "owner-user-id",
            "Clube Atualizado",
            11,
            null,
            "Rua Atualizada",
            "321",
            "Centro",
            "Sao Paulo",
            "SP",
            "01000-000",
            -23.5505,
            -46.6333);
    }
}
