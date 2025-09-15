using NUnit.Framework;
using MoonPress.Core;
using MoonPress.Core.Models;
using MoonPress.Rendering;
using System.Text;

namespace MoonPress.Core.Tests;

[TestFixture]
public class PostsFilteringIntegrationTests
{
    private StaticSiteGenerator _generator;
    private string _testDirectory;

    [SetUp]
    public void Setup()
    {
        // Clear the static cache to ensure test isolation
        MoonPress.Core.Content.ContentItemFetcher.ClearContentItems();
        
        _generator = new StaticSiteGenerator(new ContentItemHtmlRenderer());
        _testDirectory = Path.Combine(Path.GetTempPath(), $"moonpress_integration_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
        
        // Create test content structure
        var contentDir = Path.Combine(_testDirectory, "content", "pages");
        Directory.CreateDirectory(contentDir);
        
        // Create test theme
        var themeDir = Path.Combine(_testDirectory, "themes", "default");
        Directory.CreateDirectory(themeDir);
        
        // Create layout.html similar to user's
        var layoutHtml = @"<!DOCTYPE html>
<html lang=""en"">
<head>
  <meta charset=""UTF-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <title>Test Site - {{ title }}</title>
</head>
<body>
  <header>
    <nav class=""navbar"">
      <a href=""index.html"">Home</a>
      {{ navbar }}
    </nav>
  </header>
  <main>
    <div class=""content-card"">
      {{ content }}
    </div>
  </main>
</body>
</html>";
        File.WriteAllText(Path.Combine(themeDir, "layout.html"), layoutHtml);
        
        // Create index.html with posts filtering like user's
        var indexHtml = @"<h1>Test Site</h1>

<h2>📝 Latest Blog Posts</h2>

<ul>
{{ posts | category=""blog"" | limit=5 }}
  <li><a href=""{{ slug }}.html"">{{ title }}</a></li>
{{ /posts }}
</ul>

<h2>📖 Latest Books</h2>

<ul>
{{ posts | category=""books"" | limit=3 }}
  <li><a href=""{{ slug }}.html"">{{ title }}</a></li>
{{ /posts }}
</ul>";
        File.WriteAllText(Path.Combine(themeDir, "index.html"), indexHtml);
        
        // Create test blog posts
        var blogPost1 = @"---
id: blog-post-1
title: First Blog Post
slug: first-blog-post
category: blog
datePublished: 2025-09-10 10:00:00
isDraft: false
---
# First Blog Post
This is the first blog post.";
        File.WriteAllText(Path.Combine(contentDir, "blog-post-1.md"), blogPost1);
        
        var blogPost2 = @"---
id: blog-post-2
title: Second Blog Post
slug: second-blog-post
category: blog
datePublished: 2025-09-12 10:00:00
isDraft: false
---
# Second Blog Post
This is the second blog post.";
        File.WriteAllText(Path.Combine(contentDir, "blog-post-2.md"), blogPost2);
        
        // Create test book
        var book1 = @"---
id: book-1
title: Amazing Book
slug: amazing-book
category: books
datePublished: 2025-09-08 10:00:00
isDraft: false
---
# Amazing Book
This is an amazing book.";
        File.WriteAllText(Path.Combine(contentDir, "book-1.md"), book1);
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
    public async Task GenerateSite_WithPostsFiltering_RendersCorrectContent()
    {
        // Arrange
        var project = new StaticSiteProject
        {
            RootFolder = _testDirectory,
            Theme = "default",
            ProjectName = "Test Site"
        };
        
        var outputDir = Path.Combine(_testDirectory, "output");
        
        // Act
        var result = await _generator.GenerateSiteAsync(project, outputDir);
        
        // Assert
        Assert.That(result.Success, Is.True, $"Site generation failed: {result.Message}");
        
        // Check that index.html was generated
        var indexPath = Path.Combine(outputDir, "index.html");
        Assert.That(File.Exists(indexPath), Is.True, "index.html was not generated");
        
        var indexContent = await File.ReadAllTextAsync(indexPath);
        
        // Check that blog posts are filtered correctly
        Assert.That(indexContent, Does.Contain("first-blog-post.html"), "First blog post link not found");
        Assert.That(indexContent, Does.Contain("Second Blog Post"), "Second blog post title not found");
        
        // Check that books are filtered correctly
        Assert.That(indexContent, Does.Contain("amazing-book.html"), "Book link not found");
        Assert.That(indexContent, Does.Contain("Amazing Book"), "Book title not found");
        
        // Verify the structure
        Assert.That(indexContent, Does.Contain("📝 Latest Blog Posts"), "Blog posts section header not found");
        Assert.That(indexContent, Does.Contain("📖 Latest Books"), "Books section header not found");
        
        Console.WriteLine("Generated index.html content:");
        Console.WriteLine(indexContent);
    }
}