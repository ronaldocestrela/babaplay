using BabaPlay.Application.Commands.TenantGameDayOptions;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.TenantGameDayOptions;

public class CreateTenantGameDayOptionCommandHandlerTests
{
    private readonly Mock<ITenantGameDayOptionRepository> _repo = new();
    private readonly Mock<IUserTenantRepository> _userTenantRepository = new();
    private readonly CreateTenantGameDayOptionCommandHandler _handler;

    public CreateTenantGameDayOptionCommandHandlerTests()
    {
        _handler = new CreateTenantGameDayOptionCommandHandler(_repo.Object, _userTenantRepository.Object);
    }

    [Fact]
    public async Task Handle_UserIsNotOwner_ShouldReturnForbidden()
    {
        var cmd = new CreateTenantGameDayOptionCommand(Guid.NewGuid(), "user-1", DayOfWeek.Tuesday, new TimeOnly(20, 0));
        _userTenantRepository
            .Setup(x => x.IsOwnerAsync(cmd.RequestedByUserId, cmd.TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.HandleAsync(cmd);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("FORBIDDEN");
    }

    [Fact]
    public async Task Handle_DuplicateActiveSlot_ShouldReturnConflict()
    {
        var cmd = new CreateTenantGameDayOptionCommand(Guid.NewGuid(), "owner-1", DayOfWeek.Tuesday, new TimeOnly(20, 0));
        _userTenantRepository
            .Setup(x => x.IsOwnerAsync(cmd.RequestedByUserId, cmd.TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _repo
            .Setup(x => x.ExistsActiveBySlotAsync(cmd.TenantId, cmd.DayOfWeek, cmd.LocalStartTime, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.HandleAsync(cmd);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("TENANT_GAMEDAY_OPTION_ALREADY_EXISTS");
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateOption()
    {
        var cmd = new CreateTenantGameDayOptionCommand(Guid.NewGuid(), "owner-1", DayOfWeek.Thursday, new TimeOnly(19, 30));
        _userTenantRepository
            .Setup(x => x.IsOwnerAsync(cmd.RequestedByUserId, cmd.TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _repo
            .Setup(x => x.ExistsActiveBySlotAsync(cmd.TenantId, cmd.DayOfWeek, cmd.LocalStartTime, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.HandleAsync(cmd);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.TenantId.Should().Be(cmd.TenantId);
        result.Value.DayOfWeek.Should().Be(DayOfWeek.Thursday);
        result.Value.LocalStartTime.Should().Be(new TimeOnly(19, 30));

        _repo.Verify(x => x.AddAsync(It.IsAny<TenantGameDayOption>(), It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
