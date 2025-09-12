using System.Text;
using MoonPress.Core.Content;
using MoonPress.Core.Models;
using MoonPress.Core.Renderers;

namespace MoonPress.Core;

/// <summary>
/// Generates the complete static site output from content items and project configuration
/// </summary>
public class StaticSiteGenerator
{
    private const string ThemesFolderName = "themes";
    private const string ContentFolderName = "content";
    private readonly IHtmlRenderer _htmlRenderer;
    
    public StaticSiteGenerator(IHtmlRenderer htmlRenderer)
    {
        _htmlRenderer = htmlRenderer ?? throw new ArgumentNullException(nameof(htmlRenderer));
    }

    /// <summary>
    /// Generates the complete static site to the output directory
    /// </summary>
    /// <param name="project">The project to generate</param>
    /// <param name="outputPath">The path where the generated site should be saved</param>
    /// <returns>A report of the generation process</returns>
    public async Task<SiteGenerationResult> GenerateSiteAsync(StaticSiteProject project, string outputPath)
    {
        if (project == null) throw new ArgumentNullException(nameof(project));
        if (string.IsNullOrWhiteSpace(outputPath)) throw new ArgumentException("Output path cannot be empty", nameof(outputPath));

        var result = new SiteGenerationResult { StartTime = DateTime.UtcNow };
        
        try
        {
            // Ensure output directory exists and is clean
            await PrepareOutputDirectoryAsync(outputPath);
            
            // Load theme template
            var themeLayout = await LoadThemeLayoutAsync(project);
            
            // Load all content items
            var contentItemsDict = ContentItemFetcher.GetContentItems(project.RootFolder);
            var contentItems = contentItemsDict.Values.ToList();
            
            // Generate HTML pages for each content item
            await GenerateContentPagesAsync(contentItems, outputPath, themeLayout, result);
            
            // Generate index page
            await GenerateIndexPageAsync(contentItems, outputPath, themeLayout, project, result);
            
            // Copy theme assets
            await CopyThemeAssetsAsync(project, outputPath, result);
            
            // Copy static assets (if any)
            await CopyStaticAssetsAsync(project.RootFolder, outputPath, result);
            
            result.Success = true;
            result.EndTime = DateTime.UtcNow;
            result.Message = $"Successfully generated {result.PagesGenerated} pages in {result.Duration.TotalSeconds:F2} seconds";
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.EndTime = DateTime.UtcNow;
            result.Message = $"Site generation failed: {ex.Message}";
            result.Error = ex;
        }

        return result;
    }

    private static async Task PrepareOutputDirectoryAsync(string outputPath)
    {
        if (Directory.Exists(outputPath))
        {
            // Clean existing output directory
            Directory.Delete(outputPath, true);
        }
        
        Directory.CreateDirectory(outputPath);
        await Task.CompletedTask;
    }

    private async Task GenerateContentPagesAsync(List<ContentItem> contentItems, string outputPath, string themeLayout, SiteGenerationResult result)
    {
        var navbar = GenerateNavbar(contentItems);
        
        foreach (var item in contentItems.Where(i => !i.IsDraft))
        {
            try
            {
                var contentHtml = _htmlRenderer.RenderHtml(item);
                var html = ApplyThemeLayout(themeLayout, item.Title, contentHtml, navbar);
                var fileName = $"{item.Slug}.html";
                var filePath = Path.Combine(outputPath, fileName);
                
                await File.WriteAllTextAsync(filePath, html);
                result.PagesGenerated++;
                result.GeneratedFiles.Add(fileName);
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Failed to generate page for '{item.Title}': {ex.Message}");
            }
        }
    }

    private async Task GenerateIndexPageAsync(List<ContentItem> contentItems, string outputPath, string themeLayout, StaticSiteProject project, SiteGenerationResult result)
    {
        var publishedItems = contentItems
            .Where(i => !i.IsDraft)
            .OrderByDescending(i => i.DatePublished)
            .ToList();

        var navbar = GenerateNavbar(contentItems);
        var indexContentHtml = await GenerateIndexContentHtmlAsync(publishedItems, project);
        var indexHtml = ApplyThemeLayout(themeLayout, "Site Index", indexContentHtml, navbar);
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
            indexTemplate = @"    <div class=""text-center mb-4"">
        <h1 class=""text-gradient"">Welcome to MoonPress</h1>
        <p>A modern, beautiful static site generator</p>
    </div>
    
    {{ARTICLES_SECTION}}";
        }

        // Generate the articles section
        var articlesSection = GenerateArticlesSectionHtml(contentItems);
        
        // Replace the placeholder
        return indexTemplate.Replace("{{ARTICLES_SECTION}}", articlesSection);
    }

    private static string GenerateArticlesSectionHtml(List<ContentItem> contentItems)
    {
        var sb = new StringBuilder();
        
        if (contentItems.Any())
        {
            sb.AppendLine("    <div class=\"py-8\">");
            sb.AppendLine("        <h2>Latest Articles</h2>");
            foreach (var item in contentItems)
            {
                sb.AppendLine("        <div class=\"article-card\">");
                sb.AppendLine($"            <h3><a href=\"{item.Slug}.html\">{item.Title}</a></h3>");
                sb.AppendLine($"            <div class=\"date\">Published on {item.DatePublished:MMMM dd, yyyy}</div>");
                if (!string.IsNullOrWhiteSpace(item.Summary))
                {
                    sb.AppendLine($"            <p>{item.Summary}</p>");
                }
                sb.AppendLine($"            <a href=\"{item.Slug}.html\">Read More →</a>");
                sb.AppendLine("        </div>");
            }
            sb.AppendLine("    </div>");
        }
        else
        {
            sb.AppendLine("    <div class=\"text-center py-8\">");
            sb.AppendLine("        <h2>Welcome to Your New Site!</h2>");
            sb.AppendLine("        <p>No content available yet. Create your first article to get started.</p>");
            sb.AppendLine("        <div class=\"mt-4\">");
            sb.AppendLine("            <p>🌙 Built with <strong>MoonPress</strong> - A modern static site generator</p>");
            sb.AppendLine("        </div>");
            sb.AppendLine("    </div>");
        }
        
        return sb.ToString();
    }

    private static async Task CopyStaticAssetsAsync(string projectRoot, string outputPath, SiteGenerationResult result)
    {
        // Look for common static asset directories
        var assetDirs = new[] { "css", "js", "images", "assets", "static" };
        
        foreach (var assetDir in assetDirs)
        {
            var sourcePath = Path.Combine(projectRoot, assetDir);
            if (Directory.Exists(sourcePath))
            {
                var targetPath = Path.Combine(outputPath, assetDir);
                await CopyDirectoryAsync(sourcePath, targetPath);
                result.GeneratedFiles.Add($"{assetDir}/ (directory)");
            }
        }
    }

    private static async Task CopyDirectoryAsync(string sourceDir, string targetDir)
    {
        Directory.CreateDirectory(targetDir);
        
        foreach (var file in Directory.GetFiles(sourceDir))
        {
            var fileName = Path.GetFileName(file);
            var targetFile = Path.Combine(targetDir, fileName);
            File.Copy(file, targetFile, true);
        }
        
        foreach (var subDir in Directory.GetDirectories(sourceDir))
        {
            var dirName = Path.GetFileName(subDir);
            var targetSubDir = Path.Combine(targetDir, dirName);
            await CopyDirectoryAsync(subDir, targetSubDir);
        }
    }

    private async Task<string> LoadThemeLayoutAsync(StaticSiteProject project)
    {
        var themePath = Path.Combine(project.RootFolder, ThemesFolderName, project.Theme, "layout.html");
        
        if (File.Exists(themePath))
        {
            string layoutHtml = await File.ReadAllTextAsync(themePath);
            // Rewrite asset links to be flat (remove /themes/{theme}/ from href/src)
            var themePrefix = $"/themes/{project.Theme}/";
            layoutHtml = layoutHtml.Replace(themePrefix, "");
            // Also handle relative links like themes/{theme}/
            var relThemePrefix = $"themes/{project.Theme}/";
            layoutHtml = layoutHtml.Replace(relThemePrefix, "");
            return layoutHtml;
        }
        
        // Fallback to a basic layout
        return @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""utf-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
    <title>{{TITLE}}</title>
    <link rel=""stylesheet"" href=""style.css"" />
</head>
<body>
    {{CONTENT}}
</body>
</html>";
    }

    private static string GenerateNavbar(List<ContentItem> contentItems)
    {
        var pageItems = contentItems
            .Where(item => item.Category.Equals("page", StringComparison.OrdinalIgnoreCase) && !item.IsDraft)
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

    private static string ApplyThemeLayout(string layout, string title, string content, string navbar = "")
    {
        return layout
            .Replace("{{TITLE}}", title)
            .Replace("{{CONTENT}}", content)
            .Replace("{{NAVBAR}}", navbar);
    }

    private async Task CopyThemeAssetsAsync(StaticSiteProject project, string outputPath, SiteGenerationResult result)
    {
        var themePath = Path.Combine(project.RootFolder, ThemesFolderName, project.Theme);

        if (!Directory.Exists(themePath))
        {
            result.Errors.Add($"Theme directory not found: {themePath}");
            return;
        }

        // Copy all theme files and folders (except .html) as flat files/folders into outputPath
        foreach (var file in Directory.GetFiles(themePath))
        {
            var fileName = Path.GetFileName(file);
            if (fileName.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
            {
                continue; // Skip template files
            }
            var targetFile = Path.Combine(outputPath, fileName);
            File.Copy(file, targetFile, true);
            result.GeneratedFiles.Add(fileName);
            Console.WriteLine($"  -> Copied theme asset: {fileName}");
        }

        // Copy all directories inside themePath as flat folders into outputPath
        foreach (var dir in Directory.GetDirectories(themePath))
        {
            var dirName = Path.GetFileName(dir);
            var targetDir = Path.Combine(outputPath, dirName);
            await CopyDirectoryAsync(dir, targetDir);
            result.GeneratedFiles.Add($"{dirName}/ (directory)");
            Console.WriteLine($"  -> Copied theme directory: {dirName}");
        }

        await Task.CompletedTask;
    }
}

/// <summary>
/// Result of a site generation operation
/// </summary>
public class SiteGenerationResult
{
    public bool Success { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan Duration => EndTime - StartTime;
    public string Message { get; set; } = string.Empty;
    public Exception? Error { get; set; }
    public int PagesGenerated { get; set; }
    public List<string> GeneratedFiles { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}
