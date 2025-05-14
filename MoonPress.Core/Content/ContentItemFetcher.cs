using System.Globalization;
using System.Text.RegularExpressions;
using MoonPress.Core.Models;

namespace MoonPress.Core.Content;

public static class ContentItemFetcher
{
    private const string PublishedDateFormat = "yyyy-MM-dd HH:mm:ss";

    // Cache for content items
    private static Dictionary<string, ContentItem>? _contentItems;

    public static Dictionary<string, ContentItem> GetContentItems(string rootFolder)
    {
        if (string.IsNullOrWhiteSpace(rootFolder))
        {
            throw new ArgumentException("Root folder cannot be null or empty.", nameof(rootFolder));
        }
        if (!Directory.Exists(rootFolder))
        {
            throw new DirectoryNotFoundException($"The specified directory does not exist: {rootFolder}");
        }

        if (_contentItems == null || !_contentItems.Any())
        {

            _contentItems = new Dictionary<string, ContentItem>();

            var pagesDirectory = Path.Combine(rootFolder, "content");
            if (Directory.Exists(pagesDirectory))
            {
                var files = Directory
                    .EnumerateFiles(pagesDirectory, "*.md", SearchOption.AllDirectories);

                foreach (var file in files)
                {
                    var contentItem = ParseContentItem(file);
                    if (contentItem != null)
                    {
                        _contentItems[contentItem.Id] = contentItem;
                    }
                }
            }
        }

        return _contentItems;
    }

    public static void UpdateCache(ContentItem newOrExistingItem)
    {
                if (newOrExistingItem == null)
        {
            throw new ArgumentNullException(nameof(newOrExistingItem));
        }

        if (_contentItems == null)
        {
            _contentItems = new Dictionary<string, ContentItem>();
        }

        // Upsert
        _contentItems[newOrExistingItem.Id] = newOrExistingItem;
    }

    private static ContentItem? ParseContentItem(string filePath)
    {
        try
        {
            var content = File.ReadAllText(filePath);

            // Extract metadata using regex
            var yamlMatch = Regex.Match(content, @"^---\s*(.*?)\s*---", RegexOptions.Singleline);
            if (!yamlMatch.Success)
            {
                return null;
            }

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

            // Extract content after the YAML front matter
            var contentStartIndex = yamlMatch.Index + yamlMatch.Length;
            var bodyContent = content.Substring(contentStartIndex).Trim();

            return new ContentItem
            {
                FilePath = filePath,
                Title = title ?? "Untitled",
                DatePublished = datePublished,
                IsDraft = isDraft,
                Contents = bodyContent
            };
        }
        catch
        {
            // Handle parsing errors (e.g., log them, show them to the user, etc.)
            return null;
        }
    }

    private static string? ExtractYamlValue(string yamlContent, string key)
    {
        var match = Regex.Match(yamlContent, $@"^{key}:\s*(.+)$", RegexOptions.Multiline);
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }
}
