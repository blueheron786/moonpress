using MoonPress.Core;
using MoonPress.Rendering;
using NUnit.Framework;

namespace MoonPress.Core.Tests;

[TestFixture]
public class ThemeSystemIntegrationTests
{
    private const string ThemesFolderName = "themes";
    private const string ContentFolderName = "content";
    private string _testProjectPath = null!;
    private string _outputPath = null!;

    [SetUp]
    public void Setup()
    {
        _testProjectPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "test-theme-project");
        _outputPath = Path.Combine(_testProjectPath, "output");
        
        // Clean up any previous test runs
        if (Directory.Exists(_testProjectPath))
        {
            Directory.Delete(_testProjectPath, true);
        }
        
        Directory.CreateDirectory(_testProjectPath);
        
        // Create test project structure
        CreateTestProject();
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_testProjectPath))
        {
            Directory.Delete(_testProjectPath, true);
        }
    }

    [Test]
    public async Task GenerateSiteAsync_WithDefaultTheme_AppliesThemeLayout()
    {
        // Arrange
        var project = StaticSiteProject.Load(_testProjectPath);
        var htmlRenderer = new ContentItemHtmlRenderer();
        var generator = new StaticSiteGenerator(htmlRenderer);

        // Act
        var result = await generator.GenerateSiteAsync(project, _outputPath);

        // Assert
        Assert.That(result.Success, Is.True, $"Generation failed: {result.Message}");
        Assert.That(result.PagesGenerated, Is.GreaterThan(0));
        
        // Check that theme files are copied
        Assert.That(File.Exists(Path.Combine(_outputPath, "style.css")), Is.True);
        
        // Check that content pages use theme layout
        var contentHtml = File.ReadAllText(Path.Combine(_outputPath, "test-article.html"));
        Assert.That(contentHtml, Does.Contain("<!DOCTYPE html>"));
        Assert.That(contentHtml, Does.Contain("<title>Test Article</title>"));
        Assert.That(contentHtml, Does.Contain("<link rel=\"stylesheet\" href=\"style.css\""));
        Assert.That(contentHtml, Does.Contain("<h1>Test Article</h1>"));
        Assert.That(contentHtml, Does.Contain("This is the content"));
    }

    private void CreateTestProject()
    {
        // Create project.json
        var projectJson = """
        {
          "ProjectName": "Test Site",
          "Theme": "default",
          "CreatedOn": "2025-09-12T14:05:00Z"
        }
        """;
        File.WriteAllText(Path.Combine(_testProjectPath, "project.json"), projectJson);

        // Create theme directory and files
        var themeDir = Path.Combine(_testProjectPath, ThemesFolderName, "default");
        Directory.CreateDirectory(themeDir);

        var layoutHtml = """
        <!DOCTYPE html>
        <html lang="en">
        <head>
            <meta charset="utf-8" />
            <meta name="viewport" content="width=device-width, initial-scale=1.0" />
            <title>{{TITLE}}</title>
            <link rel="stylesheet" href="style.css" />
        </head>
        <body>
            {{CONTENT}}
        </body>
        </html>
        """;
        File.WriteAllText(Path.Combine(themeDir, "layout.html"), layoutHtml);

        var styleCss = """
        body { font-family: Arial, sans-serif; margin: 20px; }
        .content { line-height: 1.6; }
        """;
        File.WriteAllText(Path.Combine(themeDir, "style.css"), styleCss);

        // Create content directory and test content
        var contentDir = Path.Combine(_testProjectPath, ContentFolderName);
        Directory.CreateDirectory(contentDir);

        var testContent = """
        ---
        title: "Test Article"
        slug: "test-article"
        date_published: "2024-06-01T14:30:00"
        is_draft: false
        summary: "A test article."
        ---

        This is the content of the test article.
        """;
        File.WriteAllText(Path.Combine(contentDir, "test-article.md"), testContent);
    }
}
