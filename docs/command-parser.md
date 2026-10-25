# Command Parser

`CommandParser` (`Presentation/CLI/CommandParser.cs`) turns the raw `string[] args` of the CLI into strongly-typed option objects, one method per sub-command:

| Method | Sub-command | Returns |
|--------|-------------|---------|
| `ParseDownloadOptions(args)` | `download` | `DownloadCommandOptions` |
| `ParseConvertOptions(args)` | `convert` | `ConvertCommandOptions` |
| `ParseBatchOptions(args)` | `batch` | `BatchCommandOptions` |
| `ParseInfoOptions(args)` | `info` | `InfoCommandOptions` |

The sub-command itself is dispatched in `CommandLineInterface` (`args[0]`, lower-cased). Each parser method receives the full `args` array and starts scanning at index `1`, so `args[0]` is never treated as an option.

The parser is deliberately lenient: it never throws on bad input, never reports errors, and never validates paths. Unknown or malformed input is silently ignored or replaced by a default. `args` must not be `null`.

## Tokenizing rules

The scan loop applies these rules to every token from index 1 onward:

1. **Only tokens starting with `--` are options.** Any other token (a bare word, a path, a URL) is skipped without error.
2. **Option name** = the text after `--`, lower-cased with `ToLowerInvariant()`. So `--URL`, `--Url` and `--url` are equivalent.
3. **Option value** = the *next* token, taken verbatim. It is consumed only when the option name is recognized.
4. **Recognized name → value consumed.** The loop skips past the value, so it is never re-examined as an option.
5. **Unrecognized name → value not consumed.** The following token is scanned normally. In practice it is almost always skipped by rule 1.
6. **Missing value** (the option is the last token) → the value is `null`.
7. **Value may look like an option.** There is no check for a leading `--` on the value. `--url --format` sets `Url` to `"--format"`.
8. **Repeated options:** the last occurrence wins for scalar properties. `--files` is replaced, not appended.
9. **Not supported:** the `--name=value` form. The whole token `url=https://…` is an unrecognized name and is ignored.

## Value conversion

### Enums (`--format`, `--quality`)

Parsed with `Enum.TryParse<T>(value, ignoreCase: true, …)`:

- Member names are matched case-insensitively, and surrounding whitespace is trimmed.
- On failure (unknown name, `null`, empty) the value falls back to a default:
  - `--format` → `VideoFormat.MP4`
  - `--quality` → `VideoQuality.High`
- Numeric strings are also accepted by `Enum.TryParse` (for example `"3"`), and the result is not checked against defined members.

Accepted values:

| Option | Accepted values (case-insensitive) | Default on failure |
|--------|------------------------------------|--------------------|
| `--format` | `MP4`, `WebM`, `MobileShorts`, `YouTubeShorts`, `HighQuality4K` | `MP4` |
| `--quality` | `Low`, `Medium`, `High`, `Maximum` | `High` |

### Integers (`--width`, `--height`, `--fps`)

Parsed with `int.TryParse(value, out …)`:

- Accepts an optional sign and surrounding whitespace. Parsing uses the current culture's number format, so thousands separators and decimals are rejected.
- On failure the property is **left unchanged**. In `ParseConvertOptions` that means `null`. The value token is still consumed.
- There is no range check. `0` and negative numbers are stored as given.

### Lists (`--files`)

- The value is split on `,`. Each piece is trimmed.
- Empty pieces are kept: `"a.mp4,,b.mp4"` gives three entries, the middle one empty; a trailing comma gives a trailing `""`.
- A missing value sets `Files` to `null`, not to an empty list.

### Strings (`--url`, `--output`, `--input`, `--name`)

Stored exactly as given. The parser does not trim, normalize, expand `~`, or check that files or directories exist.

## Supported options per command

### `download` → `DownloadCommandOptions`

| Option | Property | Type | Default |
|--------|----------|------|---------|
| `--url` | `Url` | `string?` | `null` |
| `--output` | `OutputPath` | `string?` | `null` |
| `--format` | `Format` | `VideoFormat` | `MP4` |
| `--quality` | `Quality` | `VideoQuality` | `High` |

### `convert` → `ConvertCommandOptions`

| Option | Property | Type | Default |
|--------|----------|------|---------|
| `--input` | `InputFile` | `string?` | `null` |
| `--output` | `OutputFile` | `string?` | `null` |
| `--format` | `Format` | `VideoFormat` | `MP4` |
| `--quality` | `Quality` | `VideoQuality` | `High` |
| `--width` | `Width` | `int?` | `null` |
| `--height` | `Height` | `int?` | `null` |
| `--fps` | `FrameRate` | `int?` | `null` |

### `batch` → `BatchCommandOptions`

| Option | Property | Type | Default |
|--------|----------|------|---------|
| `--files` | `Files` | `List<string>` | `[]` |
| `--output` | `OutputDirectory` | `string?` | `null` |
| `--name` | `Name` | `string?` | `null` |
| `--format` | `Format` | `VideoFormat` | `MP4` |
| `--quality` | `Quality` | `VideoQuality` | `High` |

Note that `batch` has no `--width`, `--height` or `--fps`. Passing them is silently ignored.

### `info` → `InfoCommandOptions`

| Option | Property | Type | Default |
|--------|----------|------|---------|
| `--url` | `Url` | `string?` | `null` |

`info` recognizes only `--url`. Every other option is ignored.

## Examples

Each row shows the command line and the resulting option values. Unlisted properties keep their defaults.

| Command line | Result |
|--------------|--------|
| `download --url https://coub.com/view/abc --format WEBM --quality maximum` | `Url` = the URL, `Format` = `WebM`, `Quality` = `Maximum` |
| `download --URL https://coub.com/view/abc --format xyz` | `Url` set; `Format` = `MP4` (fallback) |
| `download --url --format mp4` | `Url` = `"--format"`; `mp4` is a bare token and is skipped; `Format` = `MP4` (default) |
| `download --url=https://coub.com/view/abc` | `Url` = `null`; the whole token is an unrecognized name |
| `download --url https://coub.com/view/abc --width 640` | `--width` is not a `download` option and is ignored |
| `convert --input in.mp4 --output out.mp4 --width 1280 --height abc --fps 30` | `Width` = 1280, `Height` = `null`, `FrameRate` = 30 |
| `batch --files "a.mp4, b.mp4" --output ./out` | `Files` = `["a.mp4", "b.mp4"]`, `OutputDirectory` = `./out` |
| `batch --files a.mp4, b.mp4` | `Files` = `["a.mp4,"]`; `b.mp4` is a bare token and is skipped |
| `info --url https://coub.com/view/abc --format mp4` | `Url` set; `--format` is ignored |

The `batch` row without quotes is a shell-quoting pitfall. The shell splits on the space after the comma, so quote the whole list.

## Behavior summary

- No exceptions are thrown from the parse methods for bad input. The parser cannot report a typo or an unknown option to the user.
- Defaults come from the option classes' property initializers. An option that is absent, malformed, or unrecognized leaves its default in place (or `null` for nullable properties).
- Parsing is pure: the same `args` always produce the same options. The parser holds no state, so one instance can be reused.
