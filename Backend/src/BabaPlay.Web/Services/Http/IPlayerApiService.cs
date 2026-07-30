using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Forms;
using BabaPlay.Web.Models;

namespace BabaPlay.Web.Services.Http;

public interface IPlayerApiService
{
    Task<PlayerProfileResponseDto?> CompleteProfileAsync(CompleteProfileDto dto, IBrowserFile? photoFile, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PositionDto>> GetPositionsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PlayerDto>> GetPlayersAsync(CancellationToken cancellationToken = default);
    Task<PlayerDto?> GetPlayerByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> UpdatePlayerAdminAsync(Guid id, UpdatePlayerAdminDto dto, CancellationToken cancellationToken = default);
    Task<bool> UpdatePlayerRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RoleDto>> GetRolesAsync(CancellationToken cancellationToken = default);
    Task<bool> DeletePlayerAsync(Guid id, CancellationToken cancellationToken = default);
}

