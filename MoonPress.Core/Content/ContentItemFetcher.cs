using System.Globalization;
using System.Text.RegularExpressions;
using MoonPress.Core.Models;

namespace MoonPress.Core.Content;

public static class ContentItemFetcher
{
    private const string PublishedDateFormat = "yyyy-MM-dd HH:mm:ss";
    private const string ContentFolderName = "content";

    // Cache for content items. ID => item
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

            var pagesDirectory = Path.Combine(rootFolder, ContentFolderName);
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

    public static IEnumerable<string> GetCategories()
    {
        if (_contentItems == null)
        {
            throw new InvalidOperationException("Content items have not been loaded.");
        }

        return _contentItems.Values
            .Select(item => item.Category)
            .Distinct()
            .OrderBy(c => c);
    }

    public static Dictionary<string, ContentItem> GetCategoriesWithContentItems()
    {
        if (_contentItems == null)
        {
            throw new InvalidOperationException("Content items have not been loaded.");
        }

        return _contentItems.ToDictionary();
    }

    public static IEnumerable<string> GetTags()
    {
        if (_contentItems == null)
        {
            throw new InvalidOperationException("Content items have not been loaded.");
        }

        return _contentItems.Values
            .SelectMany(item => item.Tags.Split(','))
            .Select(tag => tag.Trim())
            .Distinct()
            .OrderBy(t => t);
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

    public static Dictionary<string, List<ContentItem>> GetItemsByCategory()
    {
        if (_contentItems == null)
        {
            throw new InvalidOperationException("Content items have not been loaded.");
        }

        return _contentItems.Values
            .GroupBy(item => item.Category ?? string.Empty)
            .ToDictionary(
                g => g.Key,
                g => g.ToList()
            );
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
            var datePublishedStr = ExtractYamlValue(yamlContent, "datePublished");
            var dateUpdatedStr = ExtractYamlValue(yamlContent, "dateUpdated");
            var isDraftStr = ExtractYamlValue(yamlContent, "isDraft");

            // Parse datePublished
            DateTime.TryParseExact(
                datePublishedStr,
                PublishedDateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var datePublished);

            DateTime.TryParseExact(
                dateUpdatedStr,
                PublishedDateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var dateUpdated);

            // Parse draft status
            var isDraft = bool.TryParse(isDraftStr, out var draft) && draft;

            // Extract content after the YAML front matter
            var contentStartIndex = yamlMatch.Index + yamlMatch.Length;
            var bodyContent = content.Substring(contentStartIndex).Trim();

            // Extract all YAML key-value pairs for custom fields
            var customFields = new Dictionary<string, string>();
            var yamlLines = yamlContent.Split('\n');
            var knownKeys = new HashSet<string>
            {
                "id", "title", "slug", "datePublished", "dateUpdated", "category", "tags", "isDraft", "summary"
            };
            foreach (var line in yamlLines)
            {
                var match = Regex.Match(line, @"^(?<key>[^:]+):\s*(?<value>.+)$");
                if (match.Success)
                {
                    var key = match.Groups["key"].Value.Trim();
                    var value = match.Groups["value"].Value.Trim().Trim('"');
                    if (!knownKeys.Contains(key))
                    {
                        customFields[key] = value;
                    }
                }
            }

            var contentItem = new ContentItem
            {
                Id = ExtractYamlValue(yamlContent, "id")!,
                FilePath = filePath,
                Title = ExtractYamlValue(yamlContent, "title") ?? "Untitled",
                DatePublished = datePublished,
                DateUpdated = dateUpdated,
                Category = ExtractYamlValue(yamlContent, "category") ?? string.Empty,
                Tags = ExtractYamlValue(yamlContent, "tags") ?? string.Empty,
                IsDraft = isDraft,
                Summary = ExtractYamlValue(yamlContent, "summary"),
                Contents = bodyContent,
                CustomFields = customFields
            };
            
            // Set slug through the property (after object initialization)
            var slugValue = ExtractYamlValue(yamlContent, "slug");
            if (!string.IsNullOrWhiteSpace(slugValue))
            {
                contentItem.Slug = slugValue;
            }
            
            return contentItem;
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
        if (!match.Success) return null;
        var value = match.Groups[1].Value.Trim();
        // Remove surrounding quotes if present
        if ((value.StartsWith("\"") && value.EndsWith("\"")) || (value.StartsWith("'") && value.EndsWith("'")))
        {
            value = value.Substring(1, value.Length - 2);
        }
        return value;
    }
}
