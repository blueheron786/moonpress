using MoonPress.Core;
using MoonPress.Core.Models;
using MoonPress.Core.Templates;
using NUnit.Framework;

namespace MoonPress.Core.Tests;

[TestFixture]
public class PostsFilterTests
{
    private PostsTemplateProcessor _processor;

    [SetUp]
    public void Setup()
    {
        _processor = new PostsTemplateProcessor();
    }

    [Test]
    public void ProcessPostsBlocks_ShouldReplaceBasicPostsTemplate()
    {
        // Arrange
        var template = @"
<h1>My Blog</h1>
{{posts | category=""blog"" | limit=2}}
  <li><a href=""{{url}}"">{{title}}</a></li>
{{/posts}}
<p>End</p>";

        var contentItems = new List<ContentItem>
        {
            new ContentItem { Title = "Post 1", Slug = "post-1", Category = "blog", DatePublished = DateTime.Parse("2025-01-01") },
            new ContentItem { Title = "Post 2", Slug = "post-2", Category = "blog", DatePublished = DateTime.Parse("2025-01-02") },
            new ContentItem { Title = "Post 3", Slug = "post-3", Category = "blog", DatePublished = DateTime.Parse("2025-01-03") },
            new ContentItem { Title = "News 1", Slug = "news-1", Category = "news", DatePublished = DateTime.Parse("2025-01-04") }
        };

        // Act
        var result = _processor.ProcessPostsBlocks(template, contentItems);

        // Assert
        Assert.That(result, Does.Contain("<li><a href=\"blog/post-3.html\">Post 3</a></li>"));
        Assert.That(result, Does.Contain("<li><a href=\"blog/post-2.html\">Post 2</a></li>"));
        Assert.That(result, Does.Not.Contain("Post 1"));
        Assert.That(result, Does.Not.Contain("News 1"));
        Assert.That(result, Does.Contain("<h1>My Blog</h1>"));
        Assert.That(result, Does.Contain("<p>End</p>"));
    }

    [Test]
    public void ProcessPostsBlocks_ShouldReplaceMultiplePostsBlocks()
    {
        // Arrange
        var template = @"
{{posts | category=""blog"" | limit=1}}
  <p>{{title}}</p>
{{/posts}}
<hr>
{{posts | category=""news"" | limit=1}}
  <div>{{title}}</div>
{{/posts}}";

        var contentItems = new List<ContentItem>
        {
            new ContentItem { Title = "Blog Post", Slug = "blog-post", Category = "blog", DatePublished = DateTime.Parse("2025-01-01") },
            new ContentItem { Title = "News Item", Slug = "news-item", Category = "news", DatePublished = DateTime.Parse("2025-01-02") }
        };

        // Act
        var result = _processor.ProcessPostsBlocks(template, contentItems);

        // Assert
        Assert.That(result, Does.Contain("<p>Blog Post</p>"));
        Assert.That(result, Does.Contain("<div>News Item</div>"));
        Assert.That(result, Does.Contain("<hr>"));
    }

    [Test]
    public void ProcessPostsBlocks_ShouldIgnoreEmptyCategory()
    {
        // Arrange
        var template = @"
{{posts | limit=2}}
  <span>{{title}}</span>
{{/posts}}";

        var contentItems = new List<ContentItem>
        {
            new ContentItem { Title = "Post 1", Slug = "post-1", Category = "blog", DatePublished = DateTime.Parse("2025-01-01") },
            new ContentItem { Title = "Post 2", Slug = "post-2", Category = "news", DatePublished = DateTime.Parse("2025-01-02") }
        };

        // Act
        var result = _processor.ProcessPostsBlocks(template, contentItems);

        // Assert
        Assert.That(result, Does.Contain("<span>Post 2</span>"));
        Assert.That(result, Does.Contain("<span>Post 1</span>"));
    }

    [Test]
    public void ProcessPostsBlocks_ShouldSkipDraftPosts()
    {
        // Arrange
        var template = @"
{{posts | limit=5}}
  <p>{{title}}</p>
{{/posts}}";

        var contentItems = new List<ContentItem>
        {
            new ContentItem { Title = "Published Post", Slug = "published", Category = "blog", IsDraft = false, DatePublished = DateTime.Parse("2025-01-01") },
            new ContentItem { Title = "Draft Post", Slug = "draft", Category = "blog", IsDraft = true, DatePublished = DateTime.Parse("2025-01-02") }
        };

        // Act
        var result = _processor.ProcessPostsBlocks(template, contentItems);

        // Assert
        Assert.That(result, Does.Contain("Published Post"));
        Assert.That(result, Does.Not.Contain("Draft Post"));
    }
}