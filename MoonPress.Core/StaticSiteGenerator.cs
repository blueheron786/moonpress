using System.Text;
using MoonPress.Core.Content;
using MoonPress.Core.Models;
using MoonPress.Core.Renderers;
using MoonPress.Core.Templates;
using MoonPress.Core.Generators;

namespace MoonPress.Core;

/// <summary>
/// Orchestrates the complete static site generation process using SOLID principles
/// </summary>
public class StaticSiteGenerator
{
    private readonly ContentPageGenerator _contentPageGenerator;
    private readonly CategoryPageGenerator _categoryPageGenerator;
    private readonly IndexPageGenerator _indexPageGenerator;
    private readonly AssetCopier _assetCopier;
    private readonly ThemeLayoutLoader _themeLayoutLoader;
    
    public StaticSiteGenerator(IHtmlRenderer htmlRenderer, PostsTemplateProcessor? postsProcessor = null)
    {
        if (htmlRenderer == null) throw new ArgumentNullException(nameof(htmlRenderer));
        
        var processor = postsProcessor ?? new PostsTemplateProcessor();
        
        _contentPageGenerator = new ContentPageGenerator(htmlRenderer, processor);
        _categoryPageGenerator = new CategoryPageGenerator(processor);
        _indexPageGenerator = new IndexPageGenerator(processor);
        _assetCopier = new AssetCopier();
        _themeLayoutLoader = new ThemeLayoutLoader();
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
            var themeLayoutResult = await _themeLayoutLoader.LoadThemeLayoutAsync(project);
            if (!themeLayoutResult.Success)
            {
                result.Success = false;
                result.EndTime = DateTime.UtcNow;
                result.Message = themeLayoutResult.ErrorMessage;
                return result;
            }
            var themeLayout = themeLayoutResult.Layout;
            
            // Load all content items
            var contentItemsDict = ContentItemFetcher.GetContentItems(project.RootFolder);
            var contentItems = contentItemsDict.Values.ToList();
            
            // Generate HTML pages for each content item
            await _contentPageGenerator.GenerateContentPagesAsync(contentItems, outputPath, themeLayout, result);
            
            // Generate category pages
            var themePath = Path.Combine(project.RootFolder, "themes", project.Theme);
            await _categoryPageGenerator.GenerateCategoryPagesAsync(contentItems, outputPath, themeLayout, result, themePath);
            
            // Generate index page
            await _indexPageGenerator.GenerateIndexPageAsync(contentItems, outputPath, themeLayout, project, result);
            
            // Copy theme assets
            await _assetCopier.CopyThemeAssetsAsync(project, outputPath, result);
            
            // Copy static assets (if any)
            await _assetCopier.CopyStaticAssetsAsync(project.RootFolder, outputPath, result);
            
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
            // Clean existing output directory, preserving file handle on directory.
            // This is incase there's a web-server running on that directory.
            foreach (var item in Directory.GetFileSystemEntries(outputPath))
            {
                if (Directory.Exists(item))
                {
                    Directory.Delete(item, true);
                }
                else
                {
                    File.Delete(item);
                }
            }
        }
        
        Directory.CreateDirectory(outputPath);
        await Task.CompletedTask;
    }
}