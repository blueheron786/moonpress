using System.ComponentModel.DataAnnotations;

namespace MoonPress.Core.Models;

public class ContentItem
{
    public static string Sanitize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Convert to lowercase
        var sanitized = input.ToLowerInvariant();

        // Replace underscores with spaces for normalization
        sanitized = sanitized.Replace("_", " ");

        // Remove all non-alphanumeric characters except spaces
        sanitized = System.Text.RegularExpressions.Regex.Replace(sanitized, @"[^a-z0-9\s]", " ");

        // Replace all whitespace with single dash
        sanitized = System.Text.RegularExpressions.Regex.Replace(sanitized, @"\s+", "-");

        // Remove multiple dashes
        sanitized = System.Text.RegularExpressions.Regex.Replace(sanitized, @"-+", "-");

        // Trim leading/trailing dashes
        sanitized = sanitized.Trim('-');

        return sanitized;
    }

    // Inferred, not stored in the file
    public string FilePath { get; set; } = string.Empty;

    public string Id { get; set; } = string.Empty; // populated on first save
    public DateTime DatePublished { get; set; } // also populated on first save
    public DateTime DateUpdated { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Contents { get; set; } = string.Empty;
    public bool IsDraft { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Tags { get; set; } = string.Empty; // comma separated list of tags
                                                     // Custom key/value pairs
    public Dictionary<string, string> CustomFields { get; set; } = new();

    /// <summary>
    /// Used to generate the OpenGraph og:description meta tag.
    /// </summary>
    [MaxLength(140, ErrorMessage = "Summary should be 140 characters or less to fit into og:description.")]
    public string? Summary { get; set; } = null;

    public string FileNameOnly => Sanitize(Path.GetFileName(FilePath).Replace(".md", ""));
    
    private string _slug = string.Empty;
    public string Slug 
    { 
        get => string.IsNullOrWhiteSpace(_slug) ? Sanitize(Title?.ToLower()) ?? "" : _slug;
        set => _slug = value;
    }
    
    public string Status => IsDraft ? "Draft" : "Published";

    public ContentItem()
    {
        DatePublished = DateTime.UtcNow;
        DateUpdated = DatePublished;
    }
}