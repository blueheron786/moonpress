using System.ComponentModel.DataAnnotations;

namespace MoonPress.Core.Models;

public class ContentItem
{
    public static string Sanitize(string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }
        
        // Remove invalid characters and replace spaces with hyphens
        return new string(input
            .Trim()
            .ToLowerInvariant()
            .Replace(' ', '-')
            .Replace('|', '-')
            // them doubles are hard to kill ...
            .Replace("--", "-")
            .Replace("--", "-")
            .Replace("--", "-")
            .Where(c => char.IsLetterOrDigit(c) || c == '-' || c == '_')
            .ToArray());
    }

    // Inferred, not stored in the file
    public string FilePath { get; set; } = string.Empty;

    public string Id { get; set; } // populated on first save
    public string Title { get; set; } = string.Empty;
    public string Contents { get; set; } = string.Empty;
    public bool IsDraft { get; set; }
    public List<string> Tags { get; set; } = new();
    public DateTime DatePublished { get; set; } // also populated on first save

    /// <summary>
    /// Used to generate the OpenGraph og:description meta tag.
    /// </summary>
    [MaxLength(140, ErrorMessage = "Summary should be 140 characters or less to fit into og:description.")]
    public string? Summary { get; set; } = null;

    public string FileNameOnly => Sanitize(Path.GetFileName(FilePath).Replace(".md", ""));
    public string Slug => Sanitize(Title?.ToLower()) ?? "";
    public string Status => IsDraft ? "Draft" : "Published";

    public ContentItem()
    {
        DatePublished = DateTime.UtcNow;
    }
}