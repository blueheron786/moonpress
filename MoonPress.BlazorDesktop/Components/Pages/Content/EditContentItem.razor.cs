using Microsoft.AspNetCore.Components;
using MoonPress.Core.Content;
using MoonPress.Core.Models;
using MoonPress.Rendering;

namespace MoonPress.BlazorDesktop.Components.Pages.Content;

public partial class EditContentItem : ComponentBase
{
    [Parameter]
    public string Id { get; set; }

    [Inject]
    private NavigationManager Nav { get; set; }

    private ContentItem Model = new ContentItem();

    protected override async Task OnInitializedAsync()
    {
        // Fetch the content item by Id (replace with actual data fetching logic)
        Model = await FetchContentItemById(Id);
    }

    private async Task<ContentItem> FetchContentItemById(string id)
    {
        var allItems = ContentItemFetcher.GetContentItems(ProjectState.Current!.RootFolder);
        return allItems[id];
    }

    private async Task Save()
    {
        await ContentItemSaver.SaveContentItem(Model, new ContentItemMarkdownRenderer(), ProjectState.Current!.RootFolder);
        ContentItemFetcher.UpdateCache(Model);
        Nav.NavigateTo("/content-items");
    }

    private void Cancel()
    {
        Nav.NavigateTo("/content-items");
    }
}
