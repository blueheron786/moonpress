using Microsoft.AspNetCore.Components;
using MoonPress.Core.Models;

namespace MoonPress.BlazorDesktop.Components.Pages.Content;

public partial class ContentItemForm : ComponentBase
{
    [Parameter]
    public ContentItem Model { get; set; } = default!;
    
    [Parameter]
    public EventCallback OnValidSubmit { get; set; } = default!;
    
    [Parameter]
    public EventCallback OnCancel { get; set; } = default!;
}
