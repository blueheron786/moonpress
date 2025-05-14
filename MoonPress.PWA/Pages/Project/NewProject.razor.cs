using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MoonPress.Core.Models;

namespace MoonPress.PWA.Pages.Project;

public partial class NewProject
{
    [Inject]
    private IJSRuntime _js { get; set; }
    
    private string ProjectName = "";
    private string Message = "";

    private async Task CreateNewProject()
    {
        if (string.IsNullOrWhiteSpace(ProjectName))
        {
            Message = "Please enter a project name.";
            return;
        }

        var project = new MoonPressProject()
        {
            Name = ProjectName,
            CreatedOn = DateTime.UtcNow,
            LastModifiedOn = DateTime.UtcNow,
        };

        var json = JsonSerializer.Serialize(project, new JsonSerializerOptions { WriteIndented = true });
        var projectNameSanitized = ProjectName.Replace(" ", "-").Replace(" ", "-").Replace("!", "");
        var result = await _js.InvokeAsync<FolderPickerResult>("moonpress.showFolderPicker", CancellationToken.None, []);

        Message = result.success
            ? "Project created successfully."
            : $"Failed: {result.error}";
    }

    private class FolderPickerResult
    {
        public bool success { get; set; }
        public string? name { get; set; }
        public string? error { get; set; }
    }
}