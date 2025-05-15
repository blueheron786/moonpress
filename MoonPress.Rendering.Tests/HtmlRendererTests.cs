// MoonPress.Rendering.Tests/ContentItemHtmlRendererTest.cs
using System;
using MoonPress.Core.Models;
using NUnit.Framework;

namespace MoonPress.Rendering.Tests
{
    [TestFixture]
    public class ContentItemHtmlRendererTest
    {
        public void RenderHtml_NullContentItem_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ContentItemHtmlRenderer().RenderHtml(null));
        }

        [Test]
        public void RenderHtml_HeadContainsTitleAndOpenGraphTags()
        {
            var contentItem = new ContentItem
            {
                Title = "My \"Special\" Title",
                DatePublished = new DateTime(2024, 6, 1, 14, 30, 0),
                Summary = "A summary for OG.",
                Contents = "Some content."
            };

            var result = new ContentItemHtmlRenderer().RenderHtml(contentItem);

            Assert.That(result, Does.Contain("<head>"));
            Assert.That(result, Does.Contain("<title>My \"Special\" Title</title>"));
            Assert.That(result, Does.Contain("<meta property=\"og:title\" content=\"My 'Special' Title\""));
            Assert.That(result, Does.Contain("<meta property=\"og:type\" content=\"article\""));
            Assert.That(result, Does.Contain("<meta property=\"og:url\" content=\"https://example.com/my-special-title\""));
            Assert.That(result, Does.Contain("<meta property=\"og:description\" content=\"A summary for OG.\""));
        }

        [Test]
        public void RenderHtml_HeadOgTitle_ReplacesDoubleQuotesWithSingleQuotes()
        {
            var contentItem = new ContentItem
            {
                Title = "Hello \"World\"",
                DatePublished = DateTime.Now,
                Summary = "Summary",
                Contents = "Content"
            };

            var result = new ContentItemHtmlRenderer().RenderHtml(contentItem);

            Assert.That(result, Does.Contain("content=\"Hello 'World'\""));
        }

        [Test]
        public void RenderHtml_HeadOgUrl_UsesSlugWithDashes()
        {
            var contentItem = new ContentItem
            {
                Title = "Slug With Spaces",
                DatePublished = DateTime.Now,
                Summary = "Summary",
                Contents = "Content"
            };

            var result = new ContentItemHtmlRenderer().RenderHtml(contentItem);

            Assert.That(result, Does.Contain("content=\"https://example.com/slug-with-spaces\""));
        }

        [Test]
        public void RenderHtml_HeadOgDescription_UsesSummaryIfPresent()
        {
            var contentItem = new ContentItem
            {
                Title = "Test",
                DatePublished = DateTime.Now,
                Summary = "This is the summary.",
                Contents = "Content that is long enough to be ignored."
            };

            var result = new ContentItemHtmlRenderer().RenderHtml(contentItem);

            Assert.That(result, Does.Contain("content=\"This is the summary.\""));
        }

        [Test]
        public void RenderHtml_HeadOgDescription_UsesFirst140CharsOfContentsIfSummaryNull()
        {
            var longContent = string.Join(" ", new string[30].Select((_, i) => $"word{i}"));
            var contentItem = new ContentItem
            {
                Title = "Test",
                DatePublished = DateTime.Now,
                Summary = null,
                Contents = longContent
            };

            var result = new ContentItemHtmlRenderer().RenderHtml(contentItem);

            // Should contain the first 140 chars of content, rounded up to the nearest word
            var expectedDescription = longContent.Substring(0, longContent.IndexOf(' ', 140));
            Assert.That(result, Does.Contain($"content=\"{expectedDescription}\""));
        }
    }
}