namespace MoonPress.Core;

/// <summary>
/// Result of loading a theme layout
/// </summary>
public class ThemeLayoutResult
{
    public bool Success { get; set; }
    public string Layout { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}