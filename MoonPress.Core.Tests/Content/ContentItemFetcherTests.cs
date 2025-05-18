using MoonPress.Core.Content;
using MoonPress.Core.Models;

namespace MoonPress.Core.Tests.Content;

[TestFixture]
public class ContentItemFetcherTests
{
    private string _testRoot;
    private string _contentDir;

    [SetUp]
    public void SetUp()
    {
        _testRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        _contentDir = Path.Combine(_testRoot, "content");
        Directory.CreateDirectory(_contentDir);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_testRoot))
        {
            Directory.Delete(_testRoot, true);
        }

        // Reset static cache between tests
        typeof(ContentItemFetcher)
            .GetField("_contentItems", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
            ?.SetValue(null, null);
    }

    private string WriteMarkdown(string fileName, string yaml, string body = "Body content")
    {
        var filePath = Path.Combine(_contentDir, fileName);
        File.WriteAllText(filePath, $"---\n{yaml}\n---\n{body}");
        return filePath;
    }

    [Test]
    public void GetContentItems_LoadsItemsFromMarkdown()
    {
        // Arrange
        var yaml = @"
id: test1
title: Test Title
datePublished: 2023-01-01 12:00:00
dateUpdated: 2023-01-02 13:00:00
category: CatA
tags: tag1, tag2
isDraft: false
summary: A summary";
        WriteMarkdown("item1.md", yaml);

        // Act
        var items = ContentItemFetcher.GetContentItems(_testRoot);

        // Assert
        Assert.That(items, Has.Count.EqualTo(1));
        var item = items.Values.First();
        Assert.That(item.Id, Is.EqualTo("test1"));
        Assert.That(item.Title, Is.EqualTo("Test Title"));
        Assert.That(item.Category, Is.EqualTo("CatA"));
        Assert.That(item.Tags, Is.EqualTo("tag1, tag2"));
        Assert.That(item.IsDraft, Is.False);
        Assert.That(item.Summary, Is.EqualTo("A summary"));
        Assert.That(item.Contents, Is.EqualTo("Body content"));
        Assert.That(item.DatePublished, Is.EqualTo(new DateTime(2023, 1, 1, 12, 0, 0)));
        Assert.That(item.DateUpdated, Is.EqualTo(new DateTime(2023, 1, 2, 13, 0, 0)));
    }

    [Test]
    public void GetContentItems_ThrowsIfRootFolderMissing()
    {
        // Act & Assert
        var ex = Assert.Throws<DirectoryNotFoundException>(() =>
            ContentItemFetcher.GetContentItems(Path.Combine(_testRoot, "doesnotexist")));
        Assert.That(ex.Message, Does.Contain("does not exist"));
    }

    [Test]
    public void GetContentItems_ThrowsIfRootFolderNullOrEmpty()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => ContentItemFetcher.GetContentItems(null!));
        Assert.Throws<ArgumentException>(() => ContentItemFetcher.GetContentItems(""));
    }

    [Test]
    public void GetCategories_ReturnsDistinctOrderedCategories()
    {
        // Arrange
        WriteMarkdown("a.md", "id: a\ncategory: CatB\ntags: x\nisDraft: false");
        WriteMarkdown("b.md", "id: b\ncategory: CatA\ntags: y\nisDraft: false");
        ContentItemFetcher.GetContentItems(_testRoot);

        // Act
        var cats = ContentItemFetcher.GetCategories().ToList();

        // Assert
        Assert.That(cats, Is.EqualTo(new[] { "CatA", "CatB" }));
    }

    [Test]
    public void GetTags_ReturnsDistinctOrderedTags()
    {
        // Arrange
        WriteMarkdown("a.md", "id: a\ncategory: CatB\ntags: x, y\nisDraft: false");
        WriteMarkdown("b.md", "id: b\ncategory: CatA\ntags: y, z\nisDraft: false");
        ContentItemFetcher.GetContentItems(_testRoot);

        // Act
        var tags = ContentItemFetcher.GetTags().ToList();

        // Assert
        Assert.That(tags, Is.EqualTo(new[] { "x", "y", "z" }));
    }

    [Test]
    public void GetCategories_ThrowsIfNotLoaded()
    {
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => ContentItemFetcher.GetCategories().ToList());
    }

    [Test]
    public void GetTags_ThrowsIfNotLoaded()
    {
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => ContentItemFetcher.GetTags().ToList());
    }

    [Test]
    public void UpdateCache_UpsertsItem()
    {
        // Arrange
        WriteMarkdown("a.md", "id: a\ncategory: CatB\ntags: x\nisDraft: false");
        ContentItemFetcher.GetContentItems(_testRoot);

        var newItem = new ContentItem
        {
            Id = "a",
            Category = "CatC",
            Tags = "t1",
            Title = "Updated",
            FilePath = "dummy",
            DatePublished = DateTime.Now,
            DateUpdated = DateTime.Now,
            IsDraft = false,
            Summary = "sum",
            Contents = "cont"
        };

        // Act
        ContentItemFetcher.UpdateCache(newItem);

        // Assert
        var items = ContentItemFetcher.GetContentItems(_testRoot);
        Assert.That(items["a"].Category, Is.EqualTo("CatC"));
        Assert.That(items["a"].Title, Is.EqualTo("Updated"));
    }

    [Test]
    public void UpdateCache_ThrowsIfNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ContentItemFetcher.UpdateCache(null!));
    }

    [Test]
    public void GetContentItems_ParsesCustomFields()
    {
        // Arrange
        var yaml = @"
id: test2
title: Custom Fields Test
datePublished: 2023-02-01 10:00:00
dateUpdated: 2023-02-02 11:00:00
category: CatB
tags: tag3, tag4
isDraft: true
summary: Has custom fields
customField1: value1
customField2: value2";
        WriteMarkdown("item2.md", yaml);

        // Act
        var items = ContentItemFetcher.GetContentItems(_testRoot);

        // Assert
        var item = items.Values.First(i => i.Id == "test2");
        Assert.That(item.CustomFields, Is.Not.Null);
        Assert.That(item.CustomFields.ContainsKey("customField1"));
        Assert.That(item.CustomFields["customField1"], Is.EqualTo("value1"));
        Assert.That(item.CustomFields.ContainsKey("customField2"));
        Assert.That(item.CustomFields["customField2"], Is.EqualTo("value2"));
    }

    [Test]
    public void GetContentItems_ParsesTagsWithSpacesAndQuotes()
    {
        // Arrange
        var yaml = @"
id: test3
title: Tag Quotes
datePublished: 2023-03-01 09:00:00
dateUpdated: 2023-03-02 10:00:00
category: CatC
tags: tag1, ""tag 2"", tag3
isDraft: false
summary: Tags with spaces and quotes";
        WriteMarkdown("item3.md", yaml);

        // Act
        var items = ContentItemFetcher.GetContentItems(_testRoot);

        // Assert
        var item = items.Values.First(i => i.Id == "test3");
        Assert.That(item.Tags, Is.EqualTo("tag1, \"tag 2\", tag3"));
    }

    [Test]
    public void GetContentItems_EmptyOrMissingTags_ResultsInEmptyTags()
    {
        // Arrange
        var yaml = @"
id: test4
title: No Tags
datePublished: 2023-04-01 08:00:00
dateUpdated: 2023-04-02 09:00:00
category: CatD
isDraft: false
summary: No tags field";
        WriteMarkdown("item4.md", yaml);

        // Act
        var items = ContentItemFetcher.GetContentItems(_testRoot);

        // Assert
        var item = items.Values.First(i => i.Id == "test4");
        Assert.That(item.Tags, Is.EqualTo(string.Empty));
    }
}
