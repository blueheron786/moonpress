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
            Assert.That(result, Does.Contain("---\n\n"));
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
    }
}