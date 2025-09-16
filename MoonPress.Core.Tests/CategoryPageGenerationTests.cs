using NUnit.Framework;
using MoonPress.Core;
using MoonPress.Core.Models;
using MoonPress.Core.Content;
using MoonPress.Rendering;

namespace MoonPress.Core.Tests;

[TestFixture]
public class CategoryPageGenerationTests
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
    public async Task GenerateSiteAsync_ShouldGenerateCategoryPages()
    {
        // Arrange
        await CreateTestProjectWithCategories();
        var project = StaticSiteProject.Load(_testProjectPath);

        // Act
        var result = await _generator.GenerateSiteAsync(project, _outputPath);

        // Assert
        Assert.That(result.Success, Is.True, result.Message);
        
        // Check that category directory exists
        Assert.That(Directory.Exists(Path.Combine(_outputPath, "category")), Is.True, "Category directory should exist");
        
        // Check that category pages are generated
        Assert.That(File.Exists(Path.Combine(_outputPath, "category", "blog.html")), Is.True, "Blog category page should exist");
        Assert.That(File.Exists(Path.Combine(_outputPath, "category", "tutorials.html")), Is.True, "Tutorials category page should exist");
        
        // Verify generated files list includes category pages
        Assert.That(result.GeneratedFiles.Contains("category/blog.html"), Is.True, "Blog category page should be in generated files");
        Assert.That(result.GeneratedFiles.Contains("category/tutorials.html"), Is.True, "Tutorials category page should be in generated files");
    }

    [Test]
    public async Task GenerateCategoryPages_ShouldContainCorrectContent()
    {
        // Arrange
        await CreateTestProjectWithCategories();
        var project = StaticSiteProject.Load(_testProjectPath);

        // Act
        var result = await _generator.GenerateSiteAsync(project, _outputPath);

        // Assert
        Assert.That(result.Success, Is.True, result.Message);
        
        // Check blog category page content
        var blogCategoryContent = await File.ReadAllTextAsync(Path.Combine(_outputPath, "category", "blog.html"));
        Assert.That(blogCategoryContent.Contains("Category: Blog"), Is.True, "Blog category page should contain category title");
        Assert.That(blogCategoryContent.Contains("Advanced Features in SSG"), Is.True, "Blog category page should contain blog post title");
        Assert.That(blogCategoryContent.Contains("/blog/advanced-features-ssg.html"), Is.True, "Blog category page should contain correct blog post URL");
        
        // Check tutorials category page content
        var tutorialsCategoryContent = await File.ReadAllTextAsync(Path.Combine(_outputPath, "category", "tutorials.html"));
        Assert.That(tutorialsCategoryContent.Contains("Category: Tutorials"), Is.True, "Tutorials category page should contain category title");
        Assert.That(tutorialsCategoryContent.Contains("Getting Started with MoonPress"), Is.True, "Tutorials category page should contain tutorial post title");
        Assert.That(tutorialsCategoryContent.Contains("/blog/getting-started-moonpress.html"), Is.True, "Tutorials category page should contain correct tutorial post URL");
    }

    [Test]
    public async Task GenerateCategoryPages_ShouldNotIncludeDraftPosts()
    {
        // Arrange
        await CreateTestProjectWithDrafts();
        var project = StaticSiteProject.Load(_testProjectPath);

        // Act
        var result = await _generator.GenerateSiteAsync(project, _outputPath);

        // Assert
        Assert.That(result.Success, Is.True, result.Message);
        
        // Check that draft category page is not generated (only draft posts in that category)
        Assert.That(File.Exists(Path.Combine(_outputPath, "category", "drafts.html")), Is.False, "Draft category page should not exist when only containing draft posts");
        
        // Check that published categories exist
        Assert.That(File.Exists(Path.Combine(_outputPath, "category", "published.html")), Is.True, "Published category page should exist");
        
        // Verify draft content is not in published category page
        var publishedCategoryContent = await File.ReadAllTextAsync(Path.Combine(_outputPath, "category", "published.html"));
        Assert.That(publishedCategoryContent.Contains("Draft Post"), Is.False, "Published category page should not contain draft posts");
    }

    private async Task CreateTestProjectWithCategories()
    {
        // Create project.json
        var projectJson = @"{
  ""ProjectName"": ""Test Project"",
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

        // Create posts with different categories
        var postsPath = Path.Combine(_testProjectPath, "content", "posts");
        Directory.CreateDirectory(postsPath);
        
        var blogPost = @"---
title: Advanced Features in SSG
slug: advanced-features-ssg
category: Blog
datePublished: 2025-09-12
summary: A blog post about advanced features
---

# Advanced Features
This is a blog post about advanced features.";
        await File.WriteAllTextAsync(Path.Combine(postsPath, "advanced-features.md"), blogPost);

        var tutorialPost = @"---
title: Getting Started with MoonPress
slug: getting-started-moonpress
category: Tutorials
datePublished: 2025-09-10
summary: A tutorial for beginners
---

# Getting Started
This is a tutorial post.";
        await File.WriteAllTextAsync(Path.Combine(postsPath, "getting-started.md"), tutorialPost);

        var anotherBlogPost = @"---
title: Web Development Tips
slug: web-dev-tips
category: Blog
datePublished: 2025-09-08
summary: Essential tips for web developers
---

# Web Development Tips
Essential tips for modern web development.";
        await File.WriteAllTextAsync(Path.Combine(postsPath, "web-dev-tips.md"), anotherBlogPost);
    }

    private async Task CreateTestProjectWithDrafts()
    {
        // Create project.json
        var projectJson = @"{
  ""ProjectName"": ""Test Project"",
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

        // Create posts with different draft status
        var postsPath = Path.Combine(_testProjectPath, "content", "posts");
        Directory.CreateDirectory(postsPath);
        
        var publishedPost = @"---
title: Published Post
slug: published-post
category: Published
datePublished: 2025-09-12
isDraft: false
---

# Published Post
This is a published post.";
        await File.WriteAllTextAsync(Path.Combine(postsPath, "published-post.md"), publishedPost);

        var draftPost = @"---
title: Draft Post
slug: draft-post
category: Drafts
datePublished: 2025-09-10
isDraft: true
---

# Draft Post
This is a draft post.";
        await File.WriteAllTextAsync(Path.Combine(postsPath, "draft-post.md"), draftPost);

        var mixedCategoryPost = @"---
title: Another Published Post
slug: another-published-post
category: Published
datePublished: 2025-09-08
isDraft: false
---

# Another Published Post
Another published post in the same category.";
        await File.WriteAllTextAsync(Path.Combine(postsPath, "another-published-post.md"), mixedCategoryPost);
    }
}