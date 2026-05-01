using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Queries.Ping;

public record PingQuery : IQuery<Result<PingStatusDto>>;
