using MoonstoneCms.Core;

namespace MoonstoneCms.Desktop;

/// <summary>
/// The state of the current project we have open
/// </summary>
public static class ProjectState
{
    /// <summary>
    /// Notify interested parties when the project is loaded.
    /// e.g. the nav bar page, so that it can add project-specific links.
    /// </summary>
    public static event Action? OnProjectLoaded;

    private static StaticSiteProject? _current;

    public static StaticSiteProject? Current
    {
        get => _current;
        set
        {
            _current = value;
            OnProjectLoaded?.Invoke();
        }
    }
}