using Microsoft.AspNetCore.Components;
using System.IO;

namespace MoonstoneCms.Desktop.Components.Pages.Content;

public partial class ContentItems : ComponentBase
{
    [Inject]
    protected NavigationManager? Nav { get; set; }

    private IEnumerable<string> ContentFiles { get; set; } = new List<string>();

    protected override void OnInitialized()
    {
        if (ProjectState.Current is not null)
        {
            var pagesDirectory = Path.Combine(ProjectState.Current.Location, "content");
            if (Directory.Exists(pagesDirectory))
            {
                ContentFiles = Directory
                    .EnumerateFiles(pagesDirectory, "*.md", SearchOption.AllDirectories)
                    .Select(Path.GetFileNameWithoutExtension)
                    .ToList();
            }
        }
    }
    
    protected void GoToNewContentItem() => Nav.NavigateTo("/content-item/new");
}
