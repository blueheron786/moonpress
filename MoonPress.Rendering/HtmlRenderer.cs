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

        // Render the content item to HTML. TODO: it's way more than this...
        var projectBaseUrl = "https://example.com"; // TODO: get this from the config
        var ogTitle = contentItem.Title.Replace('"', '\'');
        var ogSlug = contentItem.Slug.Replace(' ', '-');
        // If you don't have a description, use the first 140 characters of the content, rounded up to the nearest whole word.
        var ogDescription = contentItem.Summary ?? contentItem.Contents.Substring(0, contentItem.Contents.IndexOf(' ', 140));
        ogDescription = ogDescription.Replace('"', '\'');

        // TODO: we need the layout/theme file ...
        var openGraphTags = $@"<meta property=""og:title"" content=""{ogTitle}"" />
            <meta property=""og:type"" content=""article"" />
            <meta property=""og:url"" content=""{projectBaseUrl}/{ogSlug}"" />
            <!-- meta property=""og:image"" content=""https://example.com/image.jpg"" / -->
            <meta property=""og:description"" content=""{ogDescription}"" />";

        var head = $@"<head>
            <title>{contentItem.Title}</title>
            {openGraphTags}
        </head>";

        var html = $@"<h1>{contentItem.Title}</h1>
        <p>Published on: {contentItem.DatePublished:yyyy-MM-dd HH:mm:ss}</p>
        <p>{contentItem.Contents}</p>";

        return head + html;
    }
}
