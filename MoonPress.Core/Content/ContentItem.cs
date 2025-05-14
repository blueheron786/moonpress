namespace MoonPress.Core.Models;

public class ContentItem
{
    // Infered, not stored in the file
    public string FilePath { get; set; } = string.Empty;

    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Contents { get; set; } = string.Empty;
    public bool IsDraft { get; set; }
    public List<string> Tags { get; set; } = new();
    public DateTime? DatePublished { get; set; }

    public string FileNameOnly => Path.GetFileName(FilePath).Replace(' ', '-');
    public string Slug => Title?.ToLower().Replace(' ', '-') ?? "";
    public string Status => IsDraft ? "Draft" : "Published";
}