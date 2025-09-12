using NUnit.Framework;
using MoonPress.Core;
using MoonPress.Core.Models;
using MoonPress.Rendering;
using System.IO;
using System.Threading.Tasks;

namespace MoonPress.Core.Tests;

[TestFixture]
public class TemplateValidationTests
{
    private StaticSiteGenerator _generator;
    private string _testDirectory;

    [SetUp]
    public void Setup()
    {
        _generator = new StaticSiteGenerator(new ContentItemHtmlRenderer());
        _testDirectory = Path.Combine(Path.GetTempPath(), $"moonpress_template_validation_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }

    [Test]
    public async Task GenerateSiteAsync_FailsWhenContentPlaceholderMissing()
    {
        // Arrange
        var project = CreateTestProject();
        CreateThemeWithLayout(@"<!DOCTYPE html>
<html>
<head><title>{{ title }}</title></head>
<body>
    <nav>{{ navbar }}</nav>
</body>
</html>");

        // Act
        var outputPath = Path.Combine(_testDirectory, "output");
        var result = await _generator.GenerateSiteAsync(project, outputPath);

        // Debug output
        Console.WriteLine($"Result.Success: {result.Success}");
        Console.WriteLine($"Result.Message: {result.Message}");

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("{{ content }}"));
        Assert.That(result.Message, Does.Contain("missing"));
    }

    [Test]
    public async Task GenerateSiteAsync_FailsWhenNavbarPlaceholderMissing()
    {
        // Arrange
        var project = CreateTestProject();
        CreateThemeWithLayout(@"<!DOCTYPE html>
<html>
<head><title>{{ title }}</title></head>
<body>
    <main>{{ content }}</main>
</body>
</html>");

        // Act
        var outputPath = Path.Combine(_testDirectory, "output");
        var result = await _generator.GenerateSiteAsync(project, outputPath);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("{{ navbar }}"));
        Assert.That(result.Message, Does.Contain("missing"));
    }

    [Test]
    public async Task GenerateSiteAsync_FailsWhenTitlePlaceholderMissing()
    {
        // Arrange
        var project = CreateTestProject();
        CreateThemeWithLayout(@"<!DOCTYPE html>
<html>
<head>
</head>
<body>
    <nav>{{ navbar }}</nav>
    <main>{{ content }}</main>
</body>
</html>");

        // Act
        var outputPath = Path.Combine(_testDirectory, "output");
        var result = await _generator.GenerateSiteAsync(project, outputPath);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("{{ title }}"));
        Assert.That(result.Message, Does.Contain("missing"));
    }

    [Test]
    public async Task GenerateSiteAsync_SucceedsWhenAllRequiredPlaceholdersPresent()
    {
        // Arrange
        var project = CreateTestProject();
        CreateThemeWithLayout(@"<!DOCTYPE html>
<html>
<head><title>{{ title }}</title></head>
<body>
    <nav>{{ navbar }}</nav>
    <main>{{ content }}</main>
</body>
</html>");

        // Act
        var outputPath = Path.Combine(_testDirectory, "output");
        var result = await _generator.GenerateSiteAsync(project, outputPath);

        // Assert
        Assert.That(result.Success, Is.True);
    }

    [Test]
    public async Task GenerateSiteAsync_FailsWhenMultiplePlaceholdersMissing()
    {
        // Arrange
        var project = CreateTestProject();
        CreateThemeWithLayout(@"<!DOCTYPE html>
<html>
<head>
</head>
<body>
</body>
</html>");

        // Act
        var outputPath = Path.Combine(_testDirectory, "output");
        var result = await _generator.GenerateSiteAsync(project, outputPath);

        // Assert
        Assert.That(result.Success, Is.False);
        // Should mention the first missing placeholder
        Assert.That(result.Message, Does.Contain("missing"));
    }

    private StaticSiteProject CreateTestProject()
    {
        var project = new StaticSiteProject
        {
            RootFolder = _testDirectory,
            Theme = "default",
            ProjectName = "Test Site"
        };

        // Create minimal content structure
        var contentDir = Path.Combine(_testDirectory, "content");
        Directory.CreateDirectory(contentDir);

        return project;
    }

    private void CreateThemeWithLayout(string layoutHtml)
    {
        var themeDir = Path.Combine(_testDirectory, "themes", "default");
        Directory.CreateDirectory(themeDir);
        File.WriteAllText(Path.Combine(themeDir, "layout.html"), layoutHtml);
    }
}
