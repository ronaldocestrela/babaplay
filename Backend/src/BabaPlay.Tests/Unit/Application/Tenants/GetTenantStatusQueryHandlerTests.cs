using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Queries.Tenants;
using BabaPlay.Domain.Exceptions;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.Tenants;

public class GetTenantStatusQueryHandlerTests
{
    private readonly Mock<ITenantRepository> _tenantRepo = new();
    private readonly GetTenantStatusQueryHandler _handler;

    public GetTenantStatusQueryHandlerTests()
    {
        _handler = new GetTenantStatusQueryHandler(_tenantRepo.Object);
    }

    [Fact]
    public async Task Handle_TenantNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var unknownId = Guid.NewGuid();
        _tenantRepo
            .Setup(r => r.GetByIdAsync(unknownId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TenantInfoDto?)null);

        // Act
        var act = () => _handler.HandleAsync(new GetTenantStatusQuery(unknownId));

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*{unknownId}*");
    }

    [Fact]
    public async Task Handle_TenantFound_ShouldReturnTenantResponse()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var dto = new TenantInfoDto(tenantId, "My Club", "myclob", true, "", "Ready");
        _tenantRepo
            .Setup(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        // Act
        var result = await _handler.HandleAsync(new GetTenantStatusQuery(tenantId));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be(tenantId);
        result.Value.Name.Should().Be("My Club");
        result.Value.Slug.Should().Be("myclob");
        result.Value.ProvisioningStatus.Should().Be("Ready");
    }

    [Fact]
    public async Task Handle_TenantFound_StatusStringMatchesDtoValue()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var dto = new TenantInfoDto(tenantId, "FC Test", "fctest", true, "conn", "InProgress");
        _tenantRepo
            .Setup(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        // Act
        var result = await _handler.HandleAsync(new GetTenantStatusQuery(tenantId));

        // Assert
        result.Value!.ProvisioningStatus.Should().Be("InProgress");
    }
}
