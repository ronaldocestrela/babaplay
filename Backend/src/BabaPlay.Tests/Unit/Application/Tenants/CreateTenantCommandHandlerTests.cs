using BabaPlay.Application.Commands.Tenants;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.Tenants;

public class CreateTenantCommandHandlerTests
{
    private readonly Mock<ITenantRepository> _tenantRepo = new();
    private readonly Mock<ITenantProvisioningQueue> _queue = new();
    private readonly CreateTenantCommandHandler _handler;

    public CreateTenantCommandHandlerTests()
    {
        _handler = new CreateTenantCommandHandler(_tenantRepo.Object, _queue.Object);
    }

    [Fact]
    public async Task Handle_EmptyName_ShouldReturnTenantNameRequired()
    {
        // Act
        var result = await _handler.HandleAsync(new CreateTenantCommand("", "slug"));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("TENANT_NAME_REQUIRED");
        _tenantRepo.Verify(r => r.AddAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_EmptySlug_ShouldReturnTenantSlugRequired()
    {
        // Act
        var result = await _handler.HandleAsync(new CreateTenantCommand("My Club", ""));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("TENANT_SLUG_REQUIRED");
        _tenantRepo.Verify(r => r.AddAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_DuplicateSlug_ShouldReturnTenantSlugTaken()
    {
        // Arrange
        _tenantRepo
            .Setup(r => r.ExistsAsync("myclob", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.HandleAsync(new CreateTenantCommand("My Club", "myclob"));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("TENANT_SLUG_TAKEN");
        _tenantRepo.Verify(r => r.AddAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateTenantAndEnqueueProvisioning()
    {
        // Arrange
        _tenantRepo
            .Setup(r => r.ExistsAsync("myclob", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _tenantRepo
            .Setup(r => r.AddAsync(It.IsAny<Guid>(), "My Club", "myclob", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _queue
            .Setup(q => q.EnqueueAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.HandleAsync(new CreateTenantCommand("My Club", "myclob"));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Name.Should().Be("My Club");
        result.Value.Slug.Should().Be("myclob");
        result.Value.ProvisioningStatus.Should().Be("Pending");
        _tenantRepo.Verify(r => r.AddAsync(It.IsAny<Guid>(), "My Club", "myclob", It.IsAny<CancellationToken>()), Times.Once);
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
        var result = await _handler.HandleAsync(new CreateTenantCommand("My Club", "MyClob"));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Slug.Should().Be("myclob");
        _tenantRepo.Verify(r => r.AddAsync(It.IsAny<Guid>(), It.IsAny<string>(), "myclob", It.IsAny<CancellationToken>()), Times.Once);
    }
}
