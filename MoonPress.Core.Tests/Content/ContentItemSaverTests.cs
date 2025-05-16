using MoonPress.Core.Content;
using MoonPress.Core.Models;
using MoonPress.Core.Renderers;
using NSubstitute;

namespace MoonPress.Core.Tests.Content
{
    [TestFixture]
    public class ContentItemSaverTests
    {
        private string _testRoot;
        private string _contentDir;
        private IMarkdownRenderer _renderer;

        [SetUp]
        public void SetUp()
        {
            _testRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            _contentDir = Path.Combine(_testRoot, "Content");
            Directory.CreateDirectory(_contentDir);
            _renderer = Substitute.For<IMarkdownRenderer>();
            // Reset ContentItemFetcher cache
            typeof(ContentItemFetcher)
                .GetField("_contentItems", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(null, null);
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

        [Test]
        public async Task SaveContentItem_CreatesFileAndUpdatesCache()
        {
            // Arrange
            var item = new ContentItem
            {
                Title = "Test Title",
                Category = "Cat",
                Tags = "tag1,tag2",
                Summary = "summary",
                Contents = "body"
            };
            _renderer.RenderMarkdown(item).Returns("---\nid: testid\n---\nbody");

            // Act
            await ContentItemSaver.SaveContentItem(item, _renderer, _testRoot);

            // Assert
            var expectedFile = Path.Combine(_contentDir, "Test-Title.md");
            Assert.That(File.Exists(expectedFile), Is.True);

            var fileContent = await File.ReadAllTextAsync(expectedFile);
            Assert.That(fileContent, Does.Contain("body"));

            // Check cache updated
            var cache = ContentItemFetcher.GetContentItems(_testRoot);
            Assert.That(cache.Values, Has.One.Matches<ContentItem>(ci => ci.Title == "Test Title"));
        }

        [Test]
        public void SaveContentItem_ThrowsIfRootFolderNullOrEmpty()
        {
            // Arrange
            var item = new ContentItem { Title = "T" };

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(() =>
                ContentItemSaver.SaveContentItem(item, _renderer, null!));
            Assert.ThrowsAsync<ArgumentException>(() =>
                ContentItemSaver.SaveContentItem(item, _renderer, ""));
        }

        [Test]
        public void SaveContentItem_ThrowsIfItemNull()
        {
            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(() =>
                ContentItemSaver.SaveContentItem(null!, _renderer, _testRoot));
        }

        [Test]
        public async Task SaveContentItem_SetsIdAndDatesIfMissing()
        {
            // Arrange
            var before = DateTime.UtcNow;
            var item = new ContentItem { Title = "NoId" };
            _renderer.RenderMarkdown(item).Returns("dummy");

            // Act
            await ContentItemSaver.SaveContentItem(item, _renderer, _testRoot);

            // Assert
            Assert.That(item.Id, Is.Not.Null.And.Not.Empty);
            Assert.That(item.DatePublished, Is.GreaterThanOrEqualTo(before));
            Assert.That(item.DateUpdated, Is.GreaterThanOrEqualTo(before));
        }

        [Test]
        public async Task SaveContentItem_UsesFilePathIfProvided()
        {
            // Arrange
            var item = new ContentItem
            {
                Title = "Custom",
                FilePath = Path.Combine(_contentDir, "custom.md")
            };
            _renderer.RenderMarkdown(item).Returns("custom content");

            // Act
            await ContentItemSaver.SaveContentItem(item, _renderer, _testRoot);

            // Assert
            Assert.That(File.Exists(item.FilePath), Is.True);
            var content = await File.ReadAllTextAsync(item.FilePath);
            Assert.That(content, Is.EqualTo("custom content"));
        }
    }
}