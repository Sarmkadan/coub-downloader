#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace CoubDownloader.Domain.Enums;

/// <summary>
/// Visual and audio effects that can be applied to a video clip.
/// </summary>
public enum VideoEffectType
{
    /// <summary>Increase or decrease image luminance</summary>
    Brightness,

    /// <summary>Adjust tonal range between light and dark areas</summary>
    Contrast,

    /// <summary>Modify colour intensity</summary>
    Saturation,

    /// <summary>Gaussian spatial blur</summary>
    GaussianBlur,

    /// <summary>Unsharp-mask sharpening</summary>
    Sharpen,

    /// <summary>Convert to greyscale using the luma channel</summary>
    Grayscale,

    /// <summary>Warm antique sepia tone via colour channel mixing</summary>
    Sepia,

    /// <summary>Darkened radial gradient toward frame edges</summary>
    Vignette,

    /// <summary>Reduce digital or compression artefacts</summary>
    NoiseReduction,

    /// <summary>Film-grain noise overlay for analogue look</summary>
    FilmGrain,

    /// <summary>Horizontal mirror around the vertical axis</summary>
    FlipHorizontal,

    /// <summary>Vertical mirror around the horizontal axis</summary>
    FlipVertical
}

/// <summary>
/// Controls how trim boundaries are aligned to the encoded video stream.
/// </summary>
public enum TrimMode
{
    /// <summary>Seek to the exact requested timestamp; may cause I-frame decode overhead</summary>
    Precise,

    /// <summary>Snap the cut to the nearest preceding keyframe for fast, lossless segment extraction</summary>
    KeyframeAligned,

    /// <summary>Detect silence or scene changes to find a natural cut point near the requested time</summary>
    SmartDetect
}

/// <summary>
/// Strategy for joining clips during a merge operation.
/// </summary>
public enum MergeStrategy
{
    /// <summary>Direct frame-level cut with no transition; stream-copied where codecs match</summary>
    HardCut,

    /// <summary>Sequential copy without re-encoding when all clips share the same codec parameters</summary>
    Sequential,

    /// <summary>Audio and video crossfade over the configured transition duration</summary>
    Crossfade,

    /// <summary>Linear fade-to-black then fade-in between each pair of clips</summary>
    FadeThrough
}

/// <summary>
/// Preview render fidelity preset controlling the trade-off between speed and quality.
/// </summary>
public enum PreviewQuality
{
    /// <summary>Fast low-resolution render suitable for real-time scrubbing</summary>
    Draft,

    /// <summary>Half-resolution balanced preview for review purposes</summary>
    Standard,

    /// <summary>Full-resolution preview indistinguishable from the final output</summary>
    HighFidelity
}
