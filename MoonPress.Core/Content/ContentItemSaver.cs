using MoonPress.Core.Models;
using MoonPress.Core.Renderers;

namespace MoonPress.Core.Content;

public static class ContentItemSaver
{
    public static async Task SaveContentItem(ContentItem item, IMarkdownRenderer renderer, string rootFolder)
    {
        if (string.IsNullOrWhiteSpace(rootFolder))
        {
            throw new ArgumentException("Root folder cannot be null or empty.", nameof(rootFolder));
        }

        if (item == null)
        {
            throw new ArgumentNullException("Can't save non-existent content.", nameof(item));
        }

        try
        {
            // Population of items that appear on first save
            if (string.IsNullOrEmpty(item.Id))
            {
                item.Id = Guid.NewGuid().ToString();
            }
            if (item.DatePublished == DateTime.MinValue)
            {
                item.DatePublished = DateTime.UtcNow;
            }
            
            // Sanitize the title to create a valid file name
            var fileName = $"{item.Title.Replace(" ", "-")}.md";
            var filePath = Path.Combine(rootFolder, "Content");
            if (string.IsNullOrWhiteSpace(item.FilePath))
            {
                item.FilePath = Path.Combine(filePath, fileName);
            }

            item.DateUpdated = DateTime.UtcNow;

            // Create the directory if it doesn't exist
            Directory.CreateDirectory(filePath);            

            // Write the content to the file
            var markdown = renderer.RenderMarkdown(item);
            await File.WriteAllTextAsync(Path.Combine(filePath, fileName), markdown);
            ContentItemFetcher.UpdateCache(item);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving content: {ex.Message}");
        }
    }
}
