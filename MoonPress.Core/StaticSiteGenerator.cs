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
            
            // Load all content items
            var contentItemsDict = ContentItemFetcher.GetContentItems(project.RootFolder);
            var contentItems = contentItemsDict.Values.ToList();
            
            // Generate HTML pages for each content item
            await GenerateContentPagesAsync(contentItems, outputPath, result);
            
            // Generate index page
            await GenerateIndexPageAsync(contentItems, outputPath, result);
            
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

    private async Task GenerateContentPagesAsync(List<ContentItem> contentItems, string outputPath, SiteGenerationResult result)
    {
        foreach (var item in contentItems.Where(i => !i.IsDraft))
        {
            try
            {
                var html = _htmlRenderer.RenderHtml(item);
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

    private async Task GenerateIndexPageAsync(List<ContentItem> contentItems, string outputPath, SiteGenerationResult result)
    {
        var publishedItems = contentItems
            .Where(i => !i.IsDraft)
            .OrderByDescending(i => i.DatePublished)
            .ToList();

        var indexHtml = GenerateIndexHtml(publishedItems);
        var indexPath = Path.Combine(outputPath, "index.html");
        
        await File.WriteAllTextAsync(indexPath, indexHtml);
        result.PagesGenerated++;
        result.GeneratedFiles.Add("index.html");
    }

    private static string GenerateIndexHtml(List<ContentItem> contentItems)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"en\">");
        sb.AppendLine("<head>");
        sb.AppendLine("    <meta charset=\"utf-8\" />");
        sb.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\" />");
        sb.AppendLine("    <title>Site Index</title>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine("    <h1>Site Index</h1>");
        
        if (contentItems.Any())
        {
            sb.AppendLine("    <ul>");
            foreach (var item in contentItems)
            {
                sb.AppendLine($"        <li>");
                sb.AppendLine($"            <a href=\"{item.Slug}.html\">{item.Title}</a>");
                sb.AppendLine($"            <span> - {item.DatePublished:yyyy-MM-dd}</span>");
                if (!string.IsNullOrWhiteSpace(item.Summary))
                {
                    sb.AppendLine($"            <p>{item.Summary}</p>");
                }
                sb.AppendLine($"        </li>");
            }
            sb.AppendLine("    </ul>");
        }
        else
        {
            sb.AppendLine("    <p>No content available.</p>");
        }
        
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");
        
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
