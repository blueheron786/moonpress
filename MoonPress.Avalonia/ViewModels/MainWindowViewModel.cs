using System.IO;
using System.Reactive;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using MoonPress.Core.Models;
using ReactiveUI;

namespace MoonPress.Avalonia.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public string Greeting { get; } = "Welcome to MoonPress!";

    public ReactiveCommand<Unit, Unit> NewProjectCommand { get; }
    public ReactiveCommand<Unit, Unit> LoadProjectCommand { get; }

    public MainWindowViewModel()
    {
        NewProjectCommand = ReactiveCommand.Create(HandleNewProject);
        LoadProjectCommand = ReactiveCommand.Create(HandleLoadProject);
    }

    private async void HandleNewProject()
    {
        var dlg = new OpenFolderDialog
        {
            Title = "Select Folder for New Project"
        };

        var window = (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
        var folder = await dlg.ShowAsync(window);

        if (string.IsNullOrWhiteSpace(folder))
            return;

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
