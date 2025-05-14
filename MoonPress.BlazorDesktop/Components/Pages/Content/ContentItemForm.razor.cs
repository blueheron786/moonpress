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

    private int SummaryLength => Model.Summary?.Length ?? 0;

    private void OnSummaryChanged(ChangeEventArgs e)
    {
        Model.Summary = e.Value?.ToString();
        StateHasChanged();
    }
}
