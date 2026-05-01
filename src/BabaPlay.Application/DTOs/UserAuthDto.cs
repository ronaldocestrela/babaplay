namespace BabaPlay.Application.DTOs;

/// <summary>Lightweight user projection used only for authentication decisions. No Identity dependency in Application.</summary>
public record UserAuthDto(string Id, string Email, bool IsActive);
