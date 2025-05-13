using System.Reactive;
using System.Runtime.InteropServices.Swift;
using MoonPress.Avalonia.Models;
using MoonPress.Avalonia.Services.IO;
using MoonPress.Avalonia.ViewModels.Project;
using ReactiveUI;

namespace MoonPress.Avalonia.ViewModels;

/// <summary>
/// This is the main view model for the application.
/// It will contain the logic for navigating between different views (like NewProjectViewModel, LoadProjectViewModel, etc.)
/// and will be used as the DataContext for the MainWindow.
/// The commands are defined here and will be bound to buttons in the MainWindow.xaml
/// The commands are executed when the buttons are clicked.
/// </summary>
public partial class MainWindowViewModel : ViewModelBase
{
    // Commands just navigate to the appropriate view
    public ReactiveCommand<Unit, Unit> NewProjectCommand { get; }
    public ReactiveCommand<Unit, Unit> LoadProjectCommand { get; }

    private IFolderPickerService _folderPickerService;
    private IAppContext _appContext;
    
    // The currently active view
    private ViewModelBase _currentView = default!;
    
    public MainWindowViewModel(IFolderPickerService folderPickerService, IAppContext appContext)
    {
        _folderPickerService = folderPickerService;
        _appContext = appContext;

        NewProjectCommand = ReactiveCommand.Create(() => 
        {
            CurrentView = new NewProjectViewModel(_folderPickerService, _appContext);
        });

        LoadProjectCommand = ReactiveCommand.Create(() => 
        {
            CurrentView = new LoadProjectViewModel(_folderPickerService, _appContext);
        });

        // Set the initial view to LoadProjectViewModel
        CurrentView = new LoadProjectViewModel(_folderPickerService, _appContext);
    }

    public ViewModelBase CurrentView
    {
        get => _currentView;
        set => this.RaiseAndSetIfChanged(ref _currentView, value);
    }
}