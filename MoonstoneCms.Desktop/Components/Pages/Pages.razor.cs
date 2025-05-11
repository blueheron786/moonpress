using Microsoft.AspNetCore.Components;
using System.IO;
using System.Linq;

namespace MoonstoneCms.Desktop.Components.Pages;

public partial class Pages : ComponentBase
{
    protected IEnumerable<string>? PageFiles { get; set; }

    protected override void OnInitialized()
    {
        if (ProjectState.Current is not null)
        {
            var pagesDirectory = Path.Combine(ProjectState.Current.Location, "content", "pages");
            if (Directory.Exists(pagesDirectory))
            {
                PageFiles = Directory.GetFiles(pagesDirectory, "*.md")
                    .Select(Path.GetFileNameWithoutExtension)
                    .ToList();
            }
        }
    }
}
