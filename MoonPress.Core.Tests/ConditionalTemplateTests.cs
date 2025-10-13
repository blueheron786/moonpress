using MoonPress.Core.Models;
using MoonPress.Core.Templates;
using NUnit.Framework;

namespace MoonPress.Core.Tests;

[TestFixture]
public class ConditionalTemplateTests
{
    private PostsTemplateProcessor _processor;

    [SetUp]
    public void Setup()
    {
        _processor = new PostsTemplateProcessor();
    }

    [Test]
    public void ProcessSingleItemVariables_ShouldRemoveConditionalTags_WhenFieldHasValue()
    {
        // Arrange
        var template = "{{#buy_link}}<a href=\"{{buy_link}}\">Buy Now</a>{{/buy_link}}";
        var contentItem = new ContentItem
        {
            Title = "Test Book",
            CustomFields = new Dictionary<string, string>
            {
                { "buy_link", "https://example.com/buy" }
            }
        };

        // Act
        var result = _processor.ProcessSingleItemVariables(template, contentItem);

        // Assert - Should have the link WITHOUT any curly braces
        Console.WriteLine($"Result: '{result}'");
        Assert.That(result, Is.EqualTo("<a href=\"https://example.com/buy\">Buy Now</a>"));
        Assert.That(result, Does.Not.Contain("{{"));
        Assert.That(result, Does.Not.Contain("}}"));
        Assert.That(result, Does.Not.Contain("{}")); // The bug we're fixing
    }

    [Test]
    public void ProcessSingleItemVariables_ShouldRemoveEntireSection_WhenFieldIsEmpty()
    {
        // Arrange
        var template = "Before {{#buy_link}}<a href=\"{{buy_link}}\">Buy Now</a>{{/buy_link}} After";
        var contentItem = new ContentItem
        {
            Title = "Test Book",
            CustomFields = new Dictionary<string, string>
            {
                { "buy_link", "" } // Empty value
            }
        };

        // Act
        var result = _processor.ProcessSingleItemVariables(template, contentItem);

        // Assert - Should remove entire conditional section
        Console.WriteLine($"Result: '{result}'");
        Assert.That(result, Is.EqualTo("Before  After"));
        Assert.That(result, Does.Not.Contain("<a"));
        Assert.That(result, Does.Not.Contain("{{"));
    }

    [Test]
    public void ProcessSingleItemVariables_ShouldRemoveEntireSection_WhenFieldDoesNotExist()
    {
        // Arrange
        var template = "Before {{#buy_link}}<a href=\"{{buy_link}}\">Buy Now</a>{{/buy_link}} After";
        var contentItem = new ContentItem
        {
            Title = "Test Book",
            CustomFields = new Dictionary<string, string>() // No buy_link field
        };

        // Act
        var result = _processor.ProcessSingleItemVariables(template, contentItem);

        // Assert - Should keep the conditional tags since field doesn't exist
        Console.WriteLine($"Result: '{result}'");
        // Field doesn't exist in CustomFields, so conditionals won't be processed
        Assert.That(result, Does.Contain("{{#buy_link}}"));
    }

    [Test]
    public void ProcessSingleItemVariables_ShouldHandleMultipleConditionals()
    {
        // Arrange
        var template = @"
<div>
    {{#buy_paperback_amazon}}<a href=""{{buy_paperback_amazon}}"">Amazon</a>{{/buy_paperback_amazon}}
    {{#buy_paperback_barnes_noble}}<a href=""{{buy_paperback_barnes_noble}}"">B&N</a>{{/buy_paperback_barnes_noble}}
    {{#buy_ebook_kobo}}<a href=""{{buy_ebook_kobo}}"">Kobo</a>{{/buy_ebook_kobo}}
</div>";

        var contentItem = new ContentItem
        {
            Title = "Test Book",
            CustomFields = new Dictionary<string, string>
            {
                { "buy_paperback_amazon", "https://amazon.com/book" },
                { "buy_paperback_barnes_noble", "https://bn.com/book" },
                { "buy_ebook_kobo", "https://kobo.com/book" }
            }
        };

        // Act
        var result = _processor.ProcessSingleItemVariables(template, contentItem);

        // Assert
        Console.WriteLine($"Result: '{result}'");
        Assert.That(result, Does.Contain("<a href=\"https://amazon.com/book\">Amazon</a>"));
        Assert.That(result, Does.Contain("<a href=\"https://bn.com/book\">B&N</a>"));
        Assert.That(result, Does.Contain("<a href=\"https://kobo.com/book\">Kobo</a>"));
        
        // Critical: No {} artifacts
        Assert.That(result, Does.Not.Contain("{}"));
        Assert.That(result, Does.Not.Contain("{{#"));
        Assert.That(result, Does.Not.Contain("{{/"));
    }

    [Test]
    public void ProcessSingleItemVariables_RealWorldBookTemplate()
    {
        // Arrange - Actual template from book.html
        var template = @"{{#buy_link}}<h2><a href=""{{buy_link}}"">Buy Now</a></h2>{{/buy_link}}
<div class=""buy-links"">
    {{#buy_paperback_amazon}}<a href=""{{buy_paperback_amazon}}"" class=""buy-button"">Amazon</a>{{/buy_paperback_amazon}}
    {{#buy_ebook_kobo}}<a href=""{{buy_ebook_kobo}}"" class=""buy-button"">Kobo</a>{{/buy_ebook_kobo}}
</div>";

        var contentItem = new ContentItem
        {
            Title = "The Green Beast",
            Slug = "the-green-beast",
            CustomFields = new Dictionary<string, string>
            {
                { "buy_link", "https://books2read.com/greenbeast" },
                { "buy_paperback_amazon", "https://www.amazon.com/Green-Beast/dp/B0F7FRD7WY/" },
                { "buy_ebook_kobo", "https://www.kobo.com/ebook/the-green-beast-1" }
            }
        };

        // Act
        var result = _processor.ProcessSingleItemVariables(template, contentItem);

        // Assert
        Console.WriteLine($"Result: '{result}'");
        
        // Should contain the actual links
        Assert.That(result, Does.Contain("https://books2read.com/greenbeast"));
        Assert.That(result, Does.Contain("https://www.amazon.com/Green-Beast/dp/B0F7FRD7WY/"));
        Assert.That(result, Does.Contain("https://www.kobo.com/ebook/the-green-beast-1"));
        
        // Should NOT contain any template syntax
        Assert.That(result, Does.Not.Contain("{{"));
        Assert.That(result, Does.Not.Contain("}}"));
        
        // THE BUG: Should NOT contain {} artifacts
        Assert.That(result, Does.Not.Contain("{}"), 
            "Found {} artifacts - conditionals are being replaced incorrectly!");
    }
}
