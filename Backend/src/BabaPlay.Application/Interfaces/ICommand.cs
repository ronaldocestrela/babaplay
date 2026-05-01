using BabaPlay.Application.Common;

namespace BabaPlay.Application.Interfaces;

/// <summary>
/// Marker interface for commands that return a typed response.
/// Commands represent write operations (state mutations).
/// </summary>
public interface ICommand<out TResponse> { }

/// <summary>
/// Marker interface for commands that return no value.
/// </summary>
public interface ICommand : ICommand<Result> { }
