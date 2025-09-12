// MoonPress.Rendering.Tests/ContentItemHtmlRendererTest.cs
using System;
using MoonPress.Core.Models;
using NUnit.Framework;

namespace MoonPress.Rendering.Tests
{
    [TestFixture]
    public class HtmlRendererTest
    {
        [Test]
        public void RenderHtml_NullContentItem_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ContentItemHtmlRenderer().RenderHtml(null!));
        }

        [Test]
        public void RenderHtml_RendersContentWithoutHeadSection()
        {
            var contentItem = new ContentItem
            {
                Title = "My \"Special\" Title",
                DatePublished = new DateTime(2024, 6, 1, 14, 30, 0),
                Summary = "A summary for OG.",
                Contents = "Some content."
            };

            var result = new ContentItemHtmlRenderer().RenderHtml(contentItem);

            Assert.That(result, Does.Contain("<h1>My \"Special\" Title</h1>"));
            Assert.That(result, Does.Contain("Published on: 2024-06-01 14:30:00"));
            Assert.That(result, Does.Contain("<div class=\"content\">"));
            Assert.That(result, Does.Contain("Some content."));
            Assert.That(result, Does.Not.Contain("<head>"));
        }

        [Test]
        public void RenderHtml_IncludesPublicationDate()
        {
            var contentItem = new ContentItem
            {
                Title = "Hello \"World\"",
                DatePublished = new DateTime(2024, 6, 1, 14, 30, 0),
                Summary = "Summary",
                Contents = "Content"
            };

            var result = new ContentItemHtmlRenderer().RenderHtml(contentItem);

            Assert.That(result, Does.Contain("Published on: 2024-06-01 14:30:00"));
        }

        [Test]
        public void RenderHtml_WrapsContentInDiv()
        {
            var contentItem = new ContentItem
            {
                Title = "Test Title",
                DatePublished = DateTime.Now,
                Summary = "Summary",
                Contents = "Test content here"
            };

            var result = new ContentItemHtmlRenderer().RenderHtml(contentItem);

            Assert.That(result, Does.Contain("<div class=\"content\">"));
            Assert.That(result, Does.Contain("Test content here"));
            Assert.That(result, Does.Contain("</div>"));
        }

        [Test]
        public void RenderHtml_IncludesTitleAsH1()
        {
            var contentItem = new ContentItem
            {
                Title = "My Test Title",
                DatePublished = DateTime.Now,
                Summary = "This is the summary.",
                Contents = "Content that is part of the article."
            };

            var result = new ContentItemHtmlRenderer().RenderHtml(contentItem);

            Assert.That(result, Does.Contain("<h1>My Test Title</h1>"));
        }

        [Test]
        public void RenderHtml_RendersBasicContentStructure()
        {
            var longContent = "This is a long piece of content that will be rendered in the content div.";
            var contentItem = new ContentItem
            {
                Title = "Test Article",
                DatePublished = new DateTime(2024, 6, 1, 14, 30, 0),
                Summary = null,
                Contents = longContent
            };

            var result = new ContentItemHtmlRenderer().RenderHtml(contentItem);

            Assert.That(result, Does.Contain("<h1>Test Article</h1>"));
            Assert.That(result, Does.Contain("Published on: 2024-06-01 14:30:00"));
            Assert.That(result, Does.Contain("<div class=\"content\">"));
            Assert.That(result, Does.Contain(longContent));
        }
    }
}