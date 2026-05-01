using BabaPlay.Application.Common;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Ping;

public record PingCommand(string Sender) : ICommand<Result<string>>;
