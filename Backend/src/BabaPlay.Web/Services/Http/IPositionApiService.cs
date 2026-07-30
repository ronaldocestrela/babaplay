using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BabaPlay.Web.Models;

namespace BabaPlay.Web.Services.Http;

public interface IPositionApiService
{
    Task<IReadOnlyList<PositionDto>> GetPositionsAsync(CancellationToken cancellationToken = default);
    Task<PositionDto?> GetPositionByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(bool Success, string? ErrorMessage)> CreatePositionAsync(CreatePositionDto dto, CancellationToken cancellationToken = default);
    Task<(bool Success, string? ErrorMessage)> UpdatePositionAsync(Guid id, UpdatePositionDto dto, CancellationToken cancellationToken = default);
    Task<(bool Success, string? ErrorMessage)> DeletePositionAsync(Guid id, CancellationToken cancellationToken = default);
}
