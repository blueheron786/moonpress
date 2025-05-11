using System.Text.Json;

namespace MoonstoneCms.Core.Tests;

[TestFixture]
public class StaticSiteProjectTests
{
    private string _tempDirectory;

    [SetUp]
    public void SetUp()
    {
        // Create a unique temporary directory for each test run
        _tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_tempDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up the temporary directory after each test
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true); // true to delete recursively
        }
    }

    [Test]
    public void Save_CreatesFileWithCorrectContent()
    {
        // Arrange
        var project = new StaticSiteProject { ProjectName = "MyTestProject" };
        var expectedJsonPath = Path.Combine(_tempDirectory, "project.json");

        // Act
        project.Save(_tempDirectory);

        // Assert
        // Verify the file exists
        Assert.That(File.Exists(expectedJsonPath), $"File {expectedJsonPath} should exist.");

        // Verify the content is correct by loading it back
        var loadedProject = StaticSiteProject.Load(_tempDirectory); // Use the Load method to verify Save
        Assert.That(loadedProject, Is.Not.Null, "Loaded project should not be null.");
        Assert.That(loadedProject.ProjectName, Is.EqualTo(project.ProjectName), "Loaded project name should match the original.");

        // Optionally, read the raw JSON to check indentation
        var rawJson = File.ReadAllText(expectedJsonPath);
        Assert.That(rawJson, Does.Contain("\"ProjectName\": \"MyTestProject\""), "JSON content should contain ProjectName");
        Assert.That(rawJson, Does.Contain(Environment.NewLine), "JSON content should be indented"); // Simple check for indentation
    }

    [Test]
    public void Load_ReadsFileAndDeserializesCorrectly()
    {
        // Arrange
        var projectName = "AnotherProject";
        var sampleProjectJson = $@"
        {{
          ""ProjectName"": ""{projectName}""
        }}"; // Match the expected format (indented)
        var jsonPath = Path.Combine(_tempDirectory, "project.json");
        File.WriteAllText(jsonPath, sampleProjectJson);

        // Act
        var loadedProject = StaticSiteProject.Load(_tempDirectory);

        // Assert
        Assert.That(loadedProject, Is.Not.Null, "Loaded project should not be null.");
        Assert.That(loadedProject.ProjectName, Is.EqualTo(projectName), "Loaded project name should match the content of the file.");
    }

    [Test]
    public void Load_ThrowsExceptionIfFileDoesNotExist()
    {
        // Act/Assert
        Assert.Throws<FileNotFoundException>(() => StaticSiteProject.Load(_tempDirectory));
    }

    [Test]
    public void Load_ThrowsJsonExceptionForInvalidJson()
    {
        // Arrange
        var invalidJson = "{ \"ProjectName\": \"Test\""; // Missing closing brace
        var jsonPath = Path.Combine(_tempDirectory, "project.json");
        File.WriteAllText(jsonPath, invalidJson);

        // Act/Assert
        Assert.Throws<JsonException>(() => StaticSiteProject.Load(_tempDirectory));
    }
}