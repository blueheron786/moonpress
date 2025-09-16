using NUnit.Framework;
using MoonPress.Core.Content;

namespace MoonPress.Core.Tests.Content;

[TestFixture]
public class ContentItemFetcherCategoryTests
{
    [SetUp]
    public void SetUp()
    {
        ContentItemFetcher.ClearContentItems();
    }

    [TearDown]
    public void TearDown()
    {
        ContentItemFetcher.ClearContentItems();
    }

    [Test]
    public void GetItemsByCategory_Should_Parse_Uppercase_Category_Field()
    {
        // Arrange
        var tempDir = Path.GetTempPath();
        var testDir = Path.Combine(tempDir, $"moonpress_category_test_{Guid.NewGuid()}");
        var contentDir = Path.Combine(testDir, "content", "posts");
        
        try
        {
            Directory.CreateDirectory(contentDir);
            
            var markdownContent = """
---
id: test-post
Title: Test Post
Category: Technology
datePublished: 2025-09-15 10:00:00
isDraft: false
---

# Test Content
""";
            
            var filePath = Path.Combine(contentDir, "test-post.md");
            File.WriteAllText(filePath, markdownContent);
            
            // Act
            var contentItems = ContentItemFetcher.GetContentItems(testDir);
            var itemsByCategory = ContentItemFetcher.GetItemsByCategory();
            
            // Assert
            Assert.That(contentItems, Has.Count.EqualTo(1));
            Assert.That(contentItems.ContainsKey("test-post"), Is.True);
            
            var contentItem = contentItems["test-post"];
            Assert.That(contentItem.Category, Is.EqualTo("Technology"));
            
            Assert.That(itemsByCategory, Has.Count.EqualTo(1));
            Assert.That(itemsByCategory.ContainsKey("Technology"), Is.True);
            Assert.That(itemsByCategory["Technology"], Has.Count.EqualTo(1));
            Assert.That(itemsByCategory["Technology"][0].Id, Is.EqualTo("test-post"));
        }
        finally
        {
            if (Directory.Exists(testDir))
            {
                Directory.Delete(testDir, true);
            }
        }
    }

    [Test]
    public void GetItemsByCategory_Should_Parse_Lowercase_Category_Field()
    {
        // Arrange
        var tempDir = Path.GetTempPath();
        var testDir = Path.Combine(tempDir, $"moonpress_category_test_{Guid.NewGuid()}");
        var contentDir = Path.Combine(testDir, "content", "posts");
        
        try
        {
            Directory.CreateDirectory(contentDir);
            
            var markdownContent = """
---
id: test-post
title: Test Post
category: technology
datePublished: 2025-09-15 10:00:00
isDraft: false
---

# Test Content
""";
            
            var filePath = Path.Combine(contentDir, "test-post.md");
            File.WriteAllText(filePath, markdownContent);
            
            // Act
            var contentItems = ContentItemFetcher.GetContentItems(testDir);
            var itemsByCategory = ContentItemFetcher.GetItemsByCategory();
            
            // Assert
            Assert.That(contentItems, Has.Count.EqualTo(1));
            Assert.That(contentItems.ContainsKey("test-post"), Is.True);
            
            var contentItem = contentItems["test-post"];
            Assert.That(contentItem.Category, Is.EqualTo("technology"));
            
            Assert.That(itemsByCategory, Has.Count.EqualTo(1));
            Assert.That(itemsByCategory.ContainsKey("technology"), Is.True);
            Assert.That(itemsByCategory["technology"], Has.Count.EqualTo(1));
            Assert.That(itemsByCategory["technology"][0].Id, Is.EqualTo("test-post"));
        }
        finally
        {
            if (Directory.Exists(testDir))
            {
                Directory.Delete(testDir, true);
            }
        }
    }

    [Test]
    public void GetItemsByCategory_Should_Prefer_Lowercase_Over_Uppercase_Category()
    {
        // Arrange
        var tempDir = Path.GetTempPath();
        var testDir = Path.Combine(tempDir, $"moonpress_category_test_{Guid.NewGuid()}");
        var contentDir = Path.Combine(testDir, "content", "posts");
        
        try
        {
            Directory.CreateDirectory(contentDir);
            
            // Test that lowercase takes precedence when both are present
            var markdownContent = """
---
id: test-post
title: Test Post
category: technology
Category: Technology
datePublished: 2025-09-15 10:00:00
isDraft: false
---

# Test Content
""";
            
            var filePath = Path.Combine(contentDir, "test-post.md");
            File.WriteAllText(filePath, markdownContent);
            
            // Act
            var contentItems = ContentItemFetcher.GetContentItems(testDir);
            
            // Assert
            var contentItem = contentItems["test-post"];
            Assert.That(contentItem.Category, Is.EqualTo("technology"), 
                "Lowercase 'category' should take precedence over uppercase 'Category'");
        }
        finally
        {
            if (Directory.Exists(testDir))
            {
                Directory.Delete(testDir, true);
            }
        }
    }
}