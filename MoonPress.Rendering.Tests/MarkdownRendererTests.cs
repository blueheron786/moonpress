// MoonPress.Rendering/ContentItemMarkdownRendererTest.cs
using MoonPress.Core.Models;
using NUnit.Framework;

namespace MoonPress.Rendering.Tests
{
    [TestFixture]
    public class MarkdownRendererTest
    {
        [Test]
        public void RenderMarkdown_NullContentItem_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ContentItemMarkdownRenderer().RenderMarkdown(null));
        }

        [Test]
        public void RenderMarkdown_ValidContentItem_ReturnsExpectedMarkdown()
        {
            // Arrange
            var contentItem = new ContentItem
            {
                Id = Guid.NewGuid().ToString(),
                Title = "Test Title",
                DatePublished = new DateTime(2024, 6, 1, 14, 30, 0),
                IsDraft = true,
                Summary = "This is a summary.",
                Contents = "This is the **content**."
            };

            // Act
            var result = new ContentItemMarkdownRenderer().RenderMarkdown(contentItem);

            // Assert
            Assert.That(result, Does.Contain($"id: {contentItem.Id}"));
            Assert.That(result, Does.Contain("title: Test Title"));
            Assert.That(result, Does.Contain("datePublished: 2024-06-01 14:30:00"));
            Assert.That(result, Does.Contain("isDraft: true"));
            Assert.That(result, Does.Contain("summary: This is a summary."));
            Assert.That(result, Does.Contain("---"));
            Assert.That(result, Does.Contain("This is the **content**."));
        }

        [Test]
        public void RenderMarkdown_DraftFalse_RendersIsDraftFalse()
        {
            var contentItem = new ContentItem
            {
                Id = "abc123",
                Title = "Draft False",
                DatePublished = new DateTime(2024, 1, 1, 0, 0, 0),
                IsDraft = false,
                Summary = "Not a draft.",
                Contents = "Content body."
            };

            var result = new ContentItemMarkdownRenderer().RenderMarkdown(contentItem);

            Assert.That(result, Does.Contain("isDraft: false"));
        }

        [Test]
        public void RenderMarkdown_EmptyFields_RendersEmptyStrings()
        {
            var contentItem = new ContentItem
            {
                Id = "",
                Title = "",
                DatePublished = new DateTime(2024, 1, 1, 0, 0, 0),
                IsDraft = false,
                Summary = "",
                Contents = ""
            };

            var result = new ContentItemMarkdownRenderer().RenderMarkdown(contentItem);

            Assert.That(result, Does.Contain("id: "));
            Assert.That(result, Does.Contain("title: "));
            Assert.That(result, Does.Contain("summary: "));
            Assert.That(result, Does.Contain("---"));
        }

        [Test]
        public void RenderMarkdown_MultilineContent_RendersAllLines()
        {
            var contentItem = new ContentItem
            {
                Id = "multi",
                Title = "Multiline",
                DatePublished = DateTime.UtcNow,
                IsDraft = false,
                Summary = "Multiline summary",
                Contents = "Line1\nLine2\nLine3"
            };

            var result = new ContentItemMarkdownRenderer().RenderMarkdown(contentItem);

            Assert.That(result, Does.Contain("Line1"));
            Assert.That(result, Does.Contain("Line2"));
            Assert.That(result, Does.Contain("Line3"));
        }

        [Test]
        public void RenderMarkdown_WithTagsAndCategory_RendersTagsAndCategory()
        {
            var contentItem = new ContentItem
            {
                Id = "tags1",
                Title = "Tags Test",
                DatePublished = DateTime.UtcNow,
                DateUpdated = DateTime.UtcNow,
                Category = "Tech",
                Tags = "csharp, dotnet,  unit test ",
                IsDraft = false,
                Summary = "Testing tags and category.",
                Contents = "Body."
            };

            var result = new ContentItemMarkdownRenderer().RenderMarkdown(contentItem);

            Assert.That(result, Does.Contain("category: Tech"));
            Assert.That(result, Does.Contain("tags: csharp, dotnet, unit test"));
        }

        [Test]
        public void RenderMarkdown_WithCustomFields_RendersCustomFields()
        {
            var contentItem = new ContentItem
            {
                Id = "custom1",
                Title = "Custom Fields",
                DatePublished = DateTime.UtcNow,
                DateUpdated = DateTime.UtcNow,
                Category = "",
                Tags = "",
                IsDraft = false,
                Summary = "",
                Contents = "",
                CustomFields = new Dictionary<string, string>
                {
                    { "custom1", "value1" },
                    { "custom2", "value2" }
                }
            };

            var result = new ContentItemMarkdownRenderer().RenderMarkdown(contentItem);

            Assert.That(result, Does.Contain("custom1: value1"));
            Assert.That(result, Does.Contain("custom2: value2"));
        }

        [Test]
        public void RenderMarkdown_EscapesQuotesInFields()
        {
            var contentItem = new ContentItem
            {
                Id = "quotes1",
                Title = "Title with \"quotes\"",
                DatePublished = DateTime.UtcNow,
                DateUpdated = DateTime.UtcNow,
                Category = "Cat\"egory",
                Tags = "tag1, \"tag2\"",
                IsDraft = false,
                Summary = "Summary with \"quotes\"",
                Contents = "Content.",
                CustomFields = new Dictionary<string, string>
                {
                    { "field", "value with \"quotes\"" }
                }
            };

            var result = new ContentItemMarkdownRenderer().RenderMarkdown(contentItem);

            Assert.That(result, Does.Contain("title: Title with \\\"quotes\\\""));
            Assert.That(result, Does.Contain("category: Cat\\\"egory"));
            Assert.That(result, Does.Contain("tags: tag1, \\\"tag2\\\""));
            Assert.That(result, Does.Contain("summary: Summary with \\\"quotes\\\""));
            Assert.That(result, Does.Contain("field: value with \\\"quotes\\\""));
        }

        [Test]
        public void RenderMarkdown_EmptyOrNullTags_RendersTagsAsEmpty()
        {
            var contentItem = new ContentItem
            {
                Id = "tags2",
                Title = "No Tags",
                DatePublished = DateTime.UtcNow,
                DateUpdated = DateTime.UtcNow,
                Category = "",
                Tags = null,
                IsDraft = false,
                Summary = "",
                Contents = ""
            };

            var result = new ContentItemMarkdownRenderer().RenderMarkdown(contentItem);

            Assert.That(result, Does.Contain("tags: "));
        }

        [Test]
        public void RenderMarkdown_IncludesDateUpdated()
        {
            var contentItem = new ContentItem
            {
                Id = "dateupd",
                Title = "Date Updated",
                DatePublished = new DateTime(2024, 5, 1, 10, 0, 0),
                DateUpdated = new DateTime(2024, 5, 2, 12, 0, 0),
                Category = "",
                Tags = "",
                IsDraft = false,
                Summary = "",
                Contents = ""
            };

            var result = new ContentItemMarkdownRenderer().RenderMarkdown(contentItem);

            Assert.That(result, Does.Contain("datePublished: 2024-05-01 10:00:00"));
            Assert.That(result, Does.Contain("dateUpdated: 2024-05-02 12:00:00"));
        }
    }
}