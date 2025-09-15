using NUnit.Framework;
using MoonPress.Core;
using MoonPress.Core.Models;
using MoonPress.Core.Content;
using MoonPress.Rendering;

namespace MoonPress.Core.Tests;

[TestFixture]
public class PagesAndPostsGenerationTests
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
    public async Task GenerateSiteAsync_ShouldGeneratePagesInRootAndPostsInBlogDirectory()
    {
        // Arrange
        await CreateTestProject();
        var project = StaticSiteProject.Load(_testProjectPath);

        // Act
        var result = await _generator.GenerateSiteAsync(project, _outputPath);

        // Assert
        Assert.That(result.Success, Is.True, result.Message);
        
        // Check that pages are generated in root
        Assert.That(File.Exists(Path.Combine(_outputPath, "about.html")), Is.True, "About page should be in root");
        Assert.That(File.Exists(Path.Combine(_outputPath, "contact.html")), Is.True, "Contact page should be in root");
        
        // Check that posts are generated in blog directory
        Assert.That(Directory.Exists(Path.Combine(_outputPath, "blog")), Is.True, "Blog directory should exist");
        Assert.That(File.Exists(Path.Combine(_outputPath, "blog", "getting-started-moonpress.html")), Is.True, "Getting started post should be in blog directory");
        Assert.That(File.Exists(Path.Combine(_outputPath, "blog", "advanced-features-ssg.html")), Is.True, "Advanced features post should be in blog directory");
    }

    private async Task CreateTestProject()
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

        // Create pages
        var pagesPath = Path.Combine(_testProjectPath, "content", "pages");
        Directory.CreateDirectory(pagesPath);
        
        var aboutContent = @"---
title: About Us
slug: about
datePublished: 2025-09-15
---

# About Us
This is the about page.";
        await File.WriteAllTextAsync(Path.Combine(pagesPath, "about.md"), aboutContent);

        var contactContent = @"---
title: Contact
slug: contact
datePublished: 2025-09-15
---

# Contact Us
Get in touch with us.";
        await File.WriteAllTextAsync(Path.Combine(pagesPath, "contact.md"), contactContent);

        // Create posts
        var postsPath = Path.Combine(_testProjectPath, "content", "posts");
        Directory.CreateDirectory(postsPath);
        
        var post1Content = @"---
title: Getting Started with MoonPress
slug: getting-started-moonpress
category: Tutorials
datePublished: 2025-09-10
---

# Getting Started
This is a tutorial post.";
        await File.WriteAllTextAsync(Path.Combine(postsPath, "getting-started.md"), post1Content);

        var post2Content = @"---
title: Advanced Features in SSG
slug: advanced-features-ssg
category: Blog
datePublished: 2025-09-12
---

# Advanced Features
This is a blog post about advanced features.";
        await File.WriteAllTextAsync(Path.Combine(postsPath, "advanced-features.md"), post2Content);
    }
}