// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Reflection;

namespace CoubDownloader.Infrastructure.Utilities;

/// <summary>Application version and compatibility utilities</summary>
public static class VersionHelper
{
    /// <summary>Get application version</summary>
    public static string GetApplicationVersion()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        return version?.ToString() ?? "1.0.0.0";
    }

    /// <summary>Get .NET runtime version</summary>
    public static string GetRuntimeVersion()
    {
        return Environment.Version.ToString();
    }

    /// <summary>Get operating system</summary>
    public static string GetOperatingSystem()
    {
        return Environment.OSVersion.VersionString;
    }

    /// <summary>Get full application information</summary>
    public static ApplicationInfo GetApplicationInfo()
    {
        return new ApplicationInfo
        {
            AppVersion = GetApplicationVersion(),
            RuntimeVersion = GetRuntimeVersion(),
            OperatingSystem = GetOperatingSystem(),
            ProcessorCount = Environment.ProcessorCount,
            Is64BitProcess = Environment.Is64BitProcess,
            BuildDate = GetBuildDate()
        };
    }

    /// <summary>Get build date from assembly</summary>
    public static DateTime GetBuildDate()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var attribute = assembly.GetCustomAttribute<BuildDateAttribute>();

        if (attribute != null)
            return attribute.BuildDate;

        // Fallback: use file write time
        var filePath = assembly.Location;
        return File.GetLastWriteTime(filePath);
    }

    /// <summary>Compare versions</summary>
    public static int CompareVersions(string version1, string version2)
    {
        return Version.Parse(version1).CompareTo(Version.Parse(version2));
    }

    /// <summary>Check if current version is greater than specified</summary>
    public static bool IsGreaterThan(string currentVersion, string compareVersion)
    {
        return Version.Parse(currentVersion) > Version.Parse(compareVersion);
    }

    /// <summary>Check if update is available</summary>
    public static bool IsUpdateAvailable(string currentVersion, string latestVersion)
    {
        try
        {
            return Version.Parse(currentVersion) < Version.Parse(latestVersion);
        }
        catch
        {
            return false;
        }
    }
}

/// <summary>Application information</summary>
public class ApplicationInfo
{
    public string AppVersion { get; set; } = "";
    public string RuntimeVersion { get; set; } = "";
    public string OperatingSystem { get; set; } = "";
    public int ProcessorCount { get; set; }
    public bool Is64BitProcess { get; set; }
    public DateTime BuildDate { get; set; }

    public override string ToString()
    {
        return $@"
Coub Downloader Information:
  Version:     {AppVersion}
  Build Date:  {BuildDate:yyyy-MM-dd}
  Runtime:     {RuntimeVersion}
  OS:          {OperatingSystem}
  Processors:  {ProcessorCount}
  Architecture: {(Is64BitProcess ? "x64" : "x86")}";
    }
}

/// <summary>Custom attribute for marking build date</summary>
[AttributeUsage(AttributeTargets.Assembly)]
public class BuildDateAttribute : Attribute
{
    public DateTime BuildDate { get; }

    public BuildDateAttribute(string buildDate)
    {
        if (DateTime.TryParse(buildDate, out var date))
            BuildDate = date;
    }
}

/// <summary>Feature flag management</summary>
public class FeatureFlags
{
    private readonly Dictionary<string, bool> _flags = [];

    public void Enable(string featureName)
    {
        _flags[featureName.ToLowerInvariant()] = true;
    }

    public void Disable(string featureName)
    {
        _flags[featureName.ToLowerInvariant()] = false;
    }

    public bool IsEnabled(string featureName)
    {
        return _flags.TryGetValue(featureName.ToLowerInvariant(), out var enabled) && enabled;
    }

    public bool Toggle(string featureName)
    {
        var key = featureName.ToLowerInvariant();
        var current = _flags.TryGetValue(key, out var val) && val;
        _flags[key] = !current;
        return _flags[key];
    }

    public IEnumerable<(string Name, bool Enabled)> GetAllFlags()
    {
        return _flags.Select(kvp => (kvp.Key, kvp.Value));
    }

    public void Reset()
    {
        _flags.Clear();
    }
}
