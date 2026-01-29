#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using CoubDownloader.Domain.Enums;
using CoubDownloader.Domain.Models;

namespace CoubDownloader.Infrastructure.VideoEditor;

/// <summary>
/// Translates ordered <see cref="VideoEffect"/> descriptors into FFmpeg filter-graph
/// expressions ready for use with the <c>-vf</c> argument.
/// </summary>
/// <remarks>
/// All intensity values are normalised to [0.0, 1.0] before being mapped to the
/// numeric ranges that each FFmpeg filter expects. 0.5 maps to the neutral/default
/// value of the underlying filter parameter in every case.
/// </remarks>
public static class VideoEffectsProcessor
{
    /// <summary>
    /// Builds a complete <c>-vf</c> filter chain string from an ordered collection of effects.
    /// Effects are chained with commas in the order they appear in <paramref name="effects"/>.
    /// </summary>
    /// <param name="effects">
    /// Ordered effect descriptors to encode; must not be <c>null</c>.
    /// An empty collection returns the FFmpeg no-op filter <c>"null"</c>.
    /// </param>
    /// <returns>
    /// A comma-separated FFmpeg filter string, e.g.
    /// <c>"eq=brightness=0.200,gblur=sigma=3.8,vflip"</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="effects"/> is <c>null</c>.</exception>
    /// <exception cref="NotSupportedException">
    /// Thrown when an effect carries a <see cref="VideoEffectType"/> value that has no
    /// registered filter mapping.
    /// </exception>
    public static string BuildFilterGraph(IReadOnlyList<VideoEffect> effects)
    {
        ArgumentNullException.ThrowIfNull(effects);

        if (effects.Count == 0)
            return "null";

        var filters = effects
            .Select(BuildSingleFilter)
            .Where(f => !string.IsNullOrWhiteSpace(f))
            .ToList();

        return filters.Count > 0 ? string.Join(",", filters) : "null";
    }

    /// <summary>
    /// Converts a single <see cref="VideoEffect"/> into its corresponding FFmpeg filter expression.
    /// </summary>
    /// <param name="effect">Effect to convert; must not be <c>null</c>.</param>
    /// <returns>FFmpeg filter expression string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="effect"/> is <c>null</c>.</exception>
    /// <exception cref="NotSupportedException">
    /// Thrown when <paramref name="effect"/>.<see cref="VideoEffect.Type"/> has no registered mapping.
    /// </exception>
    public static string BuildSingleFilter(VideoEffect effect)
    {
        ArgumentNullException.ThrowIfNull(effect);

        return effect.Type switch
        {
            VideoEffectType.Brightness     => BuildBrightnessFilter(effect.Intensity),
            VideoEffectType.Contrast       => BuildContrastFilter(effect.Intensity),
            VideoEffectType.Saturation     => BuildSaturationFilter(effect.Intensity),
            VideoEffectType.GaussianBlur   => BuildBlurFilter(effect.Intensity),
            VideoEffectType.Sharpen        => BuildSharpenFilter(effect.Intensity),
            VideoEffectType.Grayscale      => "hue=s=0",
            VideoEffectType.Sepia          => BuildSepiaFilter(),
            VideoEffectType.Vignette       => BuildVignetteFilter(effect.Intensity),
            VideoEffectType.NoiseReduction => BuildNoiseReductionFilter(effect.Intensity),
            VideoEffectType.FilmGrain      => BuildFilmGrainFilter(effect.Intensity),
            VideoEffectType.FlipHorizontal => "hflip",
            VideoEffectType.FlipVertical   => "vflip",
            _ => throw new NotSupportedException(
                $"Effect type '{effect.Type}' has no registered FFmpeg filter mapping.")
        };
    }

    // -------------------------------------------------------------------------
    // Individual filter builders
    // -------------------------------------------------------------------------

    private static string BuildBrightnessFilter(double intensity)
    {
        // Normalise 0–1 → brightness offset −0.5 … +0.5 (FFmpeg eq filter)
        var brightness = (intensity - 0.5) * 1.0;
        return $"eq=brightness={brightness:F3}";
    }

    private static string BuildContrastFilter(double intensity)
    {
        // 0 → 0.5× contrast; 0.5 → 1.0 (neutral); 1 → 2.5× (FFmpeg eq filter range 0–2)
        var contrast = 0.5 + intensity * 1.5;
        return $"eq=contrast={contrast:F3}";
    }

    private static string BuildSaturationFilter(double intensity)
    {
        // 0 → greyscale (0.0); 0.5 → neutral (1.0); 1 → vivid (3.0)
        var saturation = intensity * 3.0;
        return $"eq=saturation={saturation:F3}";
    }

    private static string BuildBlurFilter(double intensity)
    {
        // Gaussian sigma: 0.5 (near-zero) at intensity=0; 20 at intensity=1
        var sigma = 0.5 + intensity * 19.5;
        return $"gblur=sigma={sigma:F1}";
    }

    private static string BuildSharpenFilter(double intensity)
    {
        // luma_amount 0–5; 0 = no sharpening, 1 = moderate, 5 = aggressive
        var amount = intensity * 5.0;
        return $"unsharp=luma_msize_x=5:luma_msize_y=5:luma_amount={amount:F2}";
    }

    private static string BuildSepiaFilter()
    {
        // Classic sepia matrix via colorchannelmixer (Rec. 601 luminance coefficients)
        return "colorchannelmixer=.393:.769:.189:0:.349:.686:.168:0:.272:.534:.131";
    }

    private static string BuildVignetteFilter(double intensity)
    {
        // angle controls radial fall-off; PI/5 at full intensity produces a visible but not crushing vignette
        var angle = Math.PI / 5.0 * intensity;
        return $"vignette=angle={angle:F4}";
    }

    private static string BuildNoiseReductionFilter(double intensity)
    {
        // hqdn3d spatial + temporal denoising; luma strength 0–10, chroma slightly softer
        var luma   = intensity * 10.0;
        var chroma = luma * 0.6;
        return $"hqdn3d={luma:F1}:{chroma:F1}:{luma:F1}:{chroma:F1}";
    }

    private static string BuildFilmGrainFilter(double intensity)
    {
        // Additive temporal+uniform noise simulating analogue film grain
        var strength = (int)(intensity * 80.0);
        return $"noise=alls={strength}:allf=t+u";
    }
}
