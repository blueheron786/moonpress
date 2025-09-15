using System.Text;
using MoonPress.Core.Models;

namespace MoonPress.Core.Templates;

/// <summary>
/// Processes {{ posts }} template blocks with filtering support
/// </summary>
public class PostsTemplateProcessor
{
    /// <summary>
    /// Processes all {{ posts }} blocks in the given template, replacing them with filtered content
    /// </summary>
    /// <param name="template">The template containing {{ posts }} blocks</param>
    /// <param name="contentItems">Available content items to filter from</param>
    /// <returns>Template with all {{ posts }} blocks replaced with generated HTML</returns>
    public string ProcessPostsBlocks(string template, List<ContentItem> contentItems)
    {
        if (string.IsNullOrWhiteSpace(template) || contentItems == null)
            return template;

        var result = template;
        
        // Find all {{ posts }} blocks using simple string search
        const string startPattern = "{{ posts";
        const string endPattern = "{{ /posts }}";
        
        int startIndex = 0;
        while ((startIndex = result.IndexOf(startPattern, startIndex)) != -1)
        {
            // Find the end of the opening tag
            var openTagEnd = result.IndexOf("}}", startIndex);
            if (openTagEnd == -1) break;
            openTagEnd += 2;
            
            // Find the closing tag
            var closeTagStart = result.IndexOf(endPattern, openTagEnd);
            if (closeTagStart == -1) break;
            
            var fullBlock = result.Substring(startIndex, closeTagStart + endPattern.Length - startIndex);
            var openTag = result.Substring(startIndex, openTagEnd - startIndex);
            var innerTemplate = result.Substring(openTagEnd, closeTagStart - openTagEnd);
            
            // Parse filters from the opening tag
            var filters = ParseFilters(openTag);
            
            // Apply filters to get the posts
            var filteredPosts = ApplyFilters(contentItems, filters);
            
            // Generate HTML for each post
            var generatedHtml = GeneratePostsHtml(filteredPosts, innerTemplate);
            
            // Replace the entire block with generated HTML
            result = result.Replace(fullBlock, generatedHtml);
            
            // Move past this replacement to find any other blocks
            startIndex = startIndex + generatedHtml.Length;
        }
        
        return result;
    }

    /// <summary>
    /// Parses filter parameters from the opening {{ posts }} tag
    /// </summary>
    /// <param name="openTag">The opening tag (e.g., "{{ posts | category="blog" | limit=5 }}")</param>
    /// <returns>Dictionary of filter key-value pairs</returns>
    private static Dictionary<string, string> ParseFilters(string openTag)
    {
        var filters = new Dictionary<string, string>();
        
        // Simple parsing: look for | followed by key="value" or key=value
        var parts = openTag.Split('|');
        for (int i = 1; i < parts.Length; i++) // Skip the first part "{{ posts"
        {
            var filter = parts[i].Trim().TrimEnd('}');
            var equalIndex = filter.IndexOf('=');
            if (equalIndex > 0)
            {
                var key = filter.Substring(0, equalIndex).Trim();
                var value = filter.Substring(equalIndex + 1).Trim().Trim('"');
                filters[key] = value;
            }
        }
        
        return filters;
    }

    /// <summary>
    /// Applies the specified filters to the content items
    /// </summary>
    /// <param name="contentItems">All available content items</param>
    /// <param name="filters">Filters to apply (category, limit, etc.)</param>
    /// <returns>Filtered and ordered list of content items</returns>
    private static List<ContentItem> ApplyFilters(List<ContentItem> contentItems, Dictionary<string, string> filters)
    {
        var result = contentItems.Where(item => !item.IsDraft).ToList();
        
        // Apply category filter
        if (filters.TryGetValue("category", out var category) && !string.IsNullOrWhiteSpace(category))
        {
            result = result.Where(item => 
                string.Equals(item.Category, category, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
        
        // Order by date (most recent first)
        result = result.OrderByDescending(item => item.DatePublished).ToList();
        
        // Apply limit filter
        if (filters.TryGetValue("limit", out var limitStr) && int.TryParse(limitStr, out var limit) && limit > 0)
        {
            result = result.Take(limit).ToList();
        }
        
        return result;
    }

    /// <summary>
    /// Generates HTML for the filtered posts using the provided template
    /// </summary>
    /// <param name="posts">Filtered posts to generate HTML for</param>
    /// <param name="innerTemplate">Template to use for each post</param>
    /// <returns>Generated HTML string</returns>
    private static string GeneratePostsHtml(List<ContentItem> posts, string innerTemplate)
    {
        var generatedHtml = new StringBuilder();
        
        foreach (var post in posts)
        {
            var postHtml = innerTemplate
                .Replace("{{ slug }}", post.Slug)
                .Replace("{{ title }}", post.Title)
                .Replace("{{ category }}", post.Category)
                .Replace("{{ summary }}", post.Summary ?? "")
                .Replace("{{ date }}", post.DatePublished.ToString("MMMM dd, yyyy"));
            generatedHtml.AppendLine(postHtml);
        }
        
        return generatedHtml.ToString();
    }
}