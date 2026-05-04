namespace BabaPlay.Application.DTOs;

public sealed record MatchSummaryFileResponse(
    string FileName,
    string ContentType,
    byte[] Content);
