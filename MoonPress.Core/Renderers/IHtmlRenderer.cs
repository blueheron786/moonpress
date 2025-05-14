using MoonPress.Core.Models;

namespace MoonPress.Core.Renderers;

public interface IHtmlRenderer
{
    string RenderHtml(ContentItem contentItem);
}
