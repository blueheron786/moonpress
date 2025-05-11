using System;

namespace MoonstoneCms.Desktop.Components.Layout;

public partial class NavMenu
{
    protected override void OnInitialized()
    {
        ProjectState.OnProjectLoaded += StateHasChanged;
    }

    public void Dispose()
    {
        ProjectState.OnProjectLoaded -= StateHasChanged;
    }
}
