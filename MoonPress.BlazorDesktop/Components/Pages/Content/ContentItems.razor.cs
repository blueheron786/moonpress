using Microsoft.AspNetCore.Components;
using MoonPress.Core.Content;
using MoonPress.Core.Models;

namespace MoonPress.BlazorDesktop.Components.Pages.Content;

public partial class ContentItems : ComponentBase
{

    [Inject]
    protected NavigationManager? Nav { get; set; }

    private List<ContentItem> ContentItemsList { get {
        return ProjectState.Current == null ? [] :
            ContentItemFetcher.GetContentItems(ProjectState.Current!.RootFolder).Values
            .OrderByDescending(x => x.DatePublished).ToList();
    } }

    protected void GoToNewContentItem() => Nav.NavigateTo("/content-item/new");
}