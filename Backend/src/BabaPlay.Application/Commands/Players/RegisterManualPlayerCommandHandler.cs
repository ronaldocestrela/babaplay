using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;

namespace BabaPlay.Application.Commands.Players;

public sealed class RegisterManualPlayerCommandHandler
    : ICommandHandler<RegisterManualPlayerCommand, Result<PlayerResponse>>
{
    private readonly IUserInvitationAccountService _userInvitationAccountService;
    private readonly IUserTenantMembershipService _userTenantMembershipService;
    private readonly IPlayerRepository _playerRepository;
    private readonly ITenantContext _tenantContext;

    public RegisterManualPlayerCommandHandler(
        IUserInvitationAccountService userInvitationAccountService,
        IUserTenantMembershipService userTenantMembershipService,
        IPlayerRepository playerRepository,
        ITenantContext tenantContext)
    {
        _userInvitationAccountService = userInvitationAccountService;
        _userTenantMembershipService = userTenantMembershipService;
        _playerRepository = playerRepository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<PlayerResponse>> HandleAsync(RegisterManualPlayerCommand command, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(command.Email))
            return Result<PlayerResponse>.Fail("MANUAL_PLAYER_EMAIL_REQUIRED", "Player email is required.");

        var normalizedEmail = command.Email.Trim().ToLowerInvariant();
        if (!normalizedEmail.Contains('@'))
            return Result<PlayerResponse>.Fail("MANUAL_PLAYER_EMAIL_INVALID", "Player email is invalid.");

        if (string.IsNullOrWhiteSpace(command.Password))
            return Result<PlayerResponse>.Fail("MANUAL_PLAYER_PASSWORD_REQUIRED", "Temporary password is required.");

        if (string.IsNullOrWhiteSpace(command.Name))
            return Result<PlayerResponse>.Fail("INVALID_NAME", "Player name is required.");

        var createUserResult = await _userInvitationAccountService.CreateUserAsync(normalizedEmail, command.Password.Trim(), ct);
        if (!createUserResult.IsSuccess)
            return Result<PlayerResponse>.Fail(createUserResult.ErrorCode!, createUserResult.ErrorMessage!);

        var userIdText = createUserResult.Value!;
        if (!Guid.TryParse(userIdText, out var userId))
            return Result<PlayerResponse>.Fail("MANUAL_PLAYER_USER_ID_INVALID", "Created user id is invalid.");

        await _userTenantMembershipService.EnsureMemberAsync(userIdText, _tenantContext.TenantId, ct);

        if (await _playerRepository.ExistsByUserIdAsync(userId, ct))
            return Result<PlayerResponse>.Fail("PLAYER_ALREADY_EXISTS", $"A player for user '{userId}' already exists in this tenant.");

        var player = Player.Create(
            _tenantContext.TenantId,
            userId,
            command.Name,
            command.Nickname,
            command.Phone,
            command.DateOfBirth);

        await _playerRepository.AddAsync(player, ct);
        await _playerRepository.SaveChangesAsync(ct);

        return Result<PlayerResponse>.Ok(new PlayerResponse(
            player.Id,
            player.UserId,
            player.Name,
            player.Nickname,
            player.Phone,
            player.DateOfBirth,
            player.PositionIds.ToList(),
            player.IsActive,
            player.CreatedAt));
    }
}
