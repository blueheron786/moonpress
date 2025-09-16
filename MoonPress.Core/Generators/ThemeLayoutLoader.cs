using MoonPress.Core.Models;

namespace MoonPress.Core.Generators;

/// <summary>
/// Responsible for loading and validating theme layouts
/// </summary>
public class ThemeLayoutLoader
{
    private const string ThemesFolderName = "themes";
    private const string ContentPlaceholder = "{{ content }}";
    private const string NavbarPlaceholder = "{{ navbar }}";
    private const string TitlePlaceholder = "{{ title }}";

    public async Task<ThemeLayoutResult> LoadThemeLayoutAsync(StaticSiteProject project)
    {
        var themeLayoutPath = Path.Combine(project.RootFolder, ThemesFolderName, project.Theme, "layout.html");
        
        Console.WriteLine($"Debug: Looking for theme layout at: {themeLayoutPath}");
        
        if (!File.Exists(themeLayoutPath))
        {
            return new ThemeLayoutResult
            {
                Success = false,
                ErrorMessage = $"Theme layout not found at: {themeLayoutPath}"
            };
        }

        try
        {
            var layout = await File.ReadAllTextAsync(themeLayoutPath);
            
            Console.WriteLine($"Debug: Layout HTML = {layout}");
            Console.WriteLine($"Debug: Looking for ContentPlaceholder = '{ContentPlaceholder}'");
            Console.WriteLine($"Debug: Contains ContentPlaceholder = {layout.Contains(ContentPlaceholder)}");
            
            // Validate that the theme contains all required placeholders
            var missingPlaceholders = new List<string>();
            
            if (!layout.Contains(ContentPlaceholder))
            {
                missingPlaceholders.Add(ContentPlaceholder);
            }
            
            if (!layout.Contains(NavbarPlaceholder))
            {
                missingPlaceholders.Add(NavbarPlaceholder);
            }
            
            if (!layout.Contains(TitlePlaceholder))
            {
                missingPlaceholders.Add(TitlePlaceholder);
            }
            
            if (missingPlaceholders.Any())
            {
                var placeholderList = string.Join(", ", missingPlaceholders);
                var placeholderWord = missingPlaceholders.Count == 1 ? "placeholder" : "placeholders";
                return new ThemeLayoutResult
                {
                    Success = false,
                    ErrorMessage = $"Theme layout '{project.Theme}/layout.html' is missing the required {placeholderWord}: {placeholderList}. Please add {(missingPlaceholders.Count == 1 ? "it" : "them")} to your theme layout."
                };
            }

            // Rewrite asset links to be flat (remove /themes/{theme}/ from href/src)
            var themePrefix = $"/themes/{project.Theme}/";
            layout = layout.Replace(themePrefix, "");
            // Also handle relative links like themes/{theme}/
            var relThemePrefix = $"themes/{project.Theme}/";
            layout = layout.Replace(relThemePrefix, "");

            return new ThemeLayoutResult
            {
                Success = true,
                Layout = layout
            };
        }
        catch (Exception ex)
        {
            return new ThemeLayoutResult
            {
                Success = false,
                ErrorMessage = $"Failed to load theme layout: {ex.Message}"
            };
        }
    }
}