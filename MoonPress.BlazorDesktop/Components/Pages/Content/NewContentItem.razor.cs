using System.IO;
using System.Windows;
using Microsoft.AspNetCore.Components;
using MoonPress.Core.Models;

namespace MoonPress.BlazorDesktop.Components.Pages.Content;

public partial class NewContentItem
{
    private ContentItem _item = new();
    
    [Inject]
    private NavigationManager Nav { get; set; }

    private async Task Save()
    {
        if (string.IsNullOrWhiteSpace(ProjectState.Current!.RootFolder))
        {
            // Message to user: "There was an error saving the content. A copy of your updated
            // content is in the clipboard."
            Clipboard.SetText(_item.Contents);
            return;
        }

        try
        {
            var fileName = $"{_item.Title.Replace(" ", "-")}.md";
            var filePath = Path.Combine(ProjectState.Current!.RootFolder, "Content");

            // Create the directory if it doesn't exist
            Directory.CreateDirectory(filePath);

            // Prepare the Markdown content with YAML front matter
            var markdownContent = $"---\n" +
                $"title: {_item.Title}\n" +
                $"datePublished: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n" +
                $"isDraft: {_item.IsDraft.ToString().ToLower()}\n" +
                $"---\n\n" +
                $"{_item.Contents}";

            // Write the content to the file
            await File.WriteAllTextAsync(Path.Combine(filePath, fileName), markdownContent);

            Nav.NavigateTo("/content-items");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving content: {ex.Message}");
        }
    }

    void Cancel() => Nav.NavigateTo("/content-items");
}