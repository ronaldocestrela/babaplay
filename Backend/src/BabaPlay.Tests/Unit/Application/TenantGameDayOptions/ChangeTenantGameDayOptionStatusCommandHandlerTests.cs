using BabaPlay.Application.Commands.TenantGameDayOptions;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.TenantGameDayOptions;

public class ChangeTenantGameDayOptionStatusCommandHandlerTests
{
    private readonly Mock<ITenantGameDayOptionRepository> _repo = new();
    private readonly Mock<IUserTenantRepository> _userTenantRepository = new();
    private readonly ChangeTenantGameDayOptionStatusCommandHandler _handler;

    public ChangeTenantGameDayOptionStatusCommandHandlerTests()
    {
        _handler = new ChangeTenantGameDayOptionStatusCommandHandler(_repo.Object, _userTenantRepository.Object);
    }

    [Fact]
    public async Task Handle_NotOwner_ShouldReturnForbidden()
    {
        var cmd = new ChangeTenantGameDayOptionStatusCommand(Guid.NewGuid(), Guid.NewGuid(), "member-user", true);
        _userTenantRepository
            .Setup(x => x.IsOwnerAsync(cmd.RequestedByUserId, cmd.TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.HandleAsync(cmd);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("FORBIDDEN");
    }

    [Fact]
    public async Task Handle_NotFound_ShouldReturnNotFound()
    {
        var cmd = new ChangeTenantGameDayOptionStatusCommand(Guid.NewGuid(), Guid.NewGuid(), "owner-user", false);
        _userTenantRepository
            .Setup(x => x.IsOwnerAsync(cmd.RequestedByUserId, cmd.TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _repo
            .Setup(x => x.GetByIdAsync(cmd.OptionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TenantGameDayOption?)null);

        var result = await _handler.HandleAsync(cmd);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("TENANT_GAMEDAY_OPTION_NOT_FOUND");
    }

    [Fact]
    public async Task Handle_Activate_WhenDuplicateExists_ShouldReturnConflict()
    {
        var option = TenantGameDayOption.Create(Guid.NewGuid(), DayOfWeek.Friday, new TimeOnly(20, 0));
        option.Deactivate();

        var cmd = new ChangeTenantGameDayOptionStatusCommand(option.TenantId, option.Id, "owner-user", true);

        _userTenantRepository
            .Setup(x => x.IsOwnerAsync(cmd.RequestedByUserId, cmd.TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _repo
            .Setup(x => x.GetByIdAsync(cmd.OptionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(option);
        _repo
            .Setup(x => x.ExistsActiveBySlotAsync(option.TenantId, option.DayOfWeek, option.LocalStartTime, option.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.HandleAsync(cmd);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("TENANT_GAMEDAY_OPTION_ALREADY_EXISTS");
    }

    [Fact]
    public async Task Handle_Deactivate_ShouldUpdateStatus()
    {
        var option = TenantGameDayOption.Create(Guid.NewGuid(), DayOfWeek.Monday, new TimeOnly(21, 0));
        var cmd = new ChangeTenantGameDayOptionStatusCommand(option.TenantId, option.Id, "owner-user", false);

        _userTenantRepository
            .Setup(x => x.IsOwnerAsync(cmd.RequestedByUserId, cmd.TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _repo
            .Setup(x => x.GetByIdAsync(cmd.OptionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(option);

        var result = await _handler.HandleAsync(cmd);

        result.IsSuccess.Should().BeTrue();
        result.Value!.IsActive.Should().BeFalse();
        _repo.Verify(x => x.UpdateAsync(option, It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
