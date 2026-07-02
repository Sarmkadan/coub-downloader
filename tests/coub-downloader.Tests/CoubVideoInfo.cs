#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace CoubDownloader.Tests;

public record CoubVideoInfo
{
    public string Id { get; init; } = "";
    public string Title { get; init; } = "";
    public double Duration { get; init; }
    public bool HasAudio { get; init; }
    public string? ChannelUrl { get; init; }
    public long ViewCount { get; init; }
};
