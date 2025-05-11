using MoonstoneCms.Core;

namespace MoonstoneCms.Desktop;

/// <summary>
/// The state of the current project we have open
/// </summary>
public static class ProjectState
{
    public static StaticSiteProject? Current { get; set; }
}