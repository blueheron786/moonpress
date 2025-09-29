using System.Text;
using MoonPress.Core.Models;
using MoonPress.Core.Templates;
using MoonPress.Core.Renderers;

namespace MoonPress.Core.Generators;

/// <summary>
/// Responsible for generating the index page
/// </summary>
public class IndexPageGenerator
{
    private const string ThemesFolderName = "themes";
    private readonly PostsTemplateProcessor _postsProcessor;
    private readonly IHtmlRenderer? _htmlRenderer;

    public IndexPageGenerator(PostsTemplateProcessor? postsProcessor = null, IHtmlRenderer? htmlRenderer = null)
    {
        _postsProcessor = postsProcessor ?? new PostsTemplateProcessor();
        _htmlRenderer = htmlRenderer;
    }

    public async Task GenerateIndexPageAsync(List<ContentItem> contentItems, string outputPath, string themeLayout, StaticSiteProject project, SiteGenerationResult result)
    {
        var publishedItems = contentItems
            .Where(i => !i.IsDraft)
            .OrderByDescending(i => i.DatePublished)
            .ToList();

        var navbar = GenerateNavbar(contentItems);
        var indexContentHtml = await GenerateIndexContentHtmlAsync(publishedItems, project);
        
        // First, process posts blocks in the theme layout BEFORE applying it
        var processedThemeLayout = _postsProcessor.ProcessPostsBlocks(themeLayout, contentItems);
        
        var indexHtml = ApplyThemeLayout(processedThemeLayout, "Site Index", indexContentHtml, navbar);
        
        var indexPath = Path.Combine(outputPath, "index.html");
        
        await File.WriteAllTextAsync(indexPath, indexHtml);
        result.PagesGenerated++;
        result.GeneratedFiles.Add("index.html");
    }

    private async Task<string> GenerateIndexContentHtmlAsync(List<ContentItem> contentItems, StaticSiteProject project)
    {
        // Load the index template
        var indexTemplatePath = Path.Combine(project.RootFolder, ThemesFolderName, project.Theme, "index.html");
        string indexTemplate;
        
        if (File.Exists(indexTemplatePath))
        {
            indexTemplate = await File.ReadAllTextAsync(indexTemplatePath);
        }
        else
        {
            // Fallback template
            indexTemplate = @"{{ articles_section }}";
        }

        // Process posts filters first
        indexTemplate = _postsProcessor.ProcessPostsBlocks(indexTemplate, contentItems);

        // Process markdown in the template if we have an HTML renderer
        if (_htmlRenderer != null)
        {
            // Create a temporary content item to use the markdown processor
            var tempContentItem = new ContentItem
            {
                Contents = indexTemplate,
                Title = "Index",
                DatePublished = DateTime.UtcNow
            };
            
            // Use the HTML renderer to convert markdown to HTML, then extract just the content
            var fullHtml = _htmlRenderer.RenderHtml(tempContentItem);
            
            // Extract just the content inside the <div class="content"> tags
            var contentStart = fullHtml.IndexOf("<div class=\"content\">");
            if (contentStart != -1)
            {
                contentStart = fullHtml.IndexOf('>', contentStart) + 1;
                var contentEnd = fullHtml.LastIndexOf("</div>");
                
                if (contentEnd != -1 && contentEnd > contentStart)
                {
                    indexTemplate = fullHtml.Substring(contentStart, contentEnd - contentStart).Trim();
                }
            }
        }

        // Generate the articles section
        var articlesSection = GenerateArticlesSection(contentItems);
        
        // Replace template placeholders
        return indexTemplate.Replace("{{ articles_section }}", articlesSection);
    }

    private static string GenerateArticlesSection(List<ContentItem> contentItems)
    {
        if (!contentItems.Any())
        {
            return "<p>No content available.</p>";
        }

        var html = new StringBuilder();
        html.AppendLine("<section class=\"articles\">");
        html.AppendLine("    <h2>Latest Articles</h2>");
        html.AppendLine("    <ul>");

        foreach (var item in contentItems.Take(10)) // Show latest 10 items
        {
            var url = IsFromPostsDirectory(item.FilePath) 
                ? $"/blog/{item.Slug}.html" 
                : $"/{item.Slug}.html";
            
            html.AppendLine($"        <li>");
            html.AppendLine($"            <a href=\"{url}\">{item.Title}</a>");
            
            if (!string.IsNullOrWhiteSpace(item.Summary))
            {
                html.AppendLine($"            <p>{item.Summary}</p>");
            }
            
            html.AppendLine($"        </li>");
        }

        html.AppendLine("    </ul>");
        html.AppendLine("</section>");

        return html.ToString();
    }

    private static string GenerateNavbar(List<ContentItem> contentItems)
    {
        var pageItems = contentItems
            .Where(item => IsFromPagesDirectory(item.FilePath) && !item.IsDraft && item.Display)
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