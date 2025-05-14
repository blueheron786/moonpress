using System.Windows.Forms;
using Microsoft.AspNetCore.Components;
using MoonPress.Core;

namespace MoonPress.BlazorDesktop.Components.Pages.Projects;

public partial class NewProject : ComponentBase
{
    private string _projectName = "";
    private string _selectedFolder = "";
    private bool _isCreationSuccessful = false;

    private bool IsCreateDisabled => string.IsNullOrWhiteSpace(_projectName) || string.IsNullOrWhiteSpace(_selectedFolder);

    private void SelectFolder()
    {
        using var dialog = new FolderBrowserDialog();
        var result = dialog.ShowDialog();

        if (result == DialogResult.OK)
        {
            _selectedFolder = dialog.SelectedPath;
        }
    }

    private void CreateProject()
    {
        var project = new StaticSiteProject { ProjectName = _projectName };
        project.Save(_selectedFolder);
        _isCreationSuccessful = true;
    }
}