using MoonPress.Core.Models;
using MoonPress.Core.Templates;
using NUnit.Framework;

namespace MoonPress.Core.Tests;

[TestFixture]
public class PostsTemplateProcessorCategorySlugTests
{
    private PostsTemplateProcessor _processor;

    [SetUp]
    public void Setup()
    {
        _processor = new PostsTemplateProcessor();
    }

    [Test]
    public void ProcessPostsBlocks_CategoryWithSpaces_ShouldGenerateHyphenatedURL()
    {
        // Arrange
        var template = @"
<ul>
{{posts | category=""Jannah Journeys""}}
    <li><a href=""{{url}}"">{{title}}</a></li>
{{/posts}}
</ul>";

        var contentItems = new List<ContentItem>
        {
            new ContentItem
            {
                Title = "The Little Lantern",
                Slug = "the-little-lantern",
                Category = "Jannah Journeys",
                Summary = "An audio adventure",
                DatePublished = DateTime.Parse("2025-01-15")
            }
        };

        // Act
        var result = _processor.ProcessPostsBlocks(template, contentItems);

        // Assert
        Assert.That(result, Does.Contain("/jannah-journeys/the-little-lantern.html"),
            "URL should use hyphenated category name");
        Assert.That(result, Does.Not.Contain("/jannah journeys/"),
            "URL should not contain spaces");
    }

    [Test]
    public void ProcessPostsBlocks_MultipleCategoriesWithSpaces_ShouldGenerateCorrectURLs()
    {
        // Arrange
        var template = @"
{{posts}}
    <div><a href=""{{url}}"">{{title}}</a> - {{category}}</div>
{{/posts}}";

        var contentItems = new List<ContentItem>
        {
            new ContentItem
            {
                Title = "Audio Story",
                Slug = "audio-story",
                Category = "Jannah Journeys",
                DatePublished = DateTime.Parse("2025-01-15")
            },
            new ContentItem
            {
                Title = "Novel",
                Slug = "novel",
                Category = "Young Adult Books",
                DatePublished = DateTime.Parse("2025-02-01")
            }
        };

        // Act
        var result = _processor.ProcessPostsBlocks(template, contentItems);

        // Assert
        Assert.That(result, Does.Contain("/jannah-journeys/audio-story.html"),
            "First category should be hyphenated");
        Assert.That(result, Does.Contain("/young-adult-books/novel.html"),
            "Second category should be hyphenated");
        Assert.That(result, Does.Contain("Jannah Journeys"),
            "Category display name should preserve original formatting");
        Assert.That(result, Does.Contain("Young Adult Books"),
            "Category display name should preserve original formatting");
    }

    [Test]
    public void ProcessPostsBlocks_MixedCaseCategory_ShouldGenerateLowercaseURL()
    {
        // Arrange
        var template = @"{{posts | category=""Audio Stories""}}
<a href=""{{url}}"">{{title}}</a>
{{/posts}}";

        var contentItems = new List<ContentItem>
        {
            new ContentItem
            {
                Title = "Test",
                Slug = "test",
                Category = "Audio Stories",
                DatePublished = DateTime.Parse("2025-01-15")
            }
        };

        // Act
        var result = _processor.ProcessPostsBlocks(template, contentItems);

        // Assert
        Assert.That(result, Does.Contain("/audio-stories/test.html"),
            "Category in URL should be lowercase and hyphenated");
        Assert.That(result, Does.Not.Contain("/Audio-Stories/"),
            "URL should not preserve case");
    }

    [Test]
    public void ProcessSingleItemVariables_CategoryWithSpaces_ShouldGenerateHyphenatedURL()
    {
        // Arrange
        var itemTemplate = @"<a href=""{{url}}"">{{title}}</a> - {{category}}";
        
        var contentItem = new ContentItem
        {
            Title = "The Green Beast",
            Slug = "the-green-beast",
            Category = "Middle Grade Books",
            Summary = "A thrilling adventure",
            DatePublished = DateTime.Parse("2025-01-20")
        };

        // Act
        var result = _processor.ProcessSingleItemVariables(itemTemplate, contentItem);

        // Assert
        Assert.That(result, Does.Contain("/middle-grade-books/the-green-beast.html"),
            "URL should use hyphenated category");
        Assert.That(result, Does.Contain("Middle Grade Books"),
            "Category display should preserve original");
    }

    [Test]
    public void ProcessSingleItemVariables_EmptyCategory_ShouldUseUncategorized()
    {
        // Arrange
        var itemTemplate = @"<a href=""{{url}}"">{{title}}</a>";
        
        var contentItem = new ContentItem
        {
            Title = "Uncategorized Post",
            Slug = "uncategorized-post",
            Category = "",
            DatePublished = DateTime.Parse("2025-01-20")
        };

        // Act
        var result = _processor.ProcessSingleItemVariables(itemTemplate, contentItem);

        // Assert
        Assert.That(result, Does.Contain("/uncategorized/uncategorized-post.html"),
            "Empty category should use 'uncategorized' folder");
    }

    [Test]
    public void ProcessSingleItemVariables_NullCategory_ShouldUseUncategorized()
    {
        // Arrange
        var itemTemplate = @"<a href=""{{url}}"">{{title}}</a>";
        
        var contentItem = new ContentItem
        {
            Title = "No Category Post",
            Slug = "no-category-post",
            Category = null,
            DatePublished = DateTime.Parse("2025-01-20")
        };

        // Act
        var result = _processor.ProcessSingleItemVariables(itemTemplate, contentItem);

        // Assert
        Assert.That(result, Does.Contain("/uncategorized/no-category-post.html"),
            "Null category should use 'uncategorized' folder");
    }

    [Test]
    public void ProcessPostsBlocks_CategoryFilterWithSpaces_ShouldMatch()
    {
        // Arrange - filter uses exact category name with spaces
        var template = @"
{{posts | category=""Jannah Journeys""}}
    <li>{{title}}</li>
{{/posts}}";

        var contentItems = new List<ContentItem>
        {
            new ContentItem
            {
                Title = "Story 1",
                Slug = "story-1",
                Category = "Jannah Journeys",
                DatePublished = DateTime.Parse("2025-01-15")
            },
            new ContentItem
            {
                Title = "Story 2",
                Slug = "story-2",
                Category = "Books",
                DatePublished = DateTime.Parse("2025-01-20")
            }
        };

        // Act
        var result = _processor.ProcessPostsBlocks(template, contentItems);

        // Assert
        Assert.That(result, Does.Contain("Story 1"), 
            "Should include post with matching category");
        Assert.That(result, Does.Not.Contain("Story 2"),
            "Should exclude post with different category");
    }

    [Test]
    public void ProcessPostsBlocks_RealWorldJannahJourneysExample_ShouldWork()
    {
        // Arrange - Real-world example from MM site
        var template = @"
<div class=""jannah-journeys-list"">
{{posts | category=""Jannah Journeys""}}
    <article>
        <h3><a href=""{{url}}"">{{title}}</a></h3>
        <p>{{summary}}</p>
    </article>
{{/posts}}
</div>";

        var contentItems = new List<ContentItem>
        {
            new ContentItem
            {
                Title = "The Little Lantern",
                Slug = "the-little-lantern",
                Category = "Jannah Journeys",
                Summary = "Follow the adventures of a young lantern in Paradise!",
                DatePublished = DateTime.Parse("2025-01-15")
            },
            new ContentItem
            {
                Title = "The Olive Heist",
                Slug = "the-olive-heist",
                Category = "Jannah Journeys",
                Summary = "An exciting heist story set in the gardens of Paradise!",
                DatePublished = DateTime.Parse("2025-02-01")
            }
        };

        // Act
        var result = _processor.ProcessPostsBlocks(template, contentItems);

        // Assert
        Assert.That(result, Does.Contain("/jannah-journeys/the-little-lantern.html"));
        Assert.That(result, Does.Contain("/jannah-journeys/the-olive-heist.html"));
        Assert.That(result, Does.Contain("The Little Lantern"));
        Assert.That(result, Does.Contain("The Olive Heist"));
        Assert.That(result, Does.Contain("Follow the adventures"));
        Assert.That(result, Does.Contain("An exciting heist story"));
    }
}
