// NewProjectViewModel.cs

using System;
using System.IO;
using System.Reactive;
using System.Text.Json;
using System.Threading.Tasks;
using MoonPress.Avalonia.Models;
using MoonPress.Avalonia.Services.IO;
using MoonPress.Core.Models;
using ReactiveUI;

namespace MoonPress.Avalonia.ViewModels.Project;

public class NewProjectViewModel : ViewModelBase
{
    private readonly IFolderPickerService _folderPickerService;
    private readonly IAppContext _appContext;

    // Properties (bound to the view)
    private string _projectName = "";
    public string ProjectName
    {
        get => _projectName;
        set => this.RaiseAndSetIfChanged(ref _projectName, value);
    }

    private string _projectFolder = "";
    public string ProjectFolder
    {
        get => _projectFolder;
        set => this.RaiseAndSetIfChanged(ref _projectFolder, value);
    }

    // Commands
    public ReactiveCommand<Unit, Unit> BrowseFolderCommand { get; }
    public ReactiveCommand<Unit, Unit> CreateCommand { get; }

    public NewProjectViewModel(IFolderPickerService folderPickerService, IAppContext appContext)
    {
        _folderPickerService = folderPickerService;
        _appContext = appContext;

        // Setup commands
        BrowseFolderCommand = ReactiveCommand.CreateFromTask(BrowseFolderAsync);
        CreateCommand = ReactiveCommand.CreateFromTask(CreateProjectAsync, CanCreateProject());
    }

    private async Task BrowseFolderAsync()
    {
        var folder = await _folderPickerService.ShowFolderSelectionDialogAsync();
        if (!string.IsNullOrWhiteSpace(folder))
        {
            ProjectFolder = folder;
        }
    }

    private async Task CreateProjectAsync()
    {
        var project = new MoonPressProject
        {
            ProjectName = ProjectName,
            ProjectFolder = ProjectFolder
        };

        var jsonPath = Path.Combine(ProjectFolder, "project.json");
        await File.WriteAllTextAsync(jsonPath, 
            JsonSerializer.Serialize(project, new JsonSerializerOptions { WriteIndented = true }));
        
        _appContext.CurrentProject = project;
        
        // TODO: Navigate to project dashboard or close this view
    }

    private IObservable<bool> CanCreateProject() =>
        this.WhenAnyValue(
            x => x.ProjectName,
            x => x.ProjectFolder,
            (name, folder) => 
                !string.IsNullOrWhiteSpace(name) && 
                !string.IsNullOrWhiteSpace(folder));
}