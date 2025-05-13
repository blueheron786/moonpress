using System;
using System.IO;
using System.Reactive;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using MoonPress.Avalonia.Models;
using MoonPress.Avalonia.Services.IO;
using MoonPress.Core.Models;
using ReactiveUI;

namespace MoonPress.Avalonia.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public ReactiveCommand<Unit, Unit> NewProjectCommand { get; }
    public ReactiveCommand<Unit, Unit> LoadProjectCommand { get; }

    private readonly IFolderPickerService _folderPickerService;
    private readonly IAppContext _appContext;

    public MainWindowViewModel(IFolderPickerService folderPickerService, IAppContext  appContext)
    {
        _folderPickerService = folderPickerService;
        _appContext = appContext;

        NewProjectCommand = ReactiveCommand.Create(HandleNewProject);
        LoadProjectCommand = ReactiveCommand.Create(HandleLoadProject);
    }

    private async void HandleNewProject()
    {
        var folder = await _folderPickerService.ShowFolderSelectionDialogAsync();

        if (string.IsNullOrWhiteSpace(folder))
        {
            // User cancelled, so we don't need to do anything
            return;
        }

        // var inputDialog = new TextInputDialog("Enter project name:");
        // var name = await inputDialog.ShowAsync(window);
        var name = "Hardcoded Project Name"; // TODO: Replace with actual input dialog

        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        var project = new MoonPressProject
        {
            ProjectName = name,
            ProjectFolder = folder
        };

        var jsonPath = Path.Combine(folder, "project.json");
        File.WriteAllText(jsonPath, JsonSerializer.Serialize(project, new JsonSerializerOptions { WriteIndented = true }));
        
        // TODO: Navigate to a Project Dashboard
        _appContext.CurrentProject = project;
    }

    private async void HandleLoadProject()
    {
        // Open file dialog to select a project.json file
        var folder = await _folderPickerService.ShowFolderSelectionDialogAsync();

        // If the user cancels or doesn't select any file, exit early
        if (string.IsNullOrWhiteSpace(folder))
        {
            return;
        }

        try
        {
            var jsonContent = await File.ReadAllTextAsync(Path.Join(folder, "project.json"));
            var project = JsonSerializer.Deserialize<MoonPressProject>(jsonContent);

            if (project == null)
            {
                // Handle error if the project couldn't be deserialized
                // You can show an error message to the user here
                return;
            }

            _appContext.CurrentProject = project;
        }
        catch (Exception ex)
        {
            // Handle any errors (e.g., file read errors, JSON deserialization errors)
            // You can log the error or show a message to the user
            Console.Error.WriteLine($"Error loading project: {ex.Message}");
        }
    }
}
