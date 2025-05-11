using Microsoft.AspNetCore.Components;
using MoonstoneCms.Core.Models;

namespace MoonstoneCms.Desktop.Components.Pages.Content;

public partial class NewContentItem
{
    private ContentItem _item = new();
    
    [Inject]
    private NavigationManager Nav { get; set; }

    void Save()
    {
        // TODO: Save logic
        Nav.NavigateTo("/content-items");
    }

    void Cancel() => Nav.NavigateTo("/content-items");
}