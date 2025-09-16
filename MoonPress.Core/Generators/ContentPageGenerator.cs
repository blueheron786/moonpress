using System.Text;
using MoonPress.Core.Content;
using MoonPress.Core.Models;
using MoonPress.Core.Renderers;
using MoonPress.Core.Templates;

namespace MoonPress.Core.Generators;

/// <summary>
/// Responsible for generating individual content pages (posts and pages)
/// </summary>
public class ContentPageGenerator
{
    private readonly IHtmlRenderer _htmlRenderer;
    private readonly PostsTemplateProcessor _postsProcessor;

    public ContentPageGenerator(IHtmlRenderer htmlRenderer, PostsTemplateProcessor? postsProcessor = null)
    {
        _htmlRenderer = htmlRenderer ?? throw new ArgumentNullException(nameof(htmlRenderer));
        _postsProcessor = postsProcessor ?? new PostsTemplateProcessor();
    }

    /// <summary>
    /// Renders only the markdown content of a ContentItem, without the auto-generated title and date
    /// Used for custom templates where you want full control over the layout
    /// </summary>
    private string RenderRawMarkdownContent(ContentItem contentItem)
    {
        // Use the existing HTML renderer but extract only the content div
        var fullHtml = _htmlRenderer.RenderHtml(contentItem);
        
        // Extract just the content inside the <div class="content"> tags
        var contentStart = fullHtml.IndexOf("<div class=\"content\">");
        if (contentStart == -1) return contentItem.Contents ?? string.Empty;
        
        contentStart = fullHtml.IndexOf('>', contentStart) + 1;
        var contentEnd = fullHtml.LastIndexOf("</div>");
        
        if (contentEnd == -1 || contentEnd <= contentStart) return contentItem.Contents ?? string.Empty;
        
        return fullHtml.Substring(contentStart, contentEnd - contentStart).Trim();
    }

    public async Task GenerateContentPagesAsync(List<ContentItem> contentItems, string outputPath, string themeLayout, SiteGenerationResult result, string? themePath = null)
    {
        var navbar = GenerateNavbar(contentItems);
        
        // Process posts blocks in the theme layout once, before applying to individual pages
        var processedThemeLayout = _postsProcessor.ProcessPostsBlocks(themeLayout, contentItems);
        
        foreach (var item in contentItems.Where(i => !i.IsDraft))
        {
            try
            {
                string contentHtml;
                
                // Check if this content item uses a custom template file
                var customTemplateName = item.CustomFields?.GetValueOrDefault("template");
                if (!string.IsNullOrWhiteSpace(customTemplateName) && !string.IsNullOrWhiteSpace(themePath))
                {
                    // Try to load custom template from theme/templates directory
                    var templatesDir = Path.Combine(themePath, "templates");
                    var customTemplatePath = Path.Combine(templatesDir, $"{customTemplateName}.html");
                    
                    if (File.Exists(customTemplatePath))
                    {
                        // Load and process custom template
                        var customTemplateContent = await File.ReadAllTextAsync(customTemplatePath);
                        
                        // For custom templates, render only the raw markdown content (without auto-generated title/date)
                        var pageContentHtml = RenderRawMarkdownContent(item);
                        customTemplateContent = customTemplateContent.Replace("{{content}}", pageContentHtml);
                        
                        // Process posts blocks first (this handles templates like books.html with {{posts}} sections)
                        customTemplateContent = _postsProcessor.ProcessPostsBlocks(customTemplateContent, contentItems);
                        
                        // Then process individual field variables (title, cover, buy_link, etc.)
                        // This handles templates like book.html for individual pages
                        contentHtml = _postsProcessor.ProcessSingleItemVariables(customTemplateContent, item);
                    }
                    else
                    {
                        // Fallback to standard rendering if custom template not found
                        contentHtml = _htmlRenderer.RenderHtml(item);
                        contentHtml = _postsProcessor.ProcessPostsBlocks(contentHtml, contentItems);
                    }
                }
                else
                {
                    // Use standard markdown rendering
                    contentHtml = _htmlRenderer.RenderHtml(item);
                    
                    // Process posts filters in the content HTML
                    contentHtml = _postsProcessor.ProcessPostsBlocks(contentHtml, contentItems);
                }
                
                var html = ApplyThemeLayout(processedThemeLayout, item.Title, contentHtml, navbar, item.DatePublished);
                
                // Determine output path based on content type
                string fileName;
                string fullOutputPath;
                
                if (IsFromPagesDirectory(item.FilePath))
                {
                    // Pages go in root: /<slug>.html
                    fileName = $"{item.Slug}.html";
                    fullOutputPath = Path.Combine(outputPath, fileName);
                    result.GeneratedFiles.Add(fileName);
                }
                else if (ShouldUseCategoryDirectory(item))
                {
                    // All other content (posts, books, etc.) go in /<category>/<slug>.html
                    var categoryDirectory = !string.IsNullOrEmpty(item.Category) 
                        ? Path.Combine(outputPath, item.Category.ToLowerInvariant())
                        : Path.Combine(outputPath, "uncategorized");
                    
                    Directory.CreateDirectory(categoryDirectory);
                    fileName = $"{item.Slug}.html";
                    fullOutputPath = Path.Combine(categoryDirectory, fileName);
                    
                    var relativePath = !string.IsNullOrEmpty(item.Category)
                        ? $"{item.Category.ToLowerInvariant()}/{fileName}"
                        : $"uncategorized/{fileName}";
                    
                    result.GeneratedFiles.Add(relativePath);
                }
                else
                {
                    // Default behavior for edge cases
                    fileName = $"{item.Slug}.html";
                    fullOutputPath = Path.Combine(outputPath, fileName);
                    result.GeneratedFiles.Add(fileName);
                }
                
                await File.WriteAllTextAsync(fullOutputPath, html);
                result.PagesGenerated++;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Failed to generate page for '{item.Title}': {ex.Message}");
            }
        }
    }

    private static string GenerateNavbar(List<ContentItem> contentItems)
    {
        var pageItems = contentItems
            .Where(item => IsFromPagesDirectory(item.FilePath) && !item.IsDraft)
            .OrderBy(item => item.Title)
            .ToList();

        if (!pageItems.Any())
        {
            return string.Empty;
        }

        var navbarHtml = new StringBuilder();
        foreach (var page in pageItems)
        {
            var href = !string.IsNullOrEmpty(page.Slug) ? $"/{page.Slug}.html" : $"/{page.Title.ToLowerInvariant().Replace(" ", "-")}.html";
            navbarHtml.AppendLine($"                <a href=\"{href}\" class=\"nav-link\">{page.Title}</a>");
        }

        return navbarHtml.ToString().TrimEnd();
    }

    private static bool IsFromPagesDirectory(string filePath)
    {
        return filePath.Contains(Path.Combine("content", "pages"));
    }

    private static bool IsFromPostsDirectory(string filePath)
    {
        return filePath.Contains(Path.Combine("content", "posts"));
    }

    private static bool ShouldUseCategoryDirectory(ContentItem item)
    {
        // All content except pages should use category-based directories
        return !IsFromPagesDirectory(item.FilePath);
    }

    private static string ApplyThemeLayout(string layout, string title, string content, string navbar = "", DateTime? date = null)
    {
        var result = layout
            .Replace("{{title}}", title)
            .Replace("{{content}}", content)
            .Replace("{{navbar}}", navbar);
            
        // Replace date token if a date is provided
        if (date.HasValue)
        {
            result = result.Replace("{{date}}", date.Value.ToString("MMMM dd, yyyy"));
        }
        
        return result;
    }
}