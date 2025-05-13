using System.Reactive;
using MoonPress.Avalonia.Models;
using MoonPress.Avalonia.Services.IO;
using MoonPress.Core.Models;

using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using ReactiveUI;

namespace MoonPress.Avalonia.ViewModels.Project;

public class LoadProjectViewModel : ViewModelBase
{
    private readonly IFolderPickerService _folderPickerService;
    private readonly IAppContext _appContext;

    public ReactiveCommand<Unit, Unit> LoadCommand { get; }

    public LoadProjectViewModel(IFolderPickerService folderPickerService, IAppContext appContext)
    {
        _folderPickerService = folderPickerService;
        _appContext = appContext;

        LoadCommand = ReactiveCommand.CreateFromTask(ExecuteLoadAsync);
    }

    private async Task ExecuteLoadAsync()
    {
        var folder = await _folderPickerService.ShowFolderSelectionDialogAsync();
        if (string.IsNullOrWhiteSpace(folder))
            return;

        var path = Path.Combine(folder, "project.json");
        if (!File.Exists(path))
        {
            // TODO: notify user project file was not found
            return;
        }

        var json = await File.ReadAllTextAsync(path);
        var project = JsonSerializer.Deserialize<MoonPressProject>(json);

        if (project != null)
        {
            _appContext.CurrentProject = project;
            // TODO: navigate to project dashboard or update UI
        }
    }
}
