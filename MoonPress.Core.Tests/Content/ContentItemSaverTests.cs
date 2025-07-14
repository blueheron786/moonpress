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
        public async Task SaveContentItem_SetsDatePublishedIfMinValue()
        {
            // Arrange
            var item = new ContentItem
            {
                Title = "MinDate",
                DatePublished = DateTime.MinValue // Explicitly set to min value
            };
            _renderer.RenderMarkdown(item).Returns("dummy");

            // Capture time before save
            var before = DateTime.UtcNow;

            // Act
            await ContentItemSaver.SaveContentItem(item, _renderer, _testRoot);

            // Assert
            Assert.That(item.DatePublished, Is.GreaterThanOrEqualTo(before));
            Assert.That(item.DatePublished, Is.LessThanOrEqualTo(DateTime.UtcNow));
        }

        [Test]
        public async Task SaveContentItem_SetsFilePathIfEmpty()
        {
            // Arrange
            var item = new ContentItem
            {
                Title = "Test Title",
                FilePath = string.Empty // Empty file path
            };
            _renderer.RenderMarkdown(item).Returns("dummy");

            // Act
            await ContentItemSaver.SaveContentItem(item, _renderer, _testRoot);

            // Assert
            Assert.That(item.FilePath, Is.Not.Empty);
            Assert.That(item.FilePath, Does.Contain("Test-Title.md"));
        }

        [Test]
        public async Task SaveContentItem_DoesNotOverwriteExistingFilePath()
        {
            // Arrange
            var existingPath = "existing/path/file.md";
            var item = new ContentItem
            {
                Title = "Test Title",
                FilePath = existingPath // Already has a file path
            };
            _renderer.RenderMarkdown(item).Returns("dummy");

            // Act
            await ContentItemSaver.SaveContentItem(item, _renderer, _testRoot);

            // Assert
            Assert.That(item.FilePath, Is.EqualTo(existingPath));
        }

        [Test]
        public async Task SaveContentItem_UpdatesDateUpdated()
        {
            // Arrange
            var originalDate = DateTime.UtcNow.AddDays(-1);
            var item = new ContentItem
            {
                Title = "Test Title",
                DateUpdated = originalDate
            };
            _renderer.RenderMarkdown(item).Returns("dummy");

            var before = DateTime.UtcNow;

            // Act
            await ContentItemSaver.SaveContentItem(item, _renderer, _testRoot);

            // Assert
            Assert.That(item.DateUpdated, Is.GreaterThan(originalDate));
            Assert.That(item.DateUpdated, Is.GreaterThanOrEqualTo(before));
        }

        [Test]
        public async Task SaveContentItem_CreatesDirectoryIfNotExists()
        {
            // Arrange
            var newRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var item = new ContentItem { Title = "Test" };
            _renderer.RenderMarkdown(item).Returns("dummy");

            try
            {
                // Act
                await ContentItemSaver.SaveContentItem(item, _renderer, newRoot);

                // Assert
                var contentDir = Path.Combine(newRoot, "Content");
                Assert.That(Directory.Exists(contentDir), Is.True);
            }
            finally
            {
                if (Directory.Exists(newRoot))
                {
                    Directory.Delete(newRoot, true);
                }
            }
        }

        [Test]
        public void SaveContentItem_HandlesExceptionGracefully()
        {
            // Arrange
            var item = new ContentItem { Title = "Test" };
            _renderer.RenderMarkdown(item).Returns("dummy");
            
            // Use an invalid path to trigger an exception
            var invalidPath = "\x00\x00\x00"; // Invalid path characters

            // Act & Assert
            // This should not throw an exception, but handle it internally
            Assert.DoesNotThrowAsync(async () => 
                await ContentItemSaver.SaveContentItem(item, _renderer, invalidPath));
        }

        [Test]
        public async Task SaveContentItem_CallsRendererWithCorrectItem()
        {
            // Arrange
            var item = new ContentItem { Title = "Test" };
            _renderer.RenderMarkdown(item).Returns("rendered content");

            // Act
            await ContentItemSaver.SaveContentItem(item, _renderer, _testRoot);

            // Assert
            _renderer.Received(1).RenderMarkdown(item);
        }
    }
}
