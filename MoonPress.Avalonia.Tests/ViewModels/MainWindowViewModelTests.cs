
using NUnit.Framework;
using NSubstitute;
using MoonPress.Avalonia.ViewModels;
using MoonPress.Core.Models;
using System.Text.Json;
using MoonPress.Avalonia.Models;
using MoonPress.Avalonia.Services.IO;

namespace MoonPress.Avalonia.Tests.ViewModels;

[TestFixture]
public class MainWindowViewModelTests
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

            var viewModel = new MainWindowViewModel(dialogService, appContext);

            // Act
            viewModel.NewProjectCommand.Execute().Subscribe();

            // Assert
            var jsonPath = Path.Combine(tempFolder, "project.json");
            Assert.That(File.Exists(jsonPath), Is.True);

            var json = File.ReadAllText(jsonPath);
            var project = JsonSerializer.Deserialize<MoonPressProject>(json);

            Assert.That(project!.ProjectName, Is.EqualTo("Hardcoded Project Name"));
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
