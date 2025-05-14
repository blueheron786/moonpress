using Microsoft.AspNetCore.Components;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;
using MoonPress.Core.Models;

namespace MoonPress.BlazorDesktop.Components.Pages.Content;

public partial class ContentItems : ComponentBase
{
    private const string PublishedDateFormat = "yyyy-MM-dd HH:mm:ss";

    [Inject]
    protected NavigationManager? Nav { get; set; }

    private List<ContentItem> ContentItemsList { get; set; } = new();

    protected override void OnInitialized()
    {
        if (ProjectState.Current is not null)
        {
            var pagesDirectory = Path.Combine(ProjectState.Current.RootFolder, "content");
            if (Directory.Exists(pagesDirectory))
            {
                var files = Directory
                    .EnumerateFiles(pagesDirectory, "*.md", SearchOption.AllDirectories);

                foreach (var file in files)
                {
                    var contentItem = ParseContentItem(file);
                    if (contentItem != null)
                    {
                        ContentItemsList.Add(contentItem);
                    }
                }

                // Sort by datePublished DESC
                ContentItemsList = ContentItemsList
                    .OrderByDescending(item => item.DatePublished)
                    .ToList();
            }
        }
    }

    private ContentItem? ParseContentItem(string filePath)
    {
        try
        {
            var content = File.ReadAllText(filePath);

            // Extract YAML front matter using regex
            var yamlMatch = Regex.Match(content, @"^---\s*(.*?)\s*---", RegexOptions.Singleline);
            if (!yamlMatch.Success) return null;

            var yamlContent = yamlMatch.Groups[1].Value;

            // Extract metadata from YAML
            var title = ExtractYamlValue(yamlContent, "title");
            var datePublishedStr = ExtractYamlValue(yamlContent, "datePublished");
            var isDraftStr = ExtractYamlValue(yamlContent, "isDraft");

            // Parse datePublished
            DateTime.TryParseExact(
                datePublishedStr,
                PublishedDateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var datePublished);

            // Parse draft status
            var isDraft = bool.TryParse(isDraftStr, out var draft) && draft;

            return new ContentItem
            {
                FilePath = filePath,
                Title = title ?? "Untitled",
                DatePublished = datePublished,
                IsDraft = isDraft
            };
        }
        catch
        {
            // Handle parsing errors (e.g., log them, show them to the user, etc.)
            return null;
        }
    }

    private string? ExtractYamlValue(string yamlContent, string key)
    {
        var match = Regex.Match(yamlContent, $@"^{key}:\s*(.+)$", RegexOptions.Multiline);
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }

    protected void GoToNewContentItem() => Nav.NavigateTo("/content-item/new");
}