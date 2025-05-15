using MoonPress.Core.Models;
using MoonPress.Core.Renderers;

namespace MoonPress.Rendering;

public class ContentItemMarkdownRenderer : IMarkdownRenderer
{
    public string RenderMarkdown(ContentItem contentItem)
    {
        if (contentItem == null)
        {
            throw new ArgumentNullException(nameof(contentItem), "Content item cannot be null.");
        }

        // Rudimentary markdown with meta-data. We need something ... more.
        return $"---\n" +
            $"id: {contentItem.Id}\n" +
            $"title: {contentItem.Title}\n" +
            $"datePublished: {contentItem.DatePublished:yyyy-MM-dd HH:mm:ss}\n" +
            $"isDraft: {contentItem.IsDraft.ToString().ToLower()}\n" +
            $"summary: {contentItem.Summary}\n" +
            $"---\n\n" +
            $"{contentItem.Contents}";
    }
}
