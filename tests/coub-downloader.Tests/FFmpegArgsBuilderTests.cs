using System;
using System.Collections.Generic;
using System.Text;
using CoubDownloader.Application.Services;
using CoubDownloader.Domain.Models;
using FluentAssertions;
using Xunit;

namespace CoubDownloader.Tests;

/// <summary>
/// Contains unit tests for <see cref="FFmpegArgsBuilder"/>: quoting/escaping of command-line values and the argument lists it builds.
/// </summary>
public class FFmpegArgsBuilderTests
{
    [Theory]
    [InlineData("plain", "\"plain\"")]
    [InlineData("C:\\videos\\clip.mp4", "\"C:\\videos\\clip.mp4\"")]
    [InlineData("with space.mp4", "\"with space.mp4\"")]
    [InlineData("", "\"\"")]
    public void QuoteArgument_WrapsValueInQuotes_AndLeavesPlainBackslashesAlone(string value, string expected)
    {
        FFmpegArgsBuilder.QuoteArgument(value).Should().Be(expected);
    }

    [Fact]
    public void QuoteArgument_EscapesEmbeddedQuote()
    {
        FFmpegArgsBuilder.QuoteArgument("say \"hi\".mp4").Should().Be("\"say \\\"hi\\\".mp4\"");
    }

    [Fact]
    public void QuoteArgument_DoublesBackslashesBeforeEmbeddedQuote()
    {
        // one backslash + quote -> 2n+1 = 3 backslashes, then the quote
        FFmpegArgsBuilder.QuoteArgument("a\\\"b").Should().Be("\"a\\\\\\\"b\"");
    }

    [Fact]
    public void QuoteArgument_DoublesTrailingBackslashes()
    {
        // trailing backslashes must not escape the closing quote
        FFmpegArgsBuilder.QuoteArgument("dir\\").Should().Be("\"dir\\\\\"");
        FFmpegArgsBuilder.QuoteArgument("dir\\\\").Should().Be("\"dir\\\\\\\\\"");
    }

    [Theory]
    [InlineData("in.mp4", false)]
    [InlineData("C:\\dir\\in.mp4", false)]
    [InlineData("scale=1920:1080", false)]
    [InlineData("", true)]
    [InlineData("has space", true)]
    [InlineData("tab\there", true)]
    [InlineData("quo\"te", true)]
    public void NeedsQuoting_ReportsValuesThatCannotBeBare(string value, bool expected)
    {
        FFmpegArgsBuilder.NeedsQuoting(value).Should().Be(expected);
    }

    [Fact]
    public void ToCommandLine_QuotesOnlyWhenNeeded_AndAlwaysForAddQuoted()
    {
        var line = new FFmpegArgsBuilder()
            .Add("-y")
            .Add("-c:v", "h264")
            .Add("-metadata", "title=two words")
            .AddQuoted("out.mp4")
            .ToCommandLine();

        line.Should().Be("-y -c:v h264 -metadata \"title=two words\" \"out.mp4\"");
    }

    [Fact]
    public void ToCommandLine_QuotesEmptyValue()
    {
        new FFmpegArgsBuilder().Add("-metadata", "").ToCommandLine().Should().Be("-metadata \"\"");
    }

    [Fact]
    public void ToArray_ReturnsRawValuesWithoutQuoting()
    {
        var args = new FFmpegArgsBuilder()
            .Add("-i", "my clip \"1\".mp4")
            .AddQuoted("out put.mp4")
            .ToArray();

        args.Should().Equal("-i", "my clip \"1\".mp4", "out put.mp4");
    }

    [Theory]
    [InlineData("plain value")]
    [InlineData("say \"hi\"")]
    [InlineData("trailing\\")]
    [InlineData("back\\\"slash")]
    [InlineData("C:\\Program Files\\ffmpeg\\bin\\ffmpeg.exe")]
    [InlineData("[0:v]split[a][b];[a]scale=1080:1920[x]")]
    [InlineData("")]
    public void ToCommandLine_RoundTripsThroughMsvcrtParser(string value)
    {
        var line = new FFmpegArgsBuilder().AddQuoted(value).ToCommandLine();

        ParseCommandLine(line).Should().Equal(value);
    }

    [Fact]
    public void ToCommandLine_RoundTripsPathsWithQuotesAndSpaces()
    {
        const string path = "/tmp/my \"best\" clip\\";
        var line = new FFmpegArgsBuilder()
            .Add("-i").AddQuoted(path)
            .Add("-y")
            .ToCommandLine();

        ParseCommandLine(line).Should().Equal("-i", path, "-y");
    }

    [Fact]
    public void ConcatListEntry_EscapesSingleQuotes()
    {
        FFmpegArgsBuilder.ConcatListEntry("/tmp/it's.mp4").Should().Be("file '/tmp/it'\\''s.mp4'");
    }

    [Fact]
    public void ConcatListEntry_LeavesOrdinaryPathUnchanged()
    {
        FFmpegArgsBuilder.ConcatListEntry("/tmp/clip one.mp4").Should().Be("file '/tmp/clip one.mp4'");
    }

    [Fact]
    public void ConcatArguments_BuildsCopyConcatArgumentList()
    {
        FFmpegArgsBuilder.ConcatArguments("list.txt", "out.mp4")
            .Should().Equal("-f", "concat", "-safe", "0", "-i", "list.txt", "-c", "copy", "-y", "out.mp4");
    }

    [Fact]
    public void MetadataProbeCommand_QuotesFilePath()
    {
        FFmpegArgsBuilder.MetadataProbeCommand("my clip.mp4").Should().Be(
            "-v error -select_streams v:0 " +
            "-show_entries stream=width,height,duration,codec_name,r_frame_rate,bit_rate " +
            "-show_entries format=size,duration,bit_rate,format_name -of json \"my clip.mp4\"");
    }

    [Fact]
    public void MediaInfoProbeCommand_QuotesFilePathWithEmbeddedQuote()
    {
        const string path = "clip \"x\".mp4";
        var line = FFmpegArgsBuilder.MediaInfoProbeCommand(path);

        line.Should().StartWith("-v error -show_entries format=duration,size,bit_rate -of json ");
        ParseCommandLine(line).Should().Equal("-v", "error", "-show_entries", "format=duration,size,bit_rate", "-of", "json", path);
    }

    [Fact]
    public void ConversionCommand_BuildsDefaultScaleAndCodecs()
    {
        var settings = new ConversionSettings { Id = "test" };

        FFmpegArgsBuilder.ConversionCommand("in.mp4", "out.mp4", settings).Should().Be(
            "-i \"in.mp4\" -vf \"scale=1920:1080:force_original_aspect_ratio=decrease\" -r 30 " +
            "-c:v h264 -b:v 5000k -c:a aac -b:a 128k -preset medium \"out.mp4\" -y");
    }

    [Fact]
    public void ConversionCommand_AppendsFadesAndStretchesWhenAspectRatioNotPreserved()
    {
        var settings = new ConversionSettings
        {
            Id = "test",
            PreserveAspectRatio = false,
            ApplyFades = true,
            FadeInMs = 250,
            FadeOutMs = 1500
        };

        FFmpegArgsBuilder.ConversionCommand("in dir/in.mp4", "out.mp4", settings).Should().Be(
            "-i \"in dir/in.mp4\" -vf \"scale=1920:1080,fade=t=in:st=0:d=0.25,fade=t=out:st=10:d=1.5\" -r 30 " +
            "-c:v h264 -b:v 5000k -c:a aac -b:a 128k -preset medium \"out.mp4\" -y");
    }

    [Fact]
    public void ConversionCommand_QuotesPathsWithEmbeddedQuotes()
    {
        var line = FFmpegArgsBuilder.ConversionCommand("a\"b.mp4", "c.mp4", new ConversionSettings { Id = "test" });

        ParseCommandLine(line).Should().Contain("a\"b.mp4");
    }

    [Fact]
    public void AudioTrackCommand_MapsVideoAndAudioInputs()
    {
        var settings = new ConversionSettings { Id = "test", AudioCodec = "libopus", AudioBitrate = 96 };

        FFmpegArgsBuilder.AudioTrackCommand("v.mp4", "a file.m4a", "out.mp4", settings).Should().Be(
            "-i \"v.mp4\" -i \"a file.m4a\" -c:v copy -c:a libopus -b:a 96k -shortest \"out.mp4\" -y");
    }

    [Fact]
    public void RescaleCommand_BuildsH264AacArguments()
    {
        FFmpegArgsBuilder.RescaleCommand("in.mp4", "out.mp4", 640, 360).Should().Be(
            "-i \"in.mp4\" -vf scale=640:360 -c:v h264 -crf 23 -c:a aac -b:a 128k \"out.mp4\" -y");
    }

    [Fact]
    public void ShortsCommand_BuildsBlurredVerticalFilter()
    {
        FFmpegArgsBuilder.ShortsCommand("in.mp4", "shorts.mp4").Should().Be(
            "-i \"in.mp4\" " +
            "-vf \"[0:v]split[a][b];[a]scale=1080:1920:force_original_aspect_ratio=increase,crop=1080:1920,boxblur=20:5[blurred];" +
            "[b]scale=1080:1920:force_original_aspect_ratio=decrease[fg];[blurred][fg]overlay=(W-w)/2:(H-h)/2\" " +
            "-c:v h264 -crf 23 -preset medium -c:a aac -b:a 128k \"shorts.mp4\" -y");
    }

    /// <summary>
    /// Splits a command line into arguments using the MSVCRT rules that .NET applies to <c>ProcessStartInfo.Arguments</c>.
    /// </summary>
    private static List<string> ParseCommandLine(string commandLine)
    {
        var result = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;
        var hasToken = false;
        var i = 0;

        while (i < commandLine.Length)
        {
            var c = commandLine[i];

            if (c == '\\')
            {
                var backslashes = 0;
                while (i < commandLine.Length && commandLine[i] == '\\')
                {
                    backslashes++;
                    i++;
                }

                hasToken = true;
                if (i < commandLine.Length && commandLine[i] == '"')
                {
                    // 2n backslashes + quote -> n backslashes and a toggle; 2n+1 -> n backslashes and a literal quote.
                    current.Append('\\', backslashes / 2);
                    if (backslashes % 2 == 1)
                    {
                        current.Append('"');
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }

                    i++;
                }
                else
                {
                    current.Append('\\', backslashes);
                }

                continue;
            }

            i++;
            if (c == '"')
            {
                inQuotes = !inQuotes;
                hasToken = true;
            }
            else if (!inQuotes && char.IsWhiteSpace(c))
            {
                if (hasToken)
                {
                    result.Add(current.ToString());
                    current.Clear();
                    hasToken = false;
                }
            }
            else
            {
                current.Append(c);
                hasToken = true;
            }
        }

        if (hasToken)
        {
            result.Add(current.ToString());
        }

        return result;
    }
}
