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
}
