using MoonPress.Core.Models;

namespace MoonPress.Core.Renderers;

public interface IMarkdownRenderer
{
    string RenderMarkdown(ContentItem contentItem);
}
