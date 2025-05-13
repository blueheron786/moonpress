
using NUnit.Framework;
using NSubstitute;
using MoonPress.Avalonia.ViewModels;
using MoonPress.Core.Models;
using System.Text.Json;
using MoonPress.Avalonia.Models;
using MoonPress.Avalonia.Services.IO;
using MoonPress.Avalonia.ViewModels.Project;

namespace MoonPress.Avalonia.Tests.ViewModels.Project;

[TestFixture]
public class NewProjectViewModelTests
{
    [Test]
    public void NewProjectCommand_CreatesProjectJsonFileAndSetsAppContext()
    {
        // Arrange
        var tempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempFolder);

        try
        {

            var dialogService = Substitute.For<IFolderPickerService>();
            dialogService.ShowFolderSelectionDialogAsync()
                .Returns(tempFolder);
        
            var appContext = Substitute.For<IAppContext>();

            var viewModel = new NewProjectViewModel(dialogService, appContext);
            viewModel.BrowseFolderCommand.Execute().Subscribe();

            // Act
            viewModel.CreateCommand.Execute().Subscribe();

            // Assert
            var jsonPath = Path.Combine(tempFolder, "project.json");
            Assert.That(File.Exists(jsonPath), Is.True);

            var json = File.ReadAllText(jsonPath);
            var project = JsonSerializer.Deserialize<MoonPressProject>(json);

            Assert.That(project, Is.Not.Null);
            Assert.That(project.ProjectFolder, Is.EqualTo(tempFolder));
            Assert.That(appContext.CurrentProject!.ProjectFolder, Is.EqualTo(project.ProjectFolder));
            Assert.That(appContext.CurrentProject.ProjectName, Is.EqualTo(project.ProjectName));
        }
        finally
        {
            // Cleanup
            Directory.Delete(tempFolder, true);
        }
    }
}
