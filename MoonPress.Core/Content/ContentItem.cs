using System.ComponentModel.DataAnnotations;

namespace MoonPress.Core.Models;

public class ContentItem
{
    // Infered, not stored in the file
    public string FilePath { get; set; } = string.Empty;

    public string Id { get; set; } // populated on first save
    public string Title { get; set; } = string.Empty;
    public string Contents { get; set; } = string.Empty;
    public bool IsDraft { get; set; }
    public List<string> Tags { get; set; } = new();
    public DateTime? DatePublished { get; set; } // also populated on first save

    /// <summary>
    /// Used to generate the OpenGraph og:description meta tag.
    /// </summary>
    [MaxLength(140, ErrorMessage = "Summary should be 140 characters or less to fit into og:description.")]
    public string? Summary { get; set; } = null;

    public string FileNameOnly => Path.GetFileName(FilePath).Replace(' ', '-');
    public string Slug => Title?.ToLower().Replace(' ', '-') ?? "";
    public string Status => IsDraft ? "Draft" : "Published";

    public ContentItem()
    {
        DatePublished = DateTime.UtcNow;
    }
}