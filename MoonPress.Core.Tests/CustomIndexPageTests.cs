using MoonPress.Core;
using MoonPress.Core.Content;
using MoonPress.Core.Generators;
using MoonPress.Core.Models;
using MoonPress.Core.Renderers;
using MoonPress.Core.Templates;
using NSubstitute;
using NUnit.Framework;

namespace MoonPress.Core.Tests;

[TestFixture]
public class CustomIndexPageTests
{
    private string _testProjectPath;
    private string _testOutputPath;

    [SetUp]
    public void Setup()
    {
        _testProjectPath = Path.Combine(Path.GetTempPath(), "moonpress_test_" + Guid.NewGuid().ToString());
        _testOutputPath = Path.Combine(_testProjectPath, "output");
        
        Directory.CreateDirectory(_testProjectPath);
        Directory.CreateDirectory(_testOutputPath);
        Directory.CreateDirectory(Path.Combine(_testProjectPath, "content", "pages"));
        Directory.CreateDirectory(Path.Combine(_testProjectPath, "themes", "default"));
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_testProjectPath))
        {
            Directory.Delete(_testProjectPath, true);
        }
        ContentItemFetcher.ClearContentItems();
    }

    [Test]
    public async Task IndexPageGenerator_ShouldSkipGeneration_WhenCustomIndexPageExists()
    {
        // Arrange
        var contentItems = new List<ContentItem>
        {
            new ContentItem 
            { 
                Title = "Home", 
                Slug = "index", 
                Category = "pages",
                IsDraft = false,
                FilePath = Path.Combine(_testProjectPath, "content", "pages", "home.md"),
                Contents = "# Welcome to my site"
            },
            new ContentItem 
            { 
                Title = "About", 
                Slug = "about", 
                Category = "pages",
                IsDraft = false,
                FilePath = Path.Combine(_testProjectPath, "content", "pages", "about.md"),
                Contents = "About page content"
            }
        };

        var project = new StaticSiteProject
        {
            RootFolder = _testProjectPath,
            Theme = "default",
            ProjectName = "Test"
        };

        var result = new SiteGenerationResult { StartTime = DateTime.UtcNow };
        var themeLayout = "<html><body>{{content}}</body></html>";
        
        var htmlRenderer = Substitute.For<IHtmlRenderer>();
        var indexGenerator = new IndexPageGenerator(new PostsTemplateProcessor(), htmlRenderer);

        // Act
        await indexGenerator.GenerateIndexPageAsync(contentItems, _testOutputPath, themeLayout, project, result);

        // Assert
        // Should NOT create index.html because a page with slug="index" exists
        Assert.That(result.PagesGenerated, Is.EqualTo(0));
        Assert.That(result.GeneratedFiles, Does.Not.Contain("index.html"));
    }

    [Test]
    public async Task IndexPageGenerator_ShouldGenerateDefaultIndex_WhenNoCustomIndexExists()
    {
        // Arrange
        var contentItems = new List<ContentItem>
        {
            new ContentItem 
            { 
                Title = "About", 
                Slug = "about", 
                Category = "pages",
                IsDraft = false,
                FilePath = Path.Combine(_testProjectPath, "content", "pages", "about.md"),
                Contents = "About page content",
                DatePublished = DateTime.Parse("2025-01-01")
            }
        };

        var project = new StaticSiteProject
        {
            RootFolder = _testProjectPath,
            Theme = "default",
            ProjectName = "Test"
        };

        var result = new SiteGenerationResult { StartTime = DateTime.UtcNow };
        var themeLayout = "<html><body>{{content}}</body></html>";
        
        var htmlRenderer = Substitute.For<IHtmlRenderer>();
        var indexGenerator = new IndexPageGenerator(new PostsTemplateProcessor(), htmlRenderer);

        // Act
        await indexGenerator.GenerateIndexPageAsync(contentItems, _testOutputPath, themeLayout, project, result);

        // Assert
        // Should create default index.html
        Assert.That(result.PagesGenerated, Is.EqualTo(1));
        Assert.That(result.GeneratedFiles, Does.Contain("index.html"));
        Assert.That(File.Exists(Path.Combine(_testOutputPath, "index.html")), Is.True);
    }

    [Test]
    public async Task IndexPageGenerator_ShouldRespectCaseInsensitiveSlug()
    {
        // Arrange
        var contentItems = new List<ContentItem>
        {
            new ContentItem 
            { 
                Title = "Home", 
                Slug = "INDEX", // Uppercase
                Category = "pages",
                IsDraft = false,
                FilePath = Path.Combine(_testProjectPath, "content", "pages", "home.md"),
                Contents = "# Home page"
            }
        };

        var project = new StaticSiteProject
        {
            RootFolder = _testProjectPath,
            Theme = "default",
            ProjectName = "Test"
        };

        var result = new SiteGenerationResult { StartTime = DateTime.UtcNow };
        var themeLayout = "<html><body>{{content}}</body></html>";
        
        var htmlRenderer = Substitute.For<IHtmlRenderer>();
        var indexGenerator = new IndexPageGenerator(new PostsTemplateProcessor(), htmlRenderer);

        // Act
        await indexGenerator.GenerateIndexPageAsync(contentItems, _testOutputPath, themeLayout, project, result);

        // Assert
        // Should recognize INDEX as index and skip generation
        Assert.That(result.PagesGenerated, Is.EqualTo(0));
    }

    [Test]
    public async Task IndexPageGenerator_ShouldIgnoreDraftIndexPage()
    {
        // Arrange
        var contentItems = new List<ContentItem>
        {
            new ContentItem 
            { 
                Title = "Home", 
                Slug = "index",
                Category = "pages",
                IsDraft = true, // Draft!
                FilePath = Path.Combine(_testProjectPath, "content", "pages", "home.md"),
                Contents = "# Home page"
            }
        };

        var project = new StaticSiteProject
        {
            RootFolder = _testProjectPath,
            Theme = "default",
            ProjectName = "Test"
        };

        var result = new SiteGenerationResult { StartTime = DateTime.UtcNow };
        var themeLayout = "<html><body>{{content}}</body></html>";
        
        var htmlRenderer = Substitute.For<IHtmlRenderer>();
        var indexGenerator = new IndexPageGenerator(new PostsTemplateProcessor(), htmlRenderer);

        // Act
        await indexGenerator.GenerateIndexPageAsync(contentItems, _testOutputPath, themeLayout, project, result);

        // Assert
        // Should generate default index because the custom index is a draft
        Assert.That(result.PagesGenerated, Is.EqualTo(1));
        Assert.That(result.GeneratedFiles, Does.Contain("index.html"));
    }
}
