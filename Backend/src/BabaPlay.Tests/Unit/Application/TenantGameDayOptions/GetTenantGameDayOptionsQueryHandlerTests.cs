using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Queries.TenantGameDayOptions;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.TenantGameDayOptions;

public class GetTenantGameDayOptionsQueryHandlerTests
{
    private readonly Mock<ITenantGameDayOptionRepository> _repo = new();
    private readonly GetTenantGameDayOptionsQueryHandler _handler;

    public GetTenantGameDayOptionsQueryHandlerTests()
    {
        _handler = new GetTenantGameDayOptionsQueryHandler(_repo.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnMappedList()
    {
        var tenantId = Guid.NewGuid();
        var options = new List<TenantGameDayOption>
        {
            TenantGameDayOption.Create(tenantId, DayOfWeek.Wednesday, new TimeOnly(20, 0)),
            TenantGameDayOption.Create(tenantId, DayOfWeek.Saturday, new TimeOnly(9, 0)),
        };

        _repo
            .Setup(x => x.GetByTenantAsync(tenantId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(options);

        var result = await _handler.HandleAsync(new GetTenantGameDayOptionsQuery(tenantId, null));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value![0].TenantId.Should().Be(tenantId);
        result.Value[0].DayOfWeek.Should().Be(DayOfWeek.Wednesday);
    }
}
