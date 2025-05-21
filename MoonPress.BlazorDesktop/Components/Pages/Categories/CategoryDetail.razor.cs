using Microsoft.AspNetCore.Components;
using MoonPress.Core.Content;
using MoonPress.Core.Models;

namespace MoonPress.BlazorDesktop.Components.Pages.Categories;

public partial class CategoryDetail : ComponentBase
{
    [Parameter]
    public string CategoryName { get; set; } = string.Empty;

    private List<ContentItem> Items = new();

    protected override void OnParametersSet()
    {
        var all = ContentItemFetcher.GetItemsByCategory();
        if (all.TryGetValue(CategoryName, out var items))
        {
            Items = items
                .OrderByDescending(i => i.DatePublished)
                .ToList();
        }
        else
        {
            Items = new List<ContentItem>();
        }
    }
}
