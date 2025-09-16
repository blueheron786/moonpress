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

    public async Task GenerateContentPagesAsync(List<ContentItem> contentItems, string outputPath, string themeLayout, SiteGenerationResult result)
    {
        var navbar = GenerateNavbar(contentItems);
        
        // Process posts blocks in the theme layout once, before applying to individual pages
        var processedThemeLayout = _postsProcessor.ProcessPostsBlocks(themeLayout, contentItems);
        
        foreach (var item in contentItems.Where(i => !i.IsDraft))
        {
            try
            {
                var contentHtml = _htmlRenderer.RenderHtml(item);
                
                // Process posts filters in the content HTML
                contentHtml = _postsProcessor.ProcessPostsBlocks(contentHtml, contentItems);
                
                var html = ApplyThemeLayout(processedThemeLayout, item.Title, contentHtml, navbar);
                
                // Determine output path based on content type
                string fileName;
                string fullOutputPath;
                
                if (IsFromPostsDirectory(item.FilePath))
                {
                    // Posts go in /<category>/<slug>.html
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
                else if (IsFromPagesDirectory(item.FilePath))
                {
                    // Pages go in /<slug>.html
                    fileName = $"{item.Slug}.html";
                    fullOutputPath = Path.Combine(outputPath, fileName);
                    result.GeneratedFiles.Add(fileName);
                }
                else
                {
                    // Default behavior for other content
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
            var href = !string.IsNullOrEmpty(page.Slug) ? $"{page.Slug}.html" : $"{page.Title.ToLowerInvariant().Replace(" ", "-")}.html";
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

    private static string ApplyThemeLayout(string layout, string title, string content, string navbar = "")
    {
        return layout
            .Replace("{{ title }}", title)
            .Replace("{{ content }}", content)
            .Replace("{{ navbar }}", navbar);
    }
}