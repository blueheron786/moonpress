using MoonPress.Core.Models;
using MoonPress.Core.Generators;
using MoonPress.Core.Renderers;
using MoonPress.Core.Templates;
using NSubstitute;
using NUnit.Framework;

namespace MoonPress.Core.Tests;

[TestFixture]
public class TemplateProcessingBeforeMarkdownTests
{
    private string _testProjectPath;
    private string _testOutputPath;
    private IHtmlRenderer _htmlRenderer;
    private ContentPageGenerator _pageGenerator;

    [SetUp]
    public void Setup()
    {
        _testProjectPath = Path.Combine(Path.GetTempPath(), "moonpress_test_" + Guid.NewGuid().ToString());
        _testOutputPath = Path.Combine(_testProjectPath, "output");
        
        Directory.CreateDirectory(_testProjectPath);
        Directory.CreateDirectory(_testOutputPath);
        Directory.CreateDirectory(Path.Combine(_testProjectPath, "content", "pages"));
        Directory.CreateDirectory(Path.Combine(_testProjectPath, "themes", "default"));
        
        // Use a mock renderer that simulates markdown to HTML conversion
        _htmlRenderer = Substitute.For<IHtmlRenderer>();
        _htmlRenderer.RenderHtml(Arg.Any<ContentItem>())
            .Returns(call => 
            {
                var item = call.Arg<ContentItem>();
                // Simple mock: wrap content in div
                return $"<div class=\"content\">{item.Contents}</div>";
            });
        
        _pageGenerator = new ContentPageGenerator(_htmlRenderer, new PostsTemplateProcessor());
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
    public async Task ContentPageGenerator_ShouldProcessPostsBlocksBeforeMarkdownRendering()
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
                Contents = @"## Books

{{posts | category=""Books"" | limit=2}}
<div class=""book"">{{title}}</div>
{{/posts}}"
            },
            new ContentItem 
            { 
                Title = "Book 1", 
                Slug = "book-1",
                Category = "Books",
                IsDraft = false,
                FilePath = Path.Combine(_testProjectPath, "content", "books", "book-1.md"),
                DatePublished = DateTime.Parse("2025-01-01")
            },
            new ContentItem 
            { 
                Title = "Book 2", 
                Slug = "book-2",
                Category = "Books",
                IsDraft = false,
                FilePath = Path.Combine(_testProjectPath, "content", "books", "book-2.md"),
                DatePublished = DateTime.Parse("2025-01-02")
            }
        };

        var result = new SiteGenerationResult { StartTime = DateTime.UtcNow };
        var themeLayout = "<html><body>{{content}}</body></html>";

        // Act
        await _pageGenerator.GenerateContentPagesAsync(contentItems, _testOutputPath, themeLayout, result);

        // Assert
        var indexHtml = File.ReadAllText(Path.Combine(_testOutputPath, "index.html"));
        
        // Should contain processed book listings
        Assert.That(indexHtml, Does.Contain("<div class=\"book\">Book 2</div>"));
        Assert.That(indexHtml, Does.Contain("<div class=\"book\">Book 1</div>"));
        
        // Should NOT contain template syntax
        Assert.That(indexHtml, Does.Not.Contain("{{posts"));
        Assert.That(indexHtml, Does.Not.Contain("{{/posts}}"));
        Assert.That(indexHtml, Does.Not.Contain("{{title}}"));
    }

    [Test]
    public async Task ContentPageGenerator_ShouldPreserveMarkdownHeadings_AfterTemplateProcessing()
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
                Contents = @"## Welcome

{{posts | category=""Blog"" | limit=1}}
- {{title}}
{{/posts}}

## Footer"
            },
            new ContentItem 
            { 
                Title = "My Post", 
                Slug = "my-post",
                Category = "Blog",
                IsDraft = false,
                FilePath = Path.Combine(_testProjectPath, "content", "posts", "post.md"),
                DatePublished = DateTime.Parse("2025-01-01")
            }
        };

        var result = new SiteGenerationResult { StartTime = DateTime.UtcNow };
        var themeLayout = "<html><body>{{content}}</body></html>";

        // Act
        await _pageGenerator.GenerateContentPagesAsync(contentItems, _testOutputPath, themeLayout, result);

        // Assert
        var indexHtml = File.ReadAllText(Path.Combine(_testOutputPath, "index.html"));
        
        // Should have markdown processed (headings become HTML)
        Assert.That(indexHtml, Does.Contain("## Welcome").Or.Contain("<h2"));
        Assert.That(indexHtml, Does.Contain("## Footer").Or.Contain("Footer"));
        
        // Should have template processed (post title inserted)
        Assert.That(indexHtml, Does.Contain("My Post"));
    }

    [Test]
    public async Task ContentPageGenerator_ShouldHandleMultiplePostsBlocks_InSamePage()
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
                Contents = @"## Books
{{posts | category=""Books"" | limit=1}}
<p>{{title}}</p>
{{/posts}}

## Blog Posts
{{posts | category=""Blog"" | limit=1}}
<p>{{title}}</p>
{{/posts}}"
            },
            new ContentItem 
            { 
                Title = "My Book", 
                Slug = "book",
                Category = "Books",
                IsDraft = false,
                FilePath = Path.Combine(_testProjectPath, "content", "books", "book.md"),
                DatePublished = DateTime.Parse("2025-01-01")
            },
            new ContentItem 
            { 
                Title = "My Post", 
                Slug = "post",
                Category = "Blog",
                IsDraft = false,
                FilePath = Path.Combine(_testProjectPath, "content", "posts", "post.md"),
                DatePublished = DateTime.Parse("2025-01-01")
            }
        };

        var result = new SiteGenerationResult { StartTime = DateTime.UtcNow };
        var themeLayout = "<html><body>{{content}}</body></html>";

        // Act
        await _pageGenerator.GenerateContentPagesAsync(contentItems, _testOutputPath, themeLayout, result);

        // Assert
        var indexHtml = File.ReadAllText(Path.Combine(_testOutputPath, "index.html"));
        
        // Should contain both processed blocks
        Assert.That(indexHtml, Does.Contain("My Book"));
        Assert.That(indexHtml, Does.Contain("My Post"));
        
        // Should NOT contain any template syntax
        Assert.That(indexHtml, Does.Not.Contain("{{posts"));
        Assert.That(indexHtml, Does.Not.Contain("{{/posts}}"));
    }

    [Test]
    public async Task ContentPageGenerator_ShouldNotDoubleProcessTemplates()
    {
        // Arrange
        var contentItems = new List<ContentItem>
        {
            new ContentItem 
            { 
                Title = "Test Page", 
                Slug = "test",
                Category = "pages",
                IsDraft = false,
                FilePath = Path.Combine(_testProjectPath, "content", "pages", "test.md"),
                Contents = @"{{posts | category=""Books"" | limit=1}}
Count: {{title}}
{{/posts}}"
            },
            new ContentItem 
            { 
                Title = "Single Book", 
                Slug = "book",
                Category = "Books",
                IsDraft = false,
                FilePath = Path.Combine(_testProjectPath, "content", "books", "book.md"),
                DatePublished = DateTime.Parse("2025-01-01")
            }
        };

        var result = new SiteGenerationResult { StartTime = DateTime.UtcNow };
        var themeLayout = "<html><body>{{content}}</body></html>";

        // Act
        await _pageGenerator.GenerateContentPagesAsync(contentItems, _testOutputPath, themeLayout, result);

        // Assert
        var testHtml = File.ReadAllText(Path.Combine(_testOutputPath, "test.html"));
        
        // Should contain "Single Book" exactly once (not duplicated)
        var occurrences = System.Text.RegularExpressions.Regex.Matches(testHtml, "Single Book").Count;
        Assert.That(occurrences, Is.EqualTo(1), "Template should be processed exactly once");
    }

    [Test]
    public async Task ContentPageGenerator_ShouldWorkWithEmptyPostsResults()
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
                Contents = @"## Books

{{posts | category=""Books"" | limit=5}}
<div>{{title}}</div>
{{/posts}}

No books yet!"
            }
        };

        var result = new SiteGenerationResult { StartTime = DateTime.UtcNow };
        var themeLayout = "<html><body>{{content}}</body></html>";

        // Act
        await _pageGenerator.GenerateContentPagesAsync(contentItems, _testOutputPath, themeLayout, result);

        // Assert
        var indexHtml = File.ReadAllText(Path.Combine(_testOutputPath, "index.html"));
        
        // Should still render the page without errors
        Assert.That(indexHtml, Does.Contain("No books yet"));
        Assert.That(indexHtml, Does.Not.Contain("{{posts"));
    }
}
