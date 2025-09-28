using MoonPress.Core;
using MoonPress.Core.Models;
using MoonPress.Core.Templates;
using MoonPress.Rendering;
using NUnit.Framework;
using System.IO;

namespace MoonPress.Core.Tests;

[TestFixture]
public class IndexPagePostsFilterTests
{
    private string _testDirectory;
    private StaticSiteGenerator _generator;

    [SetUp]
    public void Setup()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);
        
        var htmlRenderer = new ContentItemHtmlRenderer();
        _generator = new StaticSiteGenerator(htmlRenderer);
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
    public async Task GenerateIndexPage_ShouldProcessPostsBlocksInThemeLayout()
    {
        // Arrange
        var project = CreateTestProject();
        await CreateContentItems();
        CreateThemeWithPostsInLayout();

        // Act
        var outputPath = Path.Combine(_testDirectory, "output");
        var result = await _generator.GenerateSiteAsync(project, outputPath);

        // Assert
        Assert.That(result.Success, Is.True, $"Generation failed: {result.Message}");
        
        var indexPath = Path.Combine(outputPath, "index.html");
        Assert.That(File.Exists(indexPath), Is.True);
        
        var indexContent = await File.ReadAllTextAsync(indexPath);
        
        // Should contain the filtered blog posts
        Assert.That(indexContent, Does.Contain("Test Blog Post"));
        Assert.That(indexContent, Does.Not.Contain("Test News Post")); // Different category
        Assert.That(indexContent, Does.Contain("href=\"/blog/test-blog-post.html\""));
    }

    [Test]
    public async Task GenerateIndexPage_ShouldProcessPostsBlocksInIndexTemplate()
    {
        // Arrange
        var project = CreateTestProject();
        await CreateContentItems();
        CreateThemeWithPostsInIndexTemplate();

        // Act
        var outputPath = Path.Combine(_testDirectory, "output");
        var result = await _generator.GenerateSiteAsync(project, outputPath);

        // Assert
        Assert.That(result.Success, Is.True, $"Generation failed: {result.Message}");
        
        var indexPath = Path.Combine(outputPath, "index.html");
        Assert.That(File.Exists(indexPath), Is.True);
        
        var indexContent = await File.ReadAllTextAsync(indexPath);
        
        // Should contain limited posts
        Assert.That(indexContent, Does.Contain("Test Blog Post"));
        Assert.That(indexContent, Does.Contain("Test News Post"));
    }

    [Test]
    public async Task GenerateIndexPage_ShouldProcessMarkdownInIndexTemplate()
    {
        // Arrange
        var project = CreateTestProject();
        await CreateContentItems();
        CreateThemeWithMarkdownInIndexTemplate();

        // Act
        var outputPath = Path.Combine(_testDirectory, "output");
        var result = await _generator.GenerateSiteAsync(project, outputPath);

        // Assert
        Assert.That(result.Success, Is.True, $"Generation failed: {result.Message}");
        
        var indexPath = Path.Combine(outputPath, "index.html");
        Assert.That(File.Exists(indexPath), Is.True);
        
        var indexContent = await File.ReadAllTextAsync(indexPath);
        
        // Should contain converted markdown bullet points as HTML
        Assert.That(indexContent, Does.Contain("<ul>"));
        Assert.That(indexContent, Does.Contain("<li>First newsletter item</li>"));
        Assert.That(indexContent, Does.Contain("<li>Second newsletter item</li>"));
        Assert.That(indexContent, Does.Contain("<li>Third newsletter item</li>"));
        Assert.That(indexContent, Does.Contain("</ul>"));
        
        // Should not contain raw markdown syntax
        Assert.That(indexContent, Does.Not.Contain("- First newsletter item"));
        Assert.That(indexContent, Does.Not.Contain("- Second newsletter item"));
        Assert.That(indexContent, Does.Not.Contain("- Third newsletter item"));
    }

    private StaticSiteProject CreateTestProject()
    {
        return new StaticSiteProject
        {
            RootFolder = _testDirectory,
            Theme = "default",
            ProjectName = "Test Site"
        };
    }

    private async Task CreateContentItems()
    {
        var contentDir = Path.Combine(_testDirectory, "content");
        Directory.CreateDirectory(contentDir);

        // Create a blog post
        var blogPost = @"---
title: Test Blog Post
category: blog
slug: test-blog-post
---
This is a test blog post.";
        await File.WriteAllTextAsync(Path.Combine(contentDir, "blog-post.md"), blogPost);

        // Create a news post
        var newsPost = @"---
title: Test News Post
category: news
slug: test-news-post
---
This is a test news post.";
        await File.WriteAllTextAsync(Path.Combine(contentDir, "news-post.md"), newsPost);
    }

    private void CreateThemeWithPostsInLayout()
    {
        var themePath = Path.Combine(_testDirectory, "themes", "default");
        Directory.CreateDirectory(themePath);

        var layoutHtml = @"<!DOCTYPE html>
<html>
<head><title>{{title}}</title></head>
<body>
    <nav>{{navbar}}</nav>
    <main>
        {{content}}
        <section class=""recent-blog-posts"">
            <h2>Recent Blog Posts</h2>
            {{posts | category=""blog"" | limit=3}}
              <div><a href=""{{url}}"">{{title}}</a></div>
            {{/posts}}
        </section>
    </main>
</body>
</html>";

        File.WriteAllText(Path.Combine(themePath, "layout.html"), layoutHtml);

        var indexHtml = @"<h1>Welcome to the site</h1>";
        File.WriteAllText(Path.Combine(themePath, "index.html"), indexHtml);
    }

    private void CreateThemeWithPostsInIndexTemplate()
    {
        var themePath = Path.Combine(_testDirectory, "themes", "default");
        Directory.CreateDirectory(themePath);

        var layoutHtml = @"<!DOCTYPE html>
<html>
<head><title>{{title}}</title></head>
<body>
    <nav>{{navbar}}</nav>
    <main>{{content}}</main>
</body>
</html>";

        File.WriteAllText(Path.Combine(themePath, "layout.html"), layoutHtml);

        var indexHtml = @"<h1>Welcome to the site</h1>
{{posts | limit=2}}
  <article>{{title}} ({{category}})</article>
{{/posts}}";
        File.WriteAllText(Path.Combine(themePath, "index.html"), indexHtml);
    }

    private void CreateThemeWithMarkdownInIndexTemplate()
    {
        var themePath = Path.Combine(_testDirectory, "themes", "default");
        Directory.CreateDirectory(themePath);

        var layoutHtml = @"<!DOCTYPE html>
<html>
<head><title>{{title}}</title></head>
<body>
    <nav>{{navbar}}</nav>
    <main>{{content}}</main>
</body>
</html>";

        File.WriteAllText(Path.Combine(themePath, "layout.html"), layoutHtml);

        var indexHtml = @"# Welcome to My Site

Join our newsletter to receive:

- First newsletter item
- Second newsletter item  
- Third newsletter item

Thanks for visiting!";
        File.WriteAllText(Path.Combine(themePath, "index.html"), indexHtml);
    }
}