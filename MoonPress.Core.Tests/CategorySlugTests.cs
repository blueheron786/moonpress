using MoonPress.Core;
using MoonPress.Core.Models;
using MoonPress.Rendering;
using NUnit.Framework;

namespace MoonPress.Core.Tests;

[TestFixture]
public class CategorySlugTests
{
    private StaticSiteGenerator _generator;
    private string _testProjectPath;
    private string _outputPath;

    [SetUp]
    public void Setup()
    {
        // Clear the static cache to ensure test isolation
        MoonPress.Core.Content.ContentItemFetcher.ClearContentItems();
        
        _generator = new StaticSiteGenerator(new ContentItemHtmlRenderer());
        _testProjectPath = Path.Combine(Path.GetTempPath(), "moonpress_test_" + Guid.NewGuid().ToString());
        _outputPath = Path.Combine(_testProjectPath, "output");
        
        Directory.CreateDirectory(_testProjectPath);
        Directory.CreateDirectory(_outputPath);
        Directory.CreateDirectory(Path.Combine(_testProjectPath, "content"));
        Directory.CreateDirectory(Path.Combine(_testProjectPath, "themes", "default"));
        
        // Create a minimal layout.html
        File.WriteAllText(
            Path.Combine(_testProjectPath, "themes", "default", "layout.html"),
            "<html><head><title>{{title}}</title></head><body><nav>{{navbar}}</nav>{{content}}</body></html>"
        );
        
        // Create a minimal index.html
        File.WriteAllText(
            Path.Combine(_testProjectPath, "themes", "default", "index.html"),
            "<h1>Test Site</h1>"
        );
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
    public async Task GenerateSiteAsync_CategoryWithSpaces_ShouldConvertToHyphenatedSlug()
    {
        // Arrange
        var contentFile = Path.Combine(_testProjectPath, "content", "the-little-lantern.md");
        File.WriteAllText(contentFile, @"---
id: the-little-lantern
title: The Little Lantern
slug: the-little-lantern
category: Jannah Journeys
datePublished: 2025-01-15 10:00:00
---
An audio adventure!");

        var project = new StaticSiteProject
        {
            RootFolder = _testProjectPath,
            Theme = "default"
        };

        // Act
        var result = await _generator.GenerateSiteAsync(project, _outputPath);

        // Assert
        Assert.That(result.Success, Is.True, $"Generation should succeed. Message: {result.Message}");
        
        // Check that the file was created in the correct folder with hyphenated category name
        var expectedFolder = Path.Combine(_outputPath, "jannah-journeys");
        var expectedFile = Path.Combine(expectedFolder, "the-little-lantern.html");
        
        Assert.That(Directory.Exists(expectedFolder), Is.True, 
            $"Category folder should be 'jannah-journeys', not 'jannah journeys'");
        Assert.That(File.Exists(expectedFile), Is.True, 
            "File should exist in the hyphenated category folder");
        
        // Check that the relative path in generated files list is also hyphenated
        Assert.That(result.GeneratedFiles, Does.Contain("jannah-journeys/the-little-lantern.html"),
            "Generated files list should use hyphenated category name");
    }

    [Test]
    public async Task GenerateSiteAsync_CategoryWithMultipleSpaces_ShouldConvertAllSpacesToHyphens()
    {
        // Arrange
        var contentFile = Path.Combine(_testProjectPath, "content", "test-post.md");
        File.WriteAllText(contentFile, @"---
id: test-post
title: Test Post
slug: test-post
category: Multiple Word Category Name
datePublished: 2025-01-15 10:00:00
---
Test content");

        var project = new StaticSiteProject
        {
            RootFolder = _testProjectPath,
            Theme = "default"
        };

        // Act
        var result = await _generator.GenerateSiteAsync(project, _outputPath);

        // Assert
        var expectedFolder = Path.Combine(_outputPath, "multiple-word-category-name");
        Assert.That(Directory.Exists(expectedFolder), Is.True, 
            "All spaces in category name should be converted to hyphens");
        
        var expectedFile = Path.Combine(expectedFolder, "test-post.html");
        Assert.That(File.Exists(expectedFile), Is.True);
    }

    [Test]
    public async Task GenerateSiteAsync_CategoryWithUpperCase_ShouldConvertToLowerCase()
    {
        // Arrange
        var contentFile = Path.Combine(_testProjectPath, "content", "test-book.md");
        File.WriteAllText(contentFile, @"---
id: test-book
title: Test Book
slug: test-book
category: Books
datePublished: 2025-01-15 10:00:00
---
Test content");

        var project = new StaticSiteProject
        {
            RootFolder = _testProjectPath,
            Theme = "default"
        };

        // Act
        var result = await _generator.GenerateSiteAsync(project, _outputPath);

        // Assert
        var expectedFolder = Path.Combine(_outputPath, "books");
        Assert.That(Directory.Exists(expectedFolder), Is.True, 
            "Category name should be converted to lowercase");
        
        var expectedFile = Path.Combine(expectedFolder, "test-book.html");
        Assert.That(File.Exists(expectedFile), Is.True);
    }

    [Test]
    public async Task GenerateSiteAsync_CategoryWithMixedCaseAndSpaces_ShouldConvertCorrectly()
    {
        // Arrange
        var contentFile = Path.Combine(_testProjectPath, "content", "test-post.md");
        File.WriteAllText(contentFile, @"---
id: test-post
title: Test Post
slug: test-post
category: Audio Stories
datePublished: 2025-01-15 10:00:00
---
Test content");

        var project = new StaticSiteProject
        {
            RootFolder = _testProjectPath,
            Theme = "default"
        };

        // Act
        var result = await _generator.GenerateSiteAsync(project, _outputPath);

        // Assert
        var expectedFolder = Path.Combine(_outputPath, "audio-stories");
        Assert.That(Directory.Exists(expectedFolder), Is.True, 
            "Category 'Audio Stories' should become 'audio-stories'");
        
        var expectedFile = Path.Combine(expectedFolder, "test-post.html");
        Assert.That(File.Exists(expectedFile), Is.True);
    }

    [Test]
    public async Task GenerateSiteAsync_CategoryAlreadyHyphenated_ShouldNotDouble()
    {
        // Arrange
        var contentFile = Path.Combine(_testProjectPath, "content", "test-post.md");
        File.WriteAllText(contentFile, @"---
id: test-post
title: Test Post
slug: test-post
category: already-hyphenated
datePublished: 2025-01-15 10:00:00
---
Test content");

        var project = new StaticSiteProject
        {
            RootFolder = _testProjectPath,
            Theme = "default"
        };

        // Act
        var result = await _generator.GenerateSiteAsync(project, _outputPath);

        // Assert
        var expectedFolder = Path.Combine(_outputPath, "already-hyphenated");
        Assert.That(Directory.Exists(expectedFolder), Is.True, 
            "Already hyphenated category should remain unchanged (except for case)");
        
        var expectedFile = Path.Combine(expectedFolder, "test-post.html");
        Assert.That(File.Exists(expectedFile), Is.True);
    }

    [Test]
    public async Task GenerateSiteAsync_MultiplePostsInSameCategory_ShouldUseSharedFolder()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_testProjectPath, "content", "first-story.md"), @"---
id: first-story
title: First Story
slug: first-story
category: Jannah Journeys
datePublished: 2025-01-15 10:00:00
---
First story content");

        File.WriteAllText(Path.Combine(_testProjectPath, "content", "second-story.md"), @"---
id: second-story
title: Second Story
slug: second-story
category: Jannah Journeys
datePublished: 2025-02-01 10:00:00
---
Second story content");

        var project = new StaticSiteProject
        {
            RootFolder = _testProjectPath,
            Theme = "default"
        };

        // Act
        var result = await _generator.GenerateSiteAsync(project, _outputPath);

        // Assert
        var categoryFolder = Path.Combine(_outputPath, "jannah-journeys");
        Assert.That(Directory.Exists(categoryFolder), Is.True);
        
        var firstFile = Path.Combine(categoryFolder, "first-story.html");
        var secondFile = Path.Combine(categoryFolder, "second-story.html");
        
        Assert.That(File.Exists(firstFile), Is.True, "First post should be in category folder");
        Assert.That(File.Exists(secondFile), Is.True, "Second post should be in same category folder");
        
        Assert.That(result.GeneratedFiles, Does.Contain("jannah-journeys/first-story.html"));
        Assert.That(result.GeneratedFiles, Does.Contain("jannah-journeys/second-story.html"));
    }
}
