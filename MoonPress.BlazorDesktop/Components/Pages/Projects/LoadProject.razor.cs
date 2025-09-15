using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using Microsoft.AspNetCore.Components;
using MoonPress.Core;
using MoonPress.BlazorDesktop.Services;

namespace MoonPress.BlazorDesktop.Components.Pages.Projects;

public partial class LoadProject : ComponentBase
{
    [Inject]
    private NavigationManager Nav { get; set; } = default!;

    private async Task SelectAndLoadProject()
    {
        using var dialog = new FolderBrowserDialog();
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            var folder = dialog.SelectedPath;
            var projectFile = Path.Combine(folder, "project.json");

            if (!File.Exists(projectFile))
            {
                MessageBox.Show("No project.json found in selected folder.");
                return;
            }

            try
            {
                var json = await File.ReadAllTextAsync(projectFile);
                var project = JsonSerializer.Deserialize<StaticSiteProject>(json);

                if (project is not null)
                {
                    ProjectState.Current = project;
                    ProjectState.Current.RootFolder = folder;

                    // Save as last opened project for auto-loading
                    _ = Task.Run(async () => await ProjectStateService.SaveLastOpenedProjectAsync(project, folder));

                    Nav.NavigateTo("/content-items");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load project.json:\n" + ex.Message);
            }
        }
    }
}