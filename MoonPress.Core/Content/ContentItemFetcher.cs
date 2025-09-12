using System.Globalization;
using System.Text.RegularExpressions;
using MoonPress.Core.Models;

namespace MoonPress.Core.Content;

public static class ContentItemFetcher
{
    private const string PublishedDateFormat = "yyyy-MM-dd HH:mm:ss";
    private const string ContentFolderName = "content";
    private const string PagesFolderName = "pages";

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

            var contentDirectory = Path.Combine(rootFolder, ContentFolderName);
            if (Directory.Exists(contentDirectory))
            {
                // Scan all markdown files in content directory and subdirectories
                var contentFiles = Directory
                    .EnumerateFiles(contentDirectory, "*.md", SearchOption.AllDirectories);

                foreach (var file in contentFiles)
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

            string yamlContent;
            string bodyContent;

            // Check if it's fenced YAML (---...---)
            var fencedYamlMatch = Regex.Match(content, @"^---\s*(.*?)\s*---", RegexOptions.Singleline);
            if (fencedYamlMatch.Success)
            {
                yamlContent = fencedYamlMatch.Groups[1].Value;
                var contentStartIndex = fencedYamlMatch.Index + fencedYamlMatch.Length;
                bodyContent = content.Substring(contentStartIndex).Trim();
            }
            else
            {
                // Handle simple key-value format at start of file
                var lines = content.Split('\n');
                var yamlLines = new List<string>();
                var bodyLines = new List<string>();
                bool inMetadata = true;

                foreach (var line in lines)
                {
                    if (inMetadata && Regex.IsMatch(line.Trim(), @"^[A-Za-z_][A-Za-z0-9_]*\s*:"))
                    {
                        yamlLines.Add(line);
                    }
                    else if (inMetadata && string.IsNullOrWhiteSpace(line.Trim()))
                    {
                        // Empty line might separate metadata from content
                        continue;
                    }
                    else
                    {
                        inMetadata = false;
                        bodyLines.Add(line);
                    }
                }

                yamlContent = string.Join('\n', yamlLines);
                bodyContent = string.Join('\n', bodyLines).Trim();
            }

            // If no YAML content found, return null
            if (string.IsNullOrWhiteSpace(yamlContent))
            {
                return null;
            }

            // Extract metadata from YAML - support both formats
            var datePublishedStr = ExtractYamlValue(yamlContent, "datePublished") 
                                 ?? ExtractYamlValue(yamlContent, "Date");
            var dateUpdatedStr = ExtractYamlValue(yamlContent, "dateUpdated");
            var isDraftStr = ExtractYamlValue(yamlContent, "isDraft");

            // Parse datePublished - try multiple formats
            if (!DateTime.TryParseExact(
                datePublishedStr,
                PublishedDateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var datePublished))
            {
                // Try simple date format like "2025-04-29"
                DateTime.TryParseExact(
                    datePublishedStr,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out datePublished);
            }

            DateTime.TryParseExact(
                dateUpdatedStr,
                PublishedDateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var dateUpdated);

            // Parse draft status
            var isDraft = bool.TryParse(isDraftStr, out var draft) && draft;

            // Extract all YAML key-value pairs for custom fields
            var customFields = new Dictionary<string, string>();
            var metadataLines = yamlContent.Split('\n');
            var knownKeys = new HashSet<string>
            {
                "id", "title", "slug", "datePublished", "dateUpdated", "category", "tags", "isDraft", "summary", "date", "save_as", "template"
            };
            foreach (var line in metadataLines)
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
                Id = ExtractYamlValue(yamlContent, "id") ?? Path.GetFileNameWithoutExtension(filePath),
                FilePath = filePath,
                Title = ExtractYamlValue(yamlContent, "title") ?? ExtractYamlValue(yamlContent, "Title") ?? "Untitled",
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
            var slugValue = ExtractYamlValue(yamlContent, "slug") ?? ExtractYamlValue(yamlContent, "Slug");
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
