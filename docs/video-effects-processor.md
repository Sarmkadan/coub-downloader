# VideoEffectsProcessor

`VideoEffectsProcessor` is a static class in `Infrastructure/VideoEditor/VideoEffectsProcessor.cs`. It translates an ordered list of `VideoEffect` descriptors into an FFmpeg filter-graph string for use with the `-vf` argument. It performs no I/O and runs no processes. `VideoEditorService.ApplyEffectsAsync` is its only caller in the codebase; that method passes the result to FFmpeg.

Every mapping is a pure function of `VideoEffect.Type` and `VideoEffect.Intensity`.

## Public API

### `BuildFilterGraph`

```csharp
public static string BuildFilterGraph(IReadOnlyList<VideoEffect> effects)
```

Builds the complete `-vf` value. Each effect is converted with `BuildSingleFilter` and the results are joined with `,` in input order.

| Input | Result |
|---|---|
| Empty list | `"null"` (FFmpeg no-op filter) |
| Non-empty list | Comma-joined filter expressions, in input order |

Exceptions:

- `ArgumentNullException`: `effects` is `null`, or an element of the list is `null`.
- `NotSupportedException`: an effect has a `VideoEffectType` value with no mapping.

Example:

```csharp
var graph = VideoEffectsProcessor.BuildFilterGraph(new[]
{
    new VideoEffect { Type = VideoEffectType.Brightness, Intensity = 0.7 },
    new VideoEffect { Type = VideoEffectType.FlipVertical }
});
// "eq=brightness=0.200,vflip"
```

### `BuildSingleFilter`

```csharp
public static string BuildSingleFilter(VideoEffect effect)
```

Converts one `VideoEffect` into one FFmpeg filter expression. It throws `ArgumentNullException` if `effect` is `null`, and `NotSupportedException` if the type has no mapping. The method does not validate `Intensity`.

## Supported effects

`Intensity` is the normalised value in `[0.0, 1.0]` from `VideoEffect`. The table gives the FFmpeg output for the endpoints and the midpoint. Values between these follow a linear mapping.

| `VideoEffectType` | FFmpeg output | `Intensity` 0.0 | 0.5 | 1.0 |
|---|---|---|---|---|
| `Brightness` | `eq=brightness=` | `-0.500` | `0.000` | `0.500` |
| `Contrast` | `eq=contrast=` | `0.500` | `1.250` | `2.000` |
| `Saturation` | `eq=saturation=` | `0.000` | `1.500` | `3.000` |
| `GaussianBlur` | `gblur=sigma=` | `0.5` | `10.2` | `20.0` |
| `Sharpen` | `unsharp=luma_msize_x=5:luma_msize_y=5:luma_amount=` | `0.00` | `2.50` | `5.00` |
| `Grayscale` | `hue=s=0` | Ignored | Ignored | Ignored |
| `Sepia` | `colorchannelmixer=...` (fixed matrix) | Ignored | Ignored | Ignored |
| `Vignette` | `vignette=angle=` | `0.0000` | `0.3142` | `0.6283` |
| `NoiseReduction` | `hqdn3d=L:C:L:C` | `0.0` / `0.0` | `5.0` / `3.0` | `10.0` / `6.0` |
| `FilmGrain` | `noise=alls=N:allf=t+u` | `0` | `40` | `80` |
| `FlipHorizontal` | `hflip` | Ignored | Ignored | Ignored |
| `FlipVertical` | `vflip` | Ignored | Ignored | Ignored |

For `NoiseReduction`, `L` is the luma strength, which is `Intensity × 10`. `C` is the chroma strength, which is `L × 0.6`. The filter arguments are `luma_spatial:chroma_spatial:luma_temporal:chroma_temporal`, and the two luma and two chroma values are equal.

### Effect details

- **Brightness**: `eq=brightness=(Intensity − 0.5)`. The offset is in `[−0.5, +0.5]`. `0.5` is the neutral value.
- **Contrast**: `eq=contrast=0.5 + Intensity × 1.5`. The range is `[0.5, 2.0]`. The neutral value of 1.0 is reached at `Intensity ≈ 0.333`, not at 0.5.
- **Saturation**: `eq=saturation=Intensity × 3.0`. `0` is greyscale, `1.0` is unchanged (at `Intensity ≈ 0.333`), and `3.0` is the maximum.
- **GaussianBlur**: `gblur=sigma=0.5 + Intensity × 19.5`. The range is `[0.5, 20.0]`.
- **Sharpen**: `unsharp` with a fixed 5×5 luma matrix and `luma_amount = Intensity × 5.0`. The range is `[0, 5]`. Here `0` is a no-op.
- **Grayscale**: `hue=s=0` removes chroma. It takes no intensity.
- **Sepia**: a fixed `colorchannelmixer` matrix (`.393:.769:.189:0:.349:.686:.168:0:.272:.534:.131`). It takes no intensity.
- **Vignette**: `vignette=angle=(π/5) × Intensity`. A value of `Intensity = 1.0` gives the strongest vignette (`π/5 ≈ 0.6283`).
- **NoiseReduction**: `hqdn3d` with the luma/chroma values described above.
- **FilmGrain**: `noise=alls=N:allf=t+u`. `N = (int)(Intensity × 80)`, so the value is truncated, not rounded. It adds temporal and uniform noise.
- **FlipHorizontal / FlipVertical**: `hflip` / `vflip`. They take no intensity.

## Caveats

These follow from the current code. The documentation describes the behaviour, and the code is not changed.

- **Intensity is not validated.** `VideoEffect.Intensity` has a `[Range(0.0, 1.0)]` attribute, but the processor never checks it. Values outside `[0, 1]` are passed through to FFmpeg unclamped. `NaN` is written as `NaN`, which FFmpeg rejects.
- **Neutral points are not all 0.5.** The class-level XML remark says `0.5` maps to the neutral value "in every case". That is true only for `Brightness` (and the flip/greyscale effects, which have no intensity). Contrast, saturation, blur, sharpen, vignette, noise reduction and film grain have different values at `0.5`, as the table shows.
- **Some XML examples are stale.** The `BuildFilterGraph` example in the code (`eq=brightness=0.200,gblur=sigma=3.8,vflip`) does not match the mapping. Brightness `0.2` produces `-0.300`, and `sigma=3.8` corresponds to an intensity of about `0.17`. The XML comment on `BuildContrastFilter` (`0.5 → 1.0`) and on `BuildSaturationFilter` (`0.5 → neutral 1.0`) also disagree with the code.
- **Number formatting uses the current culture.** The `{value:F3}` format specifiers follow `CultureInfo.CurrentCulture`. In a culture with a comma decimal separator, output looks like `eq=brightness=-0,300`, which is not valid FFmpeg syntax.
- **`VideoEffect.Parameters` is ignored.** The model defines a `Parameters` dictionary for passing raw FFmpeg options. The processor never reads it, so these values have no effect on the output.
- **Empty and whitespace filters are dropped.** `BuildFilterGraph` removes blank filter strings. Every current mapping returns a non-blank string, so this is a safeguard and never triggers.

## Related types

- `VideoEffect` (`Domain/Models/VideoEditorModels.cs`): the input descriptor, with `Type`, `Intensity` and `Parameters`.
- `VideoEffectType` (`Domain/Enums/VideoEditorEnums.cs`): the twelve supported effect types.
- `EffectsOperation`: wraps an ordered list of `VideoEffect` for a `VideoEditSession`.
- `VideoEditorService.ApplyEffectsAsync` (`Application/Services/VideoEditorService.cs`): calls `BuildFilterGraph` and passes the result to FFmpeg.
