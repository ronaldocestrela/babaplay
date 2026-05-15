using BabaPlay.Application.Commands.Tenants;
using BabaPlay.Application.Common;
using BabaPlay.Application.Interfaces;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.Tenants;

public class CreateTenantCommandHandlerTests
{
    private readonly Mock<ITenantRepository> _tenantRepo = new();
    private readonly Mock<ITenantProvisioningQueue> _queue = new();
    private readonly Mock<ITenantOwnerProvisioningService> _ownerProvisioning = new();
    private readonly Mock<ITenantLogoStorageService> _tenantLogoStorage = new();
    private readonly CreateTenantCommandHandler _handler;

    private static readonly TenantLogoUploadRequest ValidLogo =
        new("logo.png", "image/png", [1, 2, 3]);

    private static CreateTenantCommand CreateValidCommand(
        string name = "My Club",
        string slug = "myclob") => new(
            name,
            slug,
            "owner@test.com",
            "Password@123",
            null,
            ValidLogo,
            "Rua Central",
            "123",
            "Centro",
            "Sao Paulo",
            "SP",
            "01000-000",
            -23.5505,
            -46.6333);

    public CreateTenantCommandHandlerTests()
    {
        _handler = new CreateTenantCommandHandler(
            _tenantRepo.Object,
            _queue.Object,
            _ownerProvisioning.Object,
            _tenantLogoStorage.Object);

        _ownerProvisioning
            .Setup(x => x.ResolveOwnerUserIdAsync(
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Ok("owner-user-id"));

        _ownerProvisioning
            .Setup(x => x.EnsureOwnerMembershipAsync(
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        _tenantLogoStorage
            .Setup(x => x.SaveAsync(It.IsAny<TenantLogoSaveRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TenantLogoStoredFile("tenant-logos/abc/logo.png", "image/png", 3));
    }

    [Fact]
    public async Task Handle_EmptyName_ShouldReturnTenantNameRequired()
    {
        // Act
        var result = await _handler.HandleAsync(CreateValidCommand(name: ""));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("TENANT_NAME_REQUIRED");
        _tenantRepo.Verify(r => r.AddAsync(
            It.IsAny<Guid>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string?>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<double>(),
            It.IsAny<double>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_EmptySlug_ShouldReturnTenantSlugRequired()
    {
        // Act
        var result = await _handler.HandleAsync(CreateValidCommand(slug: ""));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("TENANT_SLUG_REQUIRED");
        _tenantRepo.Verify(r => r.AddAsync(
            It.IsAny<Guid>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string?>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<double>(),
            It.IsAny<double>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_MissingLogo_ShouldReturnTenantLogoRequired()
    {
        // Act
        var result = await _handler.HandleAsync(new CreateTenantCommand(
            "My Club",
            "myclob",
            "owner@test.com",
            "Password@123",
            null,
            null,
            "Rua Central",
            "123",
            "Centro",
            "Sao Paulo",
            "SP",
            "01000-000",
            -23.5505,
            -46.6333));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("TENANT_LOGO_REQUIRED");
        _tenantLogoStorage.Verify(x => x.SaveAsync(It.IsAny<TenantLogoSaveRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_DuplicateSlug_ShouldReturnTenantSlugTaken()
    {
        // Arrange
        _tenantRepo
            .Setup(r => r.ExistsAsync("myclob", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.HandleAsync(CreateValidCommand());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("TENANT_SLUG_TAKEN");
        _tenantRepo.Verify(r => r.AddAsync(
            It.IsAny<Guid>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string?>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<double>(),
            It.IsAny<double>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateTenantAndEnqueueProvisioning()
    {
        // Arrange
        _tenantRepo
            .Setup(r => r.ExistsAsync("myclob", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _tenantRepo
            .Setup(r => r.AddAsync(
                It.IsAny<Guid>(),
                "My Club",
                "myclob",
                It.IsAny<string>(),
                "Rua Central",
                "123",
                "Centro",
                "Sao Paulo",
                "SP",
                "01000-000",
                -23.5505,
                -46.6333,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _queue
            .Setup(q => q.EnqueueAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.HandleAsync(CreateValidCommand());

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Name.Should().Be("My Club");
        result.Value.Slug.Should().Be("myclob");
        result.Value.ProvisioningStatus.Should().Be("Pending");
        result.Value.LogoPath.Should().NotBeNullOrWhiteSpace();
        result.Value.Street.Should().Be("Rua Central");
        result.Value.City.Should().Be("Sao Paulo");
        result.Value.AssociationLatitude.Should().Be(-23.5505);
        result.Value.AssociationLongitude.Should().Be(-46.6333);
        _tenantRepo.Verify(r => r.AddAsync(
            It.IsAny<Guid>(),
            "My Club",
            "myclob",
            It.IsAny<string>(),
            "Rua Central",
            "123",
            "Centro",
            "Sao Paulo",
            "SP",
            "01000-000",
            -23.5505,
            -46.6333,
            It.IsAny<CancellationToken>()), Times.Once);
        _tenantLogoStorage.Verify(x => x.SaveAsync(It.IsAny<TenantLogoSaveRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        _queue.Verify(q => q.EnqueueAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_SlugWithUppercase_ShouldNormaliseToLowercase()
    {
        // Arrange
        _tenantRepo
            .Setup(r => r.ExistsAsync("myclob", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.HandleAsync(CreateValidCommand(slug: "MyClob"));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Slug.Should().Be("myclob");
        _tenantRepo.Verify(r => r.AddAsync(
            It.IsAny<Guid>(),
            It.IsAny<string>(),
            "myclob",
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string?>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<double>(),
            It.IsAny<double>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_AnonymousWithoutAdminCredentials_ShouldReturnTenantAdminCredentialsRequired()
    {
        // Act
        var result = await _handler.HandleAsync(new CreateTenantCommand(
            "My Club",
            "myclob",
            null,
            null,
            null,
            ValidLogo,
            "Rua Central",
            "123",
            "Centro",
            "Sao Paulo",
            "SP",
            "01000-000",
            -23.5505,
            -46.6333));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("TENANT_ADMIN_CREDENTIALS_REQUIRED");
        _tenantRepo.Verify(r => r.AddAsync(
            It.IsAny<Guid>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string?>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<double>(),
            It.IsAny<double>(),
            It.IsAny<CancellationToken>()), Times.Never);
        _queue.Verify(q => q.EnqueueAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _tenantLogoStorage.Verify(x => x.SaveAsync(It.IsAny<TenantLogoSaveRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        _ownerProvisioning.Verify(x => x.ResolveOwnerUserIdAsync(
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()), Times.Never);
        _ownerProvisioning.Verify(x => x.EnsureOwnerMembershipAsync(
            It.IsAny<string>(),
            It.IsAny<Guid>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_InvalidAssociationLatitude_ShouldReturnLatitudeInvalid()
    {
        // Act
        var result = await _handler.HandleAsync(CreateValidCommand() with { AssociationLatitude = 91 });

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("TENANT_ASSOCIATION_LATITUDE_INVALID");
    }

    [Fact]
    public async Task Handle_InvalidAssociationLongitude_ShouldReturnLongitudeInvalid()
    {
        // Act
        var result = await _handler.HandleAsync(CreateValidCommand() with { AssociationLongitude = -181 });

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("TENANT_ASSOCIATION_LONGITUDE_INVALID");
    }

    [Fact]
    public async Task Handle_StorageReturnsCloudUrl_ShouldPersistAndReturnCloudUrl()
    {
        // Arrange
        const string cloudUrl = "https://res.cloudinary.com/demo/image/upload/v1/tenant-logos/abc/logo.png";

        _tenantRepo
            .Setup(r => r.ExistsAsync("myclob", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _tenantLogoStorage
            .Setup(x => x.SaveAsync(It.IsAny<TenantLogoSaveRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TenantLogoStoredFile(cloudUrl, "image/png", 3));

        _tenantRepo
            .Setup(r => r.AddAsync(
                It.IsAny<Guid>(),
                "My Club",
                "myclob",
                cloudUrl,
                "Rua Central",
                "123",
                "Centro",
                "Sao Paulo",
                "SP",
                "01000-000",
                -23.5505,
                -46.6333,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _queue
            .Setup(q => q.EnqueueAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.HandleAsync(CreateValidCommand());

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.LogoPath.Should().Be(cloudUrl);
        _tenantRepo.Verify(r => r.AddAsync(
            It.IsAny<Guid>(),
            "My Club",
            "myclob",
            cloudUrl,
            "Rua Central",
            "123",
            "Centro",
            "Sao Paulo",
            "SP",
            "01000-000",
            -23.5505,
            -46.6333,
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
