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
    public void ProcessSingleItemVariables_ShouldKeepConditional_WhenFieldDoesNotExist()
    {
        // Arrange - Using {{#field}} syntax (not {{if field_exists}})
        var template = "Before {{#buy_link}}<a href=\"{{buy_link}}\">Buy Now</a>{{/buy_link}} After";
        var contentItem = new ContentItem
        {
            Title = "Test Book",
            CustomFields = new Dictionary<string, string>() // No buy_link field
        };

        // Act
        var result = _processor.ProcessSingleItemVariables(template, contentItem);

        // Assert - Should keep conditional tags since field doesn't exist in CustomFields
        // This is the expected behavior - it alerts the user that something is missing
        Console.WriteLine($"Result: '{result}'");
        Assert.That(result, Does.Contain("{{#buy_link}}"));
        Assert.That(result, Does.Contain("{{/buy_link}}"));
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

    [Test]
    public void ProcessSingleItemVariables_FieldExistsConditional_ShouldIncludeContent_WhenFieldExists()
    {
        // Arrange - Using {{if field_exists}} syntax
        var template = @"
<div class=""buy-links"">
    {{if field_exists buy_paperback_amazon}}<a href=""{{buy_paperback_amazon}}"">Amazon</a>{{/if}}
    {{if field_exists buy_paperback_indigo}}<a href=""{{buy_paperback_indigo}}"">Indigo</a>{{/if}}
</div>";

        var contentItem = new ContentItem
        {
            Title = "Test Book",
            Slug = "test-book",
            CustomFields = new Dictionary<string, string>
            {
                { "buy_paperback_amazon", "https://www.amazon.com/test" }
                // buy_paperback_indigo does NOT exist
            }
        };

        // Act
        var result = _processor.ProcessSingleItemVariables(template, contentItem);

        // Assert
        Console.WriteLine($"Result: '{result}'");
        
        // Should contain the link that exists
        Assert.That(result, Does.Contain("https://www.amazon.com/test"));
        Assert.That(result, Does.Contain("<a href=\"https://www.amazon.com/test\">Amazon</a>"));
        
        // Should NOT contain the Indigo link or its conditional
        Assert.That(result, Does.Not.Contain("Indigo"));
        Assert.That(result, Does.Not.Contain("{{if field_exists"));
        Assert.That(result, Does.Not.Contain("{{/if}}"));
    }

    [Test]
    public void ProcessSingleItemVariables_FieldExistsConditional_RealWorldBookTemplate()
    {
        // Arrange - Real world scenario with some retailers present, others not
        var template = @"
<h3>Paperback</h3>
{{if field_exists buy_paperback_abebooks}}<a href=""{{buy_paperback_abebooks}}"">AbeBooks</a>{{/if}}
{{if field_exists buy_paperback_amazon}}<a href=""{{buy_paperback_amazon}}"">Amazon</a>{{/if}}
{{if field_exists buy_paperback_indigo}}<a href=""{{buy_paperback_indigo}}"">Indigo</a>{{/if}}
<h3>eBook</h3>
{{if field_exists buy_ebook_amazon}}<a href=""{{buy_ebook_amazon}}"">Amazon</a>{{/if}}
{{if field_exists buy_ebook_kobo}}<a href=""{{buy_ebook_kobo}}"">Kobo</a>{{/if}}";

        var contentItem = new ContentItem
        {
            Title = "Test Book",
            Slug = "test-book",
            CustomFields = new Dictionary<string, string>
            {
                { "buy_paperback_abebooks", "https://www.abebooks.com/test" },
                { "buy_paperback_amazon", "https://www.amazon.com/test-paperback" },
                { "buy_ebook_amazon", "https://www.amazon.com/test-ebook" }
                // Missing: buy_paperback_indigo, buy_ebook_kobo
            }
        };

        // Act
        var result = _processor.ProcessSingleItemVariables(template, contentItem);

        // Assert
        Console.WriteLine($"Result: '{result}'");
        
        // Should contain existing links
        Assert.That(result, Does.Contain("https://www.abebooks.com/test"));
        Assert.That(result, Does.Contain("https://www.amazon.com/test-paperback"));
        Assert.That(result, Does.Contain("https://www.amazon.com/test-ebook"));
        
        // Should contain the section headers
        Assert.That(result, Does.Contain("<h3>Paperback</h3>"));
        Assert.That(result, Does.Contain("<h3>eBook</h3>"));
        
        // Should NOT contain links for missing retailers
        Assert.That(result, Does.Not.Contain("Indigo"));
        Assert.That(result, Does.Not.Contain("Kobo"));
        
        // Should NOT contain any template syntax
        Assert.That(result, Does.Not.Contain("{{if"));
        Assert.That(result, Does.Not.Contain("{{/if"));
    }

    [Test]
    public void ProcessSingleItemVariables_FieldExistsConditional_WorksWithEmptyValue()
    {
        // Arrange - Field exists but has empty value
        var template = @"{{if field_exists optional_field}}<span>{{optional_field}}</span>{{/if}}";

        var contentItem = new ContentItem
        {
            Title = "Test",
            Slug = "test",
            CustomFields = new Dictionary<string, string>
            {
                { "optional_field", "" } // Exists but empty
            }
        };

        // Act
        var result = _processor.ProcessSingleItemVariables(template, contentItem);

        // Assert
        Console.WriteLine($"Result: '{result}'");
        
        // Should include content because field EXISTS (even though value is empty)
        Assert.That(result, Does.Contain("<span>"));
        Assert.That(result, Does.Contain("</span>"));
        Assert.That(result, Does.Not.Contain("{{if"));
    }
    
    [Test]
    public void ProcessSingleItemVariables_FieldExistsConditional_MultipleFields()
    {
        // Arrange - Multiple {{if field_exists}} conditionals
        var template = @"
{{if field_exists field1}}<p>Field 1: {{field1}}</p>{{/if}}
{{if field_exists field2}}<p>Field 2: {{field2}}</p>{{/if}}
{{if field_exists field3}}<p>Field 3: {{field3}}</p>{{/if}}";

        var contentItem = new ContentItem
        {
            Title = "Test",
            Slug = "test",
            CustomFields = new Dictionary<string, string>
            {
                { "field1", "value1" },
                { "field3", "value3" }
                // field2 doesn't exist
            }
        };

        // Act
        var result = _processor.ProcessSingleItemVariables(template, contentItem);

        // Assert
        Console.WriteLine($"Result: '{result}'");
        
        // Should contain field1 and field3
        Assert.That(result, Does.Contain("Field 1: value1"));
        Assert.That(result, Does.Contain("Field 3: value3"));
        
        // Should NOT contain field2
        Assert.That(result, Does.Not.Contain("Field 2"));
        
        // Should NOT contain any template syntax
        Assert.That(result, Does.Not.Contain("{{if"));
        Assert.That(result, Does.Not.Contain("{{/if"));
    }
}
