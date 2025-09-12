using MoonPress.Core.Models;
using MoonPress.Core.Renderers;

namespace MoonPress.Rendering;

public class ContentItemHtmlRenderer : IHtmlRenderer
{
    public string RenderHtml(ContentItem contentItem)
    {
        if (contentItem == null)
        {
            throw new ArgumentNullException(nameof(contentItem));
        }

        // Render just the content, the theme layout will handle the HTML structure
        var html = $@"<h1>{contentItem.Title}</h1>
<p>Published on: {contentItem.DatePublished:yyyy-MM-dd HH:mm:ss}</p>
<div class=""content"">
{contentItem.Contents}
</div>";

        return html;
    }
}
