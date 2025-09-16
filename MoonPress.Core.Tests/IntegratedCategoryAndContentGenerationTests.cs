using NUnit.Framework;
using MoonPress.Core;
using MoonPress.Core.Models;
using MoonPress.Core.Content;
using MoonPress.Rendering;

namespace MoonPress.Core.Tests;

[TestFixture]
public class IntegratedCategoryAndContentGenerationTests
{
    private StaticSiteGenerator _generator = null!;
    private string _testProjectPath = null!;
    private string _outputPath = null!;

    [SetUp]
    public void Setup()
    {
        // Clear content cache to ensure clean state for each test
        ContentItemFetcher.ClearContentItems();
        
        var htmlRenderer = new ContentItemHtmlRenderer();
        _generator = new StaticSiteGenerator(htmlRenderer);
        
        _testProjectPath = Path.Combine(Path.GetTempPath(), "moonpress_test_" + Guid.NewGuid().ToString());
        _outputPath = Path.Combine(Path.GetTempPath(), "moonpress_output_" + Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testProjectPath);
        Directory.CreateDirectory(_outputPath);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_testProjectPath))
            Directory.Delete(_testProjectPath, true);
        if (Directory.Exists(_outputPath))
            Directory.Delete(_outputPath, true);
    }

    [Test]
    public async Task GenerateSiteAsync_ShouldGenerateCompleteStaticSiteWithAllFeatures()
    {
        // Arrange - Create a complete project with pages, posts, and categories
        await CreateCompleteTestProject();
        var project = StaticSiteProject.Load(_testProjectPath);

        // Act
        var result = await _generator.GenerateSiteAsync(project, _outputPath);

        // Assert - Verify site generation was successful
        Assert.That(result.Success, Is.True, result.Message);
        
        // Verify pages are generated correctly
        Assert.That(File.Exists(Path.Combine(_outputPath, "about.html")), Is.True, "About page should exist");
        Assert.That(File.Exists(Path.Combine(_outputPath, "contact.html")), Is.True, "Contact page should exist");
        
        // Verify posts are generated in their respective category directories
        Assert.That(Directory.Exists(Path.Combine(_outputPath, "tutorials")), Is.True, "Tutorials directory should exist");
        Assert.That(File.Exists(Path.Combine(_outputPath, "tutorials", "tutorial-post.html")), Is.True, "Tutorial post should exist in tutorials directory");
        Assert.That(Directory.Exists(Path.Combine(_outputPath, "blog")), Is.True, "Blog directory should exist");
        Assert.That(File.Exists(Path.Combine(_outputPath, "blog", "blog-post.html")), Is.True, "Blog post should exist in blog directory");
        
        // Verify category pages are generated
        Assert.That(Directory.Exists(Path.Combine(_outputPath, "category")), Is.True, "Category directory should exist");
        Assert.That(File.Exists(Path.Combine(_outputPath, "category", "tutorials.html")), Is.True, "Tutorials category page should exist");
        Assert.That(File.Exists(Path.Combine(_outputPath, "category", "blog.html")), Is.True, "Blog category page should exist");
        
        // Verify index page exists
        Assert.That(File.Exists(Path.Combine(_outputPath, "index.html")), Is.True, "Index page should exist");
        
        // Verify the correct number of pages were generated
        // 2 pages + 2 posts + 2 category pages + 1 index = 7 pages
        Assert.That(result.PagesGenerated, Is.EqualTo(7), $"Expected 7 pages to be generated, but got {result.PagesGenerated}");
    }

    [Test]
    public async Task CategoryPages_ShouldContainCorrectLinksToContent()
    {
        // Arrange
        await CreateCompleteTestProject();
        var project = StaticSiteProject.Load(_testProjectPath);

        // Act
        var result = await _generator.GenerateSiteAsync(project, _outputPath);

        // Assert
        Assert.That(result.Success, Is.True, result.Message);
        
        // Check blog category page content and links
        var blogCategoryContent = await File.ReadAllTextAsync(Path.Combine(_outputPath, "category", "blog.html"));
        Assert.That(blogCategoryContent.Contains("Category: Blog"), Is.True, "Blog category page should contain category title");
        Assert.That(blogCategoryContent.Contains("/blog/blog-post.html"), Is.True, "Blog category page should link to blog post with correct path");
        Assert.That(blogCategoryContent.Contains("Sample Blog Post"), Is.True, "Blog category page should contain blog post title");
        
        // Check tutorials category page content and links
        var tutorialsCategoryContent = await File.ReadAllTextAsync(Path.Combine(_outputPath, "category", "tutorials.html"));
        Assert.That(tutorialsCategoryContent.Contains("Category: Tutorials"), Is.True, "Tutorials category page should contain category title");
        Assert.That(tutorialsCategoryContent.Contains("/tutorials/tutorial-post.html"), Is.True, "Tutorials category page should link to tutorial post with correct path");
        Assert.That(tutorialsCategoryContent.Contains("Sample Tutorial Post"), Is.True, "Tutorials category page should contain tutorial post title");
    }

    private async Task CreateCompleteTestProject()
    {
        // Create project.json
        var projectJson = @"{
  ""ProjectName"": ""Complete Test Project"",
  ""Theme"": ""default"",
  ""CreatedOn"": ""2025-09-15T00:00:00Z""
}";
        await File.WriteAllTextAsync(Path.Combine(_testProjectPath, "project.json"), projectJson);

        // Create theme
        var themePath = Path.Combine(_testProjectPath, "themes", "default");
        Directory.CreateDirectory(themePath);
        
        var layoutContent = @"<!DOCTYPE html>
<html>
<head><title>{{ title }}</title></head>
<body>
    <nav>{{ navbar }}</nav>
    <main>{{ content }}</main>
</body>
</html>";
        await File.WriteAllTextAsync(Path.Combine(themePath, "layout.html"), layoutContent);

        // Create pages
        var pagesPath = Path.Combine(_testProjectPath, "content", "pages");
        Directory.CreateDirectory(pagesPath);
        
        var aboutContent = @"---
title: About Us
slug: about
datePublished: 2025-09-15
---

# About Us
This is the about page for our website.";
        await File.WriteAllTextAsync(Path.Combine(pagesPath, "about.md"), aboutContent);

        var contactContent = @"---
title: Contact
slug: contact
datePublished: 2025-09-15
---

# Contact Us
Get in touch with us through our contact form.";
        await File.WriteAllTextAsync(Path.Combine(pagesPath, "contact.md"), contactContent);

        // Create posts with categories
        var postsPath = Path.Combine(_testProjectPath, "content", "posts");
        Directory.CreateDirectory(postsPath);
        
        var tutorialPost = @"---
title: Sample Tutorial Post
slug: tutorial-post
category: Tutorials
datePublished: 2025-09-10
summary: A helpful tutorial
---

# Tutorial
This is a tutorial post in the Tutorials category.";
        await File.WriteAllTextAsync(Path.Combine(postsPath, "tutorial.md"), tutorialPost);

        var blogPost = @"---
title: Sample Blog Post
slug: blog-post
category: Blog
datePublished: 2025-09-12
summary: An interesting blog post
---

# Blog Post
This is a blog post in the Blog category.";
        await File.WriteAllTextAsync(Path.Combine(postsPath, "blog.md"), blogPost);
    }
}