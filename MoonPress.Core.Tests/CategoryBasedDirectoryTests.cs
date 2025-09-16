using NUnit.Framework;
using MoonPress.Core;
using MoonPress.Core.Models;
using MoonPress.Core.Content;
using MoonPress.Rendering;

namespace MoonPress.Core.Tests;

[TestFixture]
public class CategoryBasedDirectoryTests
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
        
        _testProjectPath = Path.Combine(Path.GetTempPath(), "moonpress_category_test_" + Guid.NewGuid().ToString());
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
    public async Task GenerateSiteAsync_ShouldUseCategoryBasedDirectoriesForAllContentTypes()
    {
        // Arrange
        await CreateTestProjectWithVariousContentTypes();
        var project = StaticSiteProject.Load(_testProjectPath);

        // Act
        var result = await _generator.GenerateSiteAsync(project, _outputPath);

        // Assert
        Assert.That(result.Success, Is.True, result.Message);
        
        // Check that pages are generated in root (no category-based directories for pages)
        Assert.That(File.Exists(Path.Combine(_outputPath, "about.html")), Is.True, "About page should be in root");
        
        // Check that posts are generated in category directories
        Assert.That(Directory.Exists(Path.Combine(_outputPath, "blog")), Is.True, "Blog directory should exist");
        Assert.That(File.Exists(Path.Combine(_outputPath, "blog", "my-blog-post.html")), Is.True, "Blog post should be in blog directory");
        
        // Check that books are generated in category directories
        Assert.That(Directory.Exists(Path.Combine(_outputPath, "fantasy")), Is.True, "Fantasy directory should exist");
        Assert.That(File.Exists(Path.Combine(_outputPath, "fantasy", "fantasy-novel.html")), Is.True, "Fantasy book should be in fantasy directory");
        
        Assert.That(Directory.Exists(Path.Combine(_outputPath, "programming")), Is.True, "Programming directory should exist");
        Assert.That(File.Exists(Path.Combine(_outputPath, "programming", "programming-guide.html")), Is.True, "Programming book should be in programming directory");
        
        // Check that articles are generated in category directories
        Assert.That(Directory.Exists(Path.Combine(_outputPath, "tutorial")), Is.True, "Tutorial directory should exist");
        Assert.That(File.Exists(Path.Combine(_outputPath, "tutorial", "how-to-article.html")), Is.True, "Tutorial article should be in tutorial directory");
        
        // Check that content without category goes to uncategorized
        Assert.That(Directory.Exists(Path.Combine(_outputPath, "uncategorized")), Is.True, "Uncategorized directory should exist");
        Assert.That(File.Exists(Path.Combine(_outputPath, "uncategorized", "no-category-post.html")), Is.True, "Content without category should be in uncategorized directory");
    }

    private async Task CreateTestProjectWithVariousContentTypes()
    {
        // Create project.json
        var projectJson = @"{
  ""ProjectName"": ""Category Test Project"",
  ""Theme"": ""default"",
  ""CreatedOn"": ""2025-09-15T00:00:00Z""
}";
        await File.WriteAllTextAsync(Path.Combine(_testProjectPath, "project.json"), projectJson);

        // Create theme
        var themePath = Path.Combine(_testProjectPath, "themes", "default");
        Directory.CreateDirectory(themePath);
        
        var layoutContent = @"<!DOCTYPE html>
<html>
<head><title>{{title}}</title></head>
<body>
    <nav>{{navbar}}</nav>
    <main>{{content}}</main>
</body>
</html>";
        await File.WriteAllTextAsync(Path.Combine(themePath, "layout.html"), layoutContent);

        // Create page (should go in root)
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

        // Create post (should use category directory)
        var postsPath = Path.Combine(_testProjectPath, "content", "posts");
        Directory.CreateDirectory(postsPath);
        
        var postContent = @"---
title: My Blog Post
slug: my-blog-post
category: Blog
datePublished: 2025-09-10
---

# My Blog Post
This is a blog post.";
        await File.WriteAllTextAsync(Path.Combine(postsPath, "blog-post.md"), postContent);

        // Create post without category (should go to uncategorized)
        var noCategoryPost = @"---
title: No Category Post
slug: no-category-post
datePublished: 2025-09-10
---

# No Category Post
This post has no category.";
        await File.WriteAllTextAsync(Path.Combine(postsPath, "no-category.md"), noCategoryPost);

        // Create books (should use category directories)
        var booksPath = Path.Combine(_testProjectPath, "content", "books");
        Directory.CreateDirectory(booksPath);
        
        var fantasyBook = @"---
title: Fantasy Novel
slug: fantasy-novel
category: Fantasy
datePublished: 2025-09-01
---

# Fantasy Novel
A great fantasy story.";
        await File.WriteAllTextAsync(Path.Combine(booksPath, "fantasy.md"), fantasyBook);

        var programmingBook = @"---
title: Programming Guide
slug: programming-guide
category: Programming
datePublished: 2025-08-15
---

# Programming Guide
Learn to code.";
        await File.WriteAllTextAsync(Path.Combine(booksPath, "programming.md"), programmingBook);

        // Create articles (should use category directories)
        var articlesPath = Path.Combine(_testProjectPath, "content", "articles");
        Directory.CreateDirectory(articlesPath);
        
        var tutorialArticle = @"---
title: How To Article
slug: how-to-article
category: Tutorial
datePublished: 2025-07-20
---

# How To Article
A step-by-step guide.";
        await File.WriteAllTextAsync(Path.Combine(articlesPath, "tutorial.md"), tutorialArticle);
    }
}