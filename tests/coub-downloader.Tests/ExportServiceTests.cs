using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CoubDownloader.Domain.Models;
using CoubDownloader.Infrastructure.Middleware;
using CoubDownloader.Infrastructure.Reporting;
using FluentAssertions;
using Moq;
using Xunit;

namespace CoubDownloader.Tests;

/// <summary>
/// Contains unit tests for the CSV export of download results in <see cref="ExportService"/>.
/// </summary>
public class ExportServiceTests : IDisposable
{
    private const string CsvHeader = "id,title,status,size,duration,path";

    private readonly Mock<ILoggingService> _mockLogger;
    private readonly ExportService _sut;
    private readonly List<string> _tempFiles = new();

    public ExportServiceTests()
    {
        _mockLogger = new Mock<ILoggingService>();
        _sut = new ExportService(_mockLogger.Object);
    }

    public void Dispose()
    {
        foreach (var file in _tempFiles)
        {
            if (File.Exists(file))
            {
                File.Delete(file);
            }
        }
    }

    private string NewTempFile()
    {
        var path = Path.GetTempFileName();
        _tempFiles.Add(path);
        return path;
    }

    private static DownloadResult CreateResult(
        string id = "r1",
        string? title = "Sample",
        bool success = true,
        long size = 1024,
        long durationMs = 500,
        string? path = "/out/sample.mp4") => new()
    {
        Id = id,
        TaskId = "t1",
        Title = title,
        Success = success,
        OutputFileSizeBytes = size,
        ProcessingTimeMs = durationMs,
        OutputFilePath = path,
        CompletedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
    };

    private async Task<string> ExportCsvAsync(List<DownloadResult> results)
    {
        var path = NewTempFile();
        var ok = await _sut.ExportDownloadResultsAsync(results, path, ExportFormat.Csv);
        ok.Should().BeTrue();
        return await File.ReadAllTextAsync(path);
    }

    // Test cases for ExportDownloadResultsAsync (CSV)

    [Fact]
    public async Task ExportDownloadResultsAsync_Csv_WritesExpectedHeader()
    {
        var csv = await ExportCsvAsync(new List<DownloadResult> { CreateResult() });

        csv.Should().StartWith(CsvHeader + Environment.NewLine);
    }

    [Fact]
    public async Task ExportDownloadResultsAsync_Csv_WritesRowWithAllColumns()
    {
        var csv = await ExportCsvAsync(new List<DownloadResult>
        {
            CreateResult(id: "r1", title: "Sample", success: true, size: 2048, durationMs: 750, path: "/out/a.mp4")
        });

        var lines = csv.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        lines.Should().HaveCount(2);
        lines[1].Should().Be("r1,Sample,completed,2048,750,/out/a.mp4");
    }

    [Fact]
    public async Task ExportDownloadResultsAsync_Csv_FailedResult_ReportsFailedStatus()
    {
        var csv = await ExportCsvAsync(new List<DownloadResult>
        {
            CreateResult(success: false, path: null)
        });

        var lines = csv.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        lines[1].Should().Be("r1,Sample,failed,1024,500,");
    }

    [Fact]
    public async Task ExportDownloadResultsAsync_Csv_NullTitleAndPath_WritesEmptyFields()
    {
        var csv = await ExportCsvAsync(new List<DownloadResult>
        {
            CreateResult(title: null, path: null)
        });

        var lines = csv.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        lines[1].Should().Be("r1,,completed,1024,500,");
    }

    [Fact]
    public async Task ExportDownloadResultsAsync_Csv_TitleWithComma_IsQuoted()
    {
        var csv = await ExportCsvAsync(new List<DownloadResult>
        {
            CreateResult(title: "Cats, dogs and birds")
        });

        csv.Should().Contain("r1,\"Cats, dogs and birds\",completed,");
    }

    [Fact]
    public async Task ExportDownloadResultsAsync_Csv_TitleWithQuotes_AreDoubledAndQuoted()
    {
        var csv = await ExportCsvAsync(new List<DownloadResult>
        {
            CreateResult(title: "He said \"hi\"")
        });

        csv.Should().Contain("r1,\"He said \"\"hi\"\"\",completed,");
    }

    [Fact]
    public async Task ExportDownloadResultsAsync_Csv_TitleWithNewline_IsQuoted()
    {
        var csv = await ExportCsvAsync(new List<DownloadResult>
        {
            CreateResult(title: "line one\nline two")
        });

        csv.Should().Contain("r1,\"line one\nline two\",completed,");
    }

    [Fact]
    public async Task ExportDownloadResultsAsync_Csv_PathWithComma_IsQuoted()
    {
        var csv = await ExportCsvAsync(new List<DownloadResult>
        {
            CreateResult(path: "/out/a, b.mp4")
        });

        csv.Should().Contain(",completed,1024,500,\"/out/a, b.mp4\"");
    }

    [Fact]
    public async Task ExportDownloadResultsAsync_Csv_EmptyList_WritesHeaderOnly()
    {
        var csv = await ExportCsvAsync(new List<DownloadResult>());

        csv.Should().Be(CsvHeader + Environment.NewLine);
    }

    [Fact]
    public async Task ExportDownloadResultsAsync_Csv_UnwritablePath_ReturnsFalseAndLogs()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "missing", "out.csv");

        var ok = await _sut.ExportDownloadResultsAsync(
            new List<DownloadResult> { CreateResult() }, path, ExportFormat.Csv);

        ok.Should().BeFalse();
        _mockLogger.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<Exception>(), It.IsAny<string?>()), Times.Once);
    }

    // Test cases for EscapeCsvField

    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("plain", "plain")]
    [InlineData("a,b", "\"a,b\"")]
    [InlineData("say \"x\"", "\"say \"\"x\"\"\"")]
    [InlineData("a\rb", "\"a\rb\"")]
    [InlineData("a\nb", "\"a\nb\"")]
    [InlineData("semi;colon", "semi;colon")]
    public void EscapeCsvField_ReturnsRfc4180Value(string? input, string expected)
    {
        ExportService.EscapeCsvField(input).Should().Be(expected);
    }
}
