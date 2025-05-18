using System.Text;
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

        var sb = new StringBuilder();
        sb.AppendLine("---");
        sb.AppendLine($"id: {contentItem.Id}");
        sb.AppendLine($"title: {EscapeYaml(contentItem.Title)}");
        sb.AppendLine($"datePublished: {contentItem.DatePublished:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"dateUpdated: {contentItem.DateUpdated:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"category: {EscapeYaml(contentItem.Category)}");
        sb.AppendLine($"tags: {string.Join(", ", (contentItem.Tags ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(t => $"{EscapeYaml(t)}"))}");
        sb.AppendLine($"isDraft: {contentItem.IsDraft.ToString().ToLower()}");
        sb.AppendLine($"summary: {EscapeYaml(contentItem.Summary)}");

        // Add custom fields
        if (contentItem.CustomFields != null)
        {
            foreach (var kvp in contentItem.CustomFields)
            {
                sb.AppendLine($"{kvp.Key}: {EscapeYaml(kvp.Value)}");
            }
        }

        sb.AppendLine("---");
        sb.AppendLine();
        sb.Append(contentItem.Contents);

        return sb.ToString();
    }
    
    private static string EscapeYaml(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return value.Replace("\"", "\\\"");
    }
}
