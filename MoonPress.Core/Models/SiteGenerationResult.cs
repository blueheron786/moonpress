namespace MoonPress.Core;

/// <summary>
/// Result of a site generation operation
/// </summary>
public class SiteGenerationResult
{
    public bool Success { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan Duration => EndTime - StartTime;
    public string Message { get; set; } = string.Empty;
    public Exception? Error { get; set; }
    public int PagesGenerated { get; set; }
    public List<string> GeneratedFiles { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}