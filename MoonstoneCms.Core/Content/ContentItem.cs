using System;
using System.Collections.Generic;

namespace MoonstoneCms.Core.Models;

public class ContentItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Slug => Title?.ToLower().Replace(' ', '-') ?? "";
    public string Contents { get; set; } = string.Empty;
    public bool IsDraft { get; set; }
    public List<string> Tags { get; set; } = new();
    public DateTime? DatePublished { get; set; }
}