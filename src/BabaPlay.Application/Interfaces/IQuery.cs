namespace BabaPlay.Application.Interfaces;

/// <summary>
/// Marker interface for queries that return a typed response.
/// Queries represent read operations and must never mutate state.
/// </summary>
public interface IQuery<out TResponse> { }
