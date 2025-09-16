using System.Text;
using MoonPress.Core.Content;
using MoonPress.Core.Models;
using MoonPress.Core.Templates;

namespace MoonPress.Core.Generators;

/// <summary>
/// Responsible for generating category pages
/// </summary>
public class CategoryPageGenerator
{
    private readonly PostsTemplateProcessor _postsProcessor;

    public CategoryPageGenerator(PostsTemplateProcessor? postsProcessor = null)
    {
        _postsProcessor = postsProcessor ?? new PostsTemplateProcessor();
    }

    public async Task GenerateCategoryPagesAsync(List<ContentItem> contentItems, string outputPath, string themeLayout, SiteGenerationResult result)
    {
        var navbar = GenerateNavbar(contentItems);
        
        // Process posts blocks in the theme layout once, before applying to individual pages
        var processedThemeLayout = _postsProcessor.ProcessPostsBlocks(themeLayout, contentItems);
        
        // Get items grouped by category
        var itemsByCategory = ContentItemFetcher.GetItemsByCategory();
        
        // Create category directory
        var categoryDirectory = Path.Combine(outputPath, "category");
        Directory.CreateDirectory(categoryDirectory);
        
        foreach (var categoryGroup in itemsByCategory)
        {
            var categoryName = categoryGroup.Key;
            var categoryItems = categoryGroup.Value;
            
            // Skip empty category or categories with no published items
            if (string.IsNullOrWhiteSpace(categoryName) || !categoryItems.Any(i => !i.IsDraft))
            {
                continue;
            }
            
            try
            {
                // Generate category page content
                var categoryContentHtml = GenerateCategoryContentHtml(categoryName, categoryItems);
                
                var html = ApplyThemeLayout(processedThemeLayout, $"Category: {categoryName}", categoryContentHtml, navbar);
                
                // Sanitize category name for filename
                var sanitizedCategoryName = ContentItem.Sanitize(categoryName);
                var fileName = $"{sanitizedCategoryName}.html";
                var filePath = Path.Combine(categoryDirectory, fileName);
                
                await File.WriteAllTextAsync(filePath, html);
                result.PagesGenerated++;
                result.GeneratedFiles.Add($"category/{fileName}");
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Failed to generate category page for '{categoryName}': {ex.Message}");
            }
        }
    }

    private string GenerateCategoryContentHtml(string categoryName, List<ContentItem> categoryItems)
    {
        var publishedItems = categoryItems.Where(i => !i.IsDraft).OrderByDescending(i => i.DatePublished);
        
        var contentHtml = new StringBuilder();
        contentHtml.AppendLine($"<h1>Category: {categoryName}</h1>");
        contentHtml.AppendLine("<ul>");
        
        foreach (var item in publishedItems)
        {
            var itemUrl = IsFromPostsDirectory(item.FilePath) 
                ? $"/{item.Category?.ToLowerInvariant()}/{item.Slug}.html" 
                : $"/{item.Slug}.html";
                
            contentHtml.AppendLine($"  <li><a href=\"{itemUrl}\">{item.Title}</a>");
            
            if (!string.IsNullOrWhiteSpace(item.Summary))
            {
                contentHtml.AppendLine($" - <em>{item.Summary}</em>");
            }
            
            contentHtml.AppendLine("</li>");
        }
        
        contentHtml.AppendLine("</ul>");
        
        return contentHtml.ToString();
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