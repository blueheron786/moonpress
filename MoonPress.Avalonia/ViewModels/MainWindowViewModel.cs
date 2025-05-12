using System;
using System.IO;
using System.Reactive;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using MoonPress.Avalonia.Services.IO;
using MoonPress.Core.Models;
using ReactiveUI;

namespace MoonPress.Avalonia.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public string Greeting { get; } = "Welcome to MoonPress!";

    public ReactiveCommand<Unit, Unit> NewProjectCommand { get; }
    public ReactiveCommand<Unit, Unit> LoadProjectCommand { get; }

    private readonly IFolderPickerService _folderPickerService;

    public MainWindowViewModel(IFolderPickerService folderPickerService)
    {
        _folderPickerService = folderPickerService;

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
    }


    private async void HandleLoadProject()
    {
        // We'll implement this next
    }
}
