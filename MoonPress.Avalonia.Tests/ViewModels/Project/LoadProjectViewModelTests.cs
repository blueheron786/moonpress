using System.Reactive;
using System.Text.Json;
using MoonPress.Avalonia.Models;
using MoonPress.Avalonia.Services.IO;
using MoonPress.Avalonia.ViewModels.Project;
using MoonPress.Core.Models;
using NSubstitute;
using NUnit.Framework;

namespace MoonPress.Avalonia.Tests.ViewModels.Project;

public class LoadProjectViewModelTests
{
    private IFolderPickerService _folderPickerService = default!;
    private IAppContext _appContext = default!;
    private LoadProjectViewModel _viewModel = default!;

    [SetUp]
    public void SetUp()
    {
        _folderPickerService = Substitute.For<IFolderPickerService>();
        _appContext = Substitute.For<IAppContext>();
        _viewModel = new LoadProjectViewModel(_folderPickerService, _appContext);
    }

    [Test]
    public void ExecuteLoadAsync_LoadsProject_WhenFolderIsSelected()
    {
        // Arrange
        var folderPath = Path.GetTempPath();
        _folderPickerService.ShowFolderSelectionDialogAsync().Returns(Task.FromResult<string?>(folderPath));
        var project = new MoonPressProject { ProjectName = "Test Project", ProjectFolder = folderPath };
        var projectJson = JsonSerializer.Serialize(project);
        File.WriteAllText(Path.Combine(folderPath, "project.json"), projectJson); // Create a mock project file

        // Act
        _viewModel.LoadCommand.Execute(Unit.Default).Subscribe();

        // Assert
        _appContext.Received().CurrentProject = Arg.Is<MoonPressProject>(p => p.ProjectName == "Test Project");
    }

    [Test]
    public void ExecuteLoadAsync_DoesNothing_WhenNoFolderIsSelected()
    {
        // Arrange
        _folderPickerService.ShowFolderSelectionDialogAsync().Returns(Task.FromResult<string?>(null));

        // Act
        _viewModel.LoadCommand.Execute(Unit.Default).Subscribe();

        // Assert
        _appContext.DidNotReceive().CurrentProject = Arg.Any<MoonPressProject>();
    }

    [Test]
    public void ExecuteLoadAsync_DoesNothing_WhenProjectJsonFileDoesNotExist()
    {
        // Arrange
        var folderPath = Path.GetTempPath();
        _folderPickerService.ShowFolderSelectionDialogAsync().Returns(Task.FromResult<string?>(folderPath));

        // Act
        _viewModel.LoadCommand.Execute(Unit.Default).Subscribe();

        // Assert
        _appContext.DidNotReceive().CurrentProject = Arg.Any<MoonPressProject>();
    }

    [Test]
    public void ExecuteLoadAsync_DoesNothing_WhenJsonIsInvalid()
    {
        // Arrange
        var folderPath = Path.GetTempPath();
        _folderPickerService.ShowFolderSelectionDialogAsync().Returns(Task.FromResult<string?>(folderPath));
        var invalidJson = "{ invalid json }";
        File.WriteAllText(Path.Combine(folderPath, "project.json"), invalidJson);

        // Act
        _viewModel.LoadCommand.Execute(Unit.Default).Subscribe();

        // Assert
        _appContext.DidNotReceive().CurrentProject = Arg.Any<MoonPressProject>();
    }
}