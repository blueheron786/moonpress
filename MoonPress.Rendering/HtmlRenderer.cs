using MoonPress.Core.Models;
using MoonPress.Core.Renderers;
using Markdig;

namespace MoonPress.Rendering;

public class ContentItemHtmlRenderer : IHtmlRenderer
{
    private readonly MarkdownPipeline _markdownPipeline;

    public ContentItemHtmlRenderer()
    {
        // Configure Markdig pipeline with common extensions
        _markdownPipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build();
    }

    public string RenderHtml(ContentItem contentItem)
    {
        if (contentItem == null)
        {
            throw new ArgumentNullException(nameof(contentItem));
        }

        // Convert markdown content to HTML
        var contentHtml = Markdown.ToHtml(contentItem.Contents ?? string.Empty, _markdownPipeline);

        // Determine if this is a page or a post/article
        // Pages (in content/pages/) don't show title and date
        // Posts/articles do show them
        var isPage = contentItem.FilePath.Contains(Path.Combine("content", "pages"));
        
        if (isPage)
        {
            // For pages, just return the rendered markdown content
            var html = $@"<div class=""content"">
{contentHtml}
</div>";
            return html;
        }
        else
        {
            // For posts/articles, include title and date
            var html = $@"<h1>{contentItem.Title}</h1>
<p>Published on: {contentItem.DatePublished:yyyy-MM-dd HH:mm:ss}</p>
<div class=""content"">
{contentHtml}
</div>";
            return html;
        }
    }
}
