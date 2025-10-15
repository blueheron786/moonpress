using System.Text;
using MoonPress.Core.Models;

namespace MoonPress.Core.Templates;


/// <summary>
/// Processes {{posts}} template blocks with filtering support
/// </summary>
public class PostsTemplateProcessor
{
    /// <summary>
    /// Processes all {{posts}} blocks in the given template, replacing them with filtered content
    /// </summary>
    /// <param name="template">The template containing {{posts}} blocks</param>
    /// <param name="contentItems">Available content items to filter from</param>
    /// <returns>Template with all {{posts}} blocks replaced with generated HTML</returns>
    public string ProcessPostsBlocks(string template, List<ContentItem> contentItems)
    {
        if (string.IsNullOrWhiteSpace(template) || contentItems == null)
            return template;

        var result = template;
        
        // Find all {{ posts }} blocks using simple string search
        const string startPattern = "{{posts";
        const string endPattern = "{{/posts}}";
        
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
    /// Processes template variables for a single content item (for individual page templates)
    /// </summary>
    /// <param name="template">The template content</param>
    /// <param name="contentItem">The content item to substitute variables for</param>
    /// <returns>Template with variables replaced</returns>
    public string ProcessSingleItemVariables(string template, ContentItem contentItem)
    {
        if (string.IsNullOrWhiteSpace(template) || contentItem == null)
            return template;

        // Process conditional sections FIRST, before variable substitution
        // This prevents variables from being replaced inside conditional tags
        var result = ProcessConditionalSections(template, contentItem);

        // Generate URL path based on category: /category/slug.html
        var urlPath = !string.IsNullOrEmpty(contentItem.Category)
            ? $"/{contentItem.Category.ToLowerInvariant()}/{contentItem.Slug}.html"
            : $"/uncategorized/{contentItem.Slug}.html";
            
        result = result
            .Replace("{{url}}", urlPath)
            .Replace("{{title}}", contentItem.Title)
            .Replace("{{category}}", contentItem.Category ?? "")
            .Replace("{{summary}}", contentItem.Summary ?? "")
            .Replace("{{date}}", contentItem.DatePublished.ToString("MMMM dd, yyyy"));
        
        // Process custom fields from frontmatter
        if (contentItem.CustomFields != null)
        {
            foreach (var field in contentItem.CustomFields)
            {
                result = result.Replace($"{{{{{field.Key}}}}}", field.Value);
            }
        }
        
        return result;
    }

    /// <summary>
    /// Parses filter parameters from the opening {{posts}} tag
    /// </summary>
    /// <param name="openTag">The opening tag (e.g., "{{posts | category="blog" | limit=5}}")</param>
    /// <returns>Dictionary of filter key-value pairs</returns>
    private static Dictionary<string, string> ParseFilters(string openTag)
    {
        var filters = new Dictionary<string, string>();
        
        // Simple parsing: look for | followed by key="value" or key=value
        var parts = openTag.Split('|');
        for (int i = 1; i < parts.Length; i++) // Skip the first part "{{posts"
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
            // Process conditional sections FIRST, before variable substitution
            var postHtml = ProcessConditionalSections(innerTemplate, post);
            
            // Generate URL path based on category: /category/slug.html
            var urlPath = !string.IsNullOrEmpty(post.Category)
                ? $"/{post.Category.ToLowerInvariant()}/{post.Slug}.html"
                : $"/uncategorized/{post.Slug}.html";
                
            postHtml = postHtml
                .Replace("{{url}}", urlPath)
                .Replace("{{title}}", post.Title)
                .Replace("{{category}}", post.Category ?? "")
                .Replace("{{summary}}", post.Summary ?? "")
                .Replace("{{date}}", post.DatePublished.ToString("MMMM dd, yyyy"));
            
            // Process custom fields from frontmatter
            if (post.CustomFields != null)
            {
                foreach (var field in post.CustomFields)
                {
                    postHtml = postHtml.Replace($"{{{{{field.Key}}}}}", field.Value);
                }
            }
            
            generatedHtml.AppendLine(postHtml);
        }
        
        return generatedHtml.ToString();
    }
    
    /// <summary>
    /// Processes conditional template sections like {{#author}}...{{/author}}
    /// </summary>
    private static string ProcessConditionalSections(string template, ContentItem post)
    {
        var result = template;
        
        // FIRST: Process {{if field_exists fieldname}} conditionals
        result = ProcessFieldExistsConditionals(result, post);
        
        // THEN: Process custom field conditionals ({{#fieldname}})
        if (post.CustomFields != null)
        {
            foreach (var field in post.CustomFields)
            {
                // Use 4 braces to output 2 braces in interpolated strings
                // $"{{{{" becomes "{{"
                var startTag = $"{{{{#{field.Key}}}}}";
                var endTag = $"{{{{/{field.Key}}}}}";
                
                if (!string.IsNullOrWhiteSpace(field.Value))
                {
                    // Field has value - remove the conditional tags but keep content
                    result = result.Replace(startTag, "").Replace(endTag, "");
                }
                else
                {
                    // Field is empty - remove entire conditional section
                    result = RemoveConditionalSection(result, startTag, endTag);
                }
            }
        }
        
        // Process standard field conditionals
        result = ProcessStandardConditionals(result, post);
        
        return result;
    }
    
    /// <summary>
    /// Processes {{if field_exists fieldname}}...{{/if}} conditionals.
    /// These check if a field exists in CustomFields, regardless of its value.
    /// </summary>
    private static string ProcessFieldExistsConditionals(string template, ContentItem post)
    {
        var result = template;
        const string pattern = "{{if field_exists ";
        
        while (true)
        {
            var startIndex = result.IndexOf(pattern);
            if (startIndex == -1) break;
            
            // Find the end of the opening tag to extract field name
            var openTagEnd = result.IndexOf("}}", startIndex);
            if (openTagEnd == -1) break;
            
            // Extract field name from "{{if field_exists fieldname}}"
            var fieldNameStart = startIndex + pattern.Length;
            var fieldName = result.Substring(fieldNameStart, openTagEnd - fieldNameStart).Trim();
            
            // Find the closing {{/if}} tag
            const string closingTag = "{{/if}}";
            var closingIndex = result.IndexOf(closingTag, openTagEnd);
            if (closingIndex == -1) break;
            
            // Extract the content between the tags
            var contentStart = openTagEnd + 2;
            var content = result.Substring(contentStart, closingIndex - contentStart);
            
            // Check if field exists in CustomFields
            var fieldExists = post.CustomFields != null && post.CustomFields.ContainsKey(fieldName);
            
            // Calculate what to replace
            var fullSection = result.Substring(startIndex, closingIndex + closingTag.Length - startIndex);
            
            if (fieldExists)
            {
                // Field exists - replace entire conditional with just the content
                result = result.Replace(fullSection, content);
            }
            else
            {
                // Field doesn't exist - remove entire conditional section
                result = result.Replace(fullSection, "");
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// Processes conditionals for standard fields like summary, category, etc.
    /// </summary>
    private static string ProcessStandardConditionals(string template, ContentItem post)
    {
        var result = template;
        
        // Summary conditional
        if (!string.IsNullOrWhiteSpace(post.Summary))
        {
            result = result.Replace("{{#summary}}", "").Replace("{{/summary}}", "");
        }
        else
        {
            result = RemoveConditionalSection(result, "{{#summary}}", "{{/summary}}");
        }
        
        // Category conditional
        if (!string.IsNullOrWhiteSpace(post.Category))
        {
            result = result.Replace("{{#category}}", "").Replace("{{/category}}", "");
        }
        else
        {
            result = RemoveConditionalSection(result, "{{#category}}", "{{/category}}");
        }
        
        // Date conditional
        if (post.DatePublished != default)
        {
            result = result.Replace("{{#date}}", "").Replace("{{/date}}", "");
        }
        else
        {
            result = RemoveConditionalSection(result, "{{#date}}", "{{/date}}");
        }
        
        return result;
    }
    
    /// <summary>
    /// Removes a conditional section and its content if the condition is false
    /// </summary>
    private static string RemoveConditionalSection(string text, string startTag, string endTag)
    {
        var startIndex = text.IndexOf(startTag);
        if (startIndex == -1) return text;
        
        var endIndex = text.IndexOf(endTag, startIndex);
        if (endIndex == -1) return text;
        
        var sectionLength = endIndex + endTag.Length - startIndex;
        return text.Remove(startIndex, sectionLength);
    }
}