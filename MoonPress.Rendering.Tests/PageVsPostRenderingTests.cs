using MoonPress.Core.Models;
using MoonPress.Rendering;
using NUnit.Framework;

namespace MoonPress.Rendering.Tests;

[TestFixture]
public class PageVsPostRenderingTests
{
    private ContentItemHtmlRenderer _renderer;
    private string _testProjectPath;

    [SetUp]
    public void Setup()
    {
        _renderer = new ContentItemHtmlRenderer();
        _testProjectPath = Path.Combine(Path.GetTempPath(), "moonpress_test_" + Guid.NewGuid().ToString());
        
        Directory.CreateDirectory(_testProjectPath);
        Directory.CreateDirectory(Path.Combine(_testProjectPath, "content", "pages"));
        Directory.CreateDirectory(Path.Combine(_testProjectPath, "content", "posts"));
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
    public void RenderHtml_PageShouldNotIncludeTitleAndDate()
    {
        // Arrange
        var pageItem = new ContentItem
        {
            Title = "About Us",
            DatePublished = DateTime.Parse("2025-01-01 10:00:00"),
            Contents = "## Our Story\n\nWe are awesome!",
            FilePath = Path.Combine(_testProjectPath, "content", "pages", "about.md")
        };

        // Act
        var result = _renderer.RenderHtml(pageItem);

        // Assert
        Assert.That(result, Does.Not.Contain("<h1>About Us</h1>"), "Pages should not have auto-generated H1 title");
        Assert.That(result, Does.Not.Contain("Published on:"), "Pages should not show publication date");
        Assert.That(result, Does.Contain("<div class=\"content\">"), "Should contain content div");
        Assert.That(result, Does.Contain("Our Story"), "Should contain the actual content");
    }

    [Test]
    public void RenderHtml_PostShouldIncludeTitleAndDate()
    {
        // Arrange
        var postItem = new ContentItem
        {
            Title = "My First Post",
            DatePublished = DateTime.Parse("2025-09-15 14:30:00"),
            Contents = "This is my **first** blog post!",
            FilePath = Path.Combine(_testProjectPath, "content", "posts", "first-post.md")
        };

        // Act
        var result = _renderer.RenderHtml(postItem);

        // Assert
        Assert.That(result, Does.Contain("<h1>My First Post</h1>"), "Posts should have auto-generated H1 title");
        Assert.That(result, Does.Contain("Published on: 2025-09-15"), "Posts should show publication date");
        Assert.That(result, Does.Contain("<div class=\"content\">"), "Should contain content div");
        Assert.That(result, Does.Contain("first"), "Should contain the actual content");
    }

    [Test]
    public void RenderHtml_ArticleShouldIncludeTitleAndDate()
    {
        // Arrange
        var articleItem = new ContentItem
        {
            Title = "Important Article",
            DatePublished = DateTime.Parse("2025-01-20 09:00:00"),
            Contents = "# This is important\n\nRead this!",
            FilePath = Path.Combine(_testProjectPath, "content", "articles", "important.md")
        };

        // Act
        var result = _renderer.RenderHtml(articleItem);

        // Assert
        Assert.That(result, Does.Contain("<h1>Important Article</h1>"), "Articles should have auto-generated H1 title");
        Assert.That(result, Does.Contain("Published on:"), "Articles should show publication date");
    }

    [Test]
    public void RenderHtml_BookCategoryShouldIncludeTitleAndDate()
    {
        // Arrange
        var bookItem = new ContentItem
        {
            Title = "The Great Novel",
            DatePublished = DateTime.Parse("2025-06-01 12:00:00"),
            Contents = "A story about...",
            FilePath = Path.Combine(_testProjectPath, "content", "books", "great-novel.md"),
            Category = "Books"
        };

        // Act
        var result = _renderer.RenderHtml(bookItem);

        // Assert
        Assert.That(result, Does.Contain("<h1>The Great Novel</h1>"), "Books should have auto-generated H1 title");
        Assert.That(result, Does.Contain("Published on:"), "Books should show publication date");
    }

    [Test]
    public void RenderHtml_PageWithMarkdownShouldRenderCorrectly()
    {
        // Arrange
        var pageItem = new ContentItem
        {
            Title = "Features",
            DatePublished = DateTime.Parse("2025-01-01"),
            Contents = @"## Key Features

- **Bold** feature 1
- *Italic* feature 2
- [Link](https://example.com)

### Subsection

Some paragraph text.",
            FilePath = Path.Combine(_testProjectPath, "content", "pages", "features.md")
        };

        // Act
        var result = _renderer.RenderHtml(pageItem);

        // Assert
        Assert.That(result, Does.Not.Contain("## Key Features"), "Markdown headers should be converted");
        Assert.That(result, Does.Contain("<h2"), "Should contain H2 tag");
        Assert.That(result, Does.Contain("<strong>"), "Bold should be converted");
        Assert.That(result, Does.Contain("<em>"), "Italic should be converted");
        Assert.That(result, Does.Contain("<a href"), "Links should be converted");
        Assert.That(result, Does.Not.Contain("Published on:"), "Pages should not show date");
    }

    [Test]
    public void RenderHtml_PostWithMarkdownShouldRenderCorrectly()
    {
        // Arrange
        var postItem = new ContentItem
        {
            Title = "My Blog Post",
            DatePublished = DateTime.Parse("2025-03-15 10:30:00"),
            Contents = @"This is a **great** post!

Here's a list:
- Item 1
- Item 2",
            FilePath = Path.Combine(_testProjectPath, "content", "posts", "blog-post.md")
        };

        // Act
        var result = _renderer.RenderHtml(postItem);

        // Assert
        Assert.That(result, Does.Contain("<h1>My Blog Post</h1>"), "Should have title");
        Assert.That(result, Does.Contain("Published on: 2025-03-15"), "Should have formatted date");
        Assert.That(result, Does.Contain("<strong>great</strong>"), "Markdown should be processed");
        Assert.That(result, Does.Contain("<li>Item 1</li>"), "Lists should be converted");
    }

    [Test]
    public void RenderHtml_EmptyContentShouldNotThrow()
    {
        // Arrange
        var pageItem = new ContentItem
        {
            Title = "Empty Page",
            DatePublished = DateTime.Parse("2025-01-01"),
            Contents = "",
            FilePath = Path.Combine(_testProjectPath, "content", "pages", "empty.md")
        };

        // Act & Assert
        Assert.DoesNotThrow(() => _renderer.RenderHtml(pageItem));
        var result = _renderer.RenderHtml(pageItem);
        Assert.That(result, Does.Contain("<div class=\"content\">"));
    }

    [Test]
    public void RenderHtml_NullContentShouldNotThrow()
    {
        // Arrange
        var pageItem = new ContentItem
        {
            Title = "Null Content Page",
            DatePublished = DateTime.Parse("2025-01-01"),
            Contents = null,
            FilePath = Path.Combine(_testProjectPath, "content", "pages", "null.md")
        };

        // Act & Assert
        Assert.DoesNotThrow(() => _renderer.RenderHtml(pageItem));
        var result = _renderer.RenderHtml(pageItem);
        Assert.That(result, Does.Contain("<div class=\"content\">"));
    }

    [Test]
    public void RenderHtml_PagePathCaseSensitivityShouldWork()
    {
        // Arrange - Windows uses backslashes, but test both
        var pageItemBackslash = new ContentItem
        {
            Title = "Test Page",
            DatePublished = DateTime.Parse("2025-01-01"),
            Contents = "Content",
            FilePath = @"d:\projects\content\pages\test.md"
        };

        var pageItemForwardslash = new ContentItem
        {
            Title = "Test Page",
            DatePublished = DateTime.Parse("2025-01-01"),
            Contents = "Content",
            FilePath = "d:/projects/content/pages/test.md"
        };

        // Act
        var resultBackslash = _renderer.RenderHtml(pageItemBackslash);
        var resultForwardslash = _renderer.RenderHtml(pageItemForwardslash);

        // Assert - Both should be recognized as pages
        Assert.That(resultBackslash, Does.Not.Contain("Published on:"));
        Assert.That(resultForwardslash, Does.Not.Contain("Published on:"));
    }
}
