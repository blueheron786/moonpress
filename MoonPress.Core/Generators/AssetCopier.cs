using MoonPress.Core.Models;

namespace MoonPress.Core.Generators;

/// <summary>
/// Responsible for copying theme assets and static files
/// </summary>
public class AssetCopier
{
    private const string ThemesFolderName = "themes";
    private const string StaticFolderName = "static";

    public async Task CopyThemeAssetsAsync(StaticSiteProject project, string outputPath, SiteGenerationResult result)
    {
        var themeFolder = Path.Combine(project.RootFolder, ThemesFolderName, project.Theme);
        
        if (!Directory.Exists(themeFolder))
        {
            return; // No theme assets to copy
        }

        // Get all files in theme folder except layout.html and index.html
        var themeFiles = Directory
            .EnumerateFiles(themeFolder, "*", SearchOption.AllDirectories)
            .Where(file => !Path.GetFileName(file).Equals("layout.html", StringComparison.OrdinalIgnoreCase) &&
                          !Path.GetFileName(file).Equals("index.html", StringComparison.OrdinalIgnoreCase));

        foreach (var themeFile in themeFiles)
        {
            try
            {
                var relativePath = Path.GetRelativePath(themeFolder, themeFile);
                var outputFile = Path.Combine(outputPath, relativePath);
                
                // Ensure the output directory exists
                var outputDir = Path.GetDirectoryName(outputFile);
                if (!string.IsNullOrEmpty(outputDir))
                {
                    Directory.CreateDirectory(outputDir);
                }
                
                await CopyFileAsync(themeFile, outputFile);
                result.GeneratedFiles.Add(relativePath);
                
                Console.WriteLine($"  -> Copied theme asset: {relativePath}");
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Failed to copy theme asset '{themeFile}': {ex.Message}");
            }
        }
    }

    public async Task CopyStaticAssetsAsync(string projectRootFolder, string outputPath, SiteGenerationResult result)
    {
        var staticFolder = Path.Combine(projectRootFolder, StaticFolderName);
        
        if (!Directory.Exists(staticFolder))
        {
            return; // No static assets to copy
        }

        var staticFiles = Directory.EnumerateFiles(staticFolder, "*", SearchOption.AllDirectories);

        foreach (var staticFile in staticFiles)
        {
            try
            {
                var relativePath = Path.GetRelativePath(staticFolder, staticFile);
                var outputFile = Path.Combine(outputPath, relativePath);
                
                // Ensure the output directory exists
                var outputDir = Path.GetDirectoryName(outputFile);
                if (!string.IsNullOrEmpty(outputDir))
                {
                    Directory.CreateDirectory(outputDir);
                }
                
                await CopyFileAsync(staticFile, outputFile);
                result.GeneratedFiles.Add(relativePath);
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Failed to copy static asset '{staticFile}': {ex.Message}");
            }
        }
    }

    private static async Task CopyFileAsync(string sourcePath, string destinationPath)
    {
        using var sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);
        using var destinationStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true);
        await sourceStream.CopyToAsync(destinationStream);
    }
}