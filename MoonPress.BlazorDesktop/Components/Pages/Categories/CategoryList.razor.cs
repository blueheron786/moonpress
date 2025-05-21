using Microsoft.AspNetCore.Components;
using MoonPress.Core.Content;
using MoonPress.Core.Models;

namespace MoonPress.BlazorDesktop.Components.Pages.Categories;

public partial class CategoryList : ComponentBase
{
    private Dictionary<string, List<ContentItem>> _itemsByCategory => ContentItemFetcher.GetItemsByCategory();
}
