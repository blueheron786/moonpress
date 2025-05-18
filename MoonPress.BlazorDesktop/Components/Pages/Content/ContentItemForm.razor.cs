using Microsoft.AspNetCore.Components;
using MoonPress.Core.Models;

namespace MoonPress.BlazorDesktop.Components.Pages.Content;

public partial class ContentItemForm : ComponentBase
{
    [Parameter]
    public ContentItem Model { get; set; } = default!;

    [Parameter]
    public EventCallback OnValidSubmitCallback { get; set; } = default!;

    [Parameter]
    public EventCallback OnCancel { get; set; } = default!;

    private int SummaryLength => Model.Summary?.Length ?? 0;

    private void OnSummaryChanged(ChangeEventArgs e)
    {
        Model.Summary = e.Value?.ToString();
        StateHasChanged();
    }

    #region custom fields

    // Helper class for binding
    public class CustomFieldPair
    {
        public string Key { get; set; } = "";
        public string Value { get; set; } = "";
    }

    // List for UI binding
    private List<CustomFieldPair> CustomFieldsList = new();

    protected override void OnInitialized()
    {
        // Populate from model
        if (Model.CustomFields != null)
        {
            CustomFieldsList = Model.CustomFields
                .Select(kvp => new CustomFieldPair { Key = kvp.Key, Value = kvp.Value })
                .ToList();
        }
    }

    private void AddCustomField()
    {
        CustomFieldsList.Add(new CustomFieldPair());
    }

    private void RemoveCustomField(CustomFieldPair pair)
    {
        CustomFieldsList.Remove(pair);
    }
    
    private async Task OnValidSubmit()
    {
        // Sync back to model
        Model.CustomFields = CustomFieldsList
            .Where(p => !string.IsNullOrWhiteSpace(p.Key))
            .ToDictionary(p => p.Key, p => p.Value ?? "");
        // Call the callback if set
        if (OnValidSubmitCallback.HasDelegate)
        {
            await OnValidSubmitCallback.InvokeAsync();
        }
    }

    #endregion
}
