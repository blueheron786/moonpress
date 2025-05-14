using Microsoft.AspNetCore.Components;
using MoonPress.Core.Content;
using MoonPress.Core.Models;
using MoonPress.Rendering;

namespace MoonPress.BlazorDesktop.Components.Pages.Content;

public partial class NewContentItem
{
    private ContentItem _item = new();
    
    [Inject]
    private NavigationManager Nav { get; set; }

    private async Task Save()
    {
        await ContentItemSaver.SaveContentItem(_item, new ContentItemMarkdownRenderer(), ProjectState.Current!.RootFolder);
        Nav.NavigateTo("/content-items");
    }

    void Cancel() => Nav.NavigateTo("/content-items");
}