using MoonPress.Core.Content;
using NUnit.Framework;

namespace MoonPress.Core.Tests.Content
{
    [TestFixture]
    public class CategoryContentTests
    {
        private string _testDirectory;

        [SetUp]
        public void Setup()
        {
            _testDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testDirectory);
            ContentItemFetcher.ClearContentItems();
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }
        }

        [Test]
        public void GetContentItems_ShouldParseExamplePagesWithCategories()
        {
            // Arrange
            var contentDir = Path.Combine(_testDirectory, "content");
            Directory.CreateDirectory(contentDir);

            // Create test blog post
            var blogPost = @"---
id: test-blog-post
title: Test Blog Post
slug: test-blog-post
category: Blog
datePublished: 2025-09-15 10:00:00
isDraft: false
summary: This is a test blog post.
---

# Test Blog Post

This is a test blog post content.";
            File.WriteAllText(Path.Combine(contentDir, "blog-post.md"), blogPost);

            // Create test book review
            var bookReview = @"---
id: test-book-review
title: Test Book Review
slug: test-book-review
category: Books
datePublished: 2025-09-14 09:15:00
isDraft: false
summary: A test book review.
---

# Test Book Review

This is a test book review content.";
            File.WriteAllText(Path.Combine(contentDir, "book-review.md"), bookReview);

            // Act
            var contentItems = ContentItemFetcher.GetContentItems(_testDirectory);

            // Assert
            Assert.That(contentItems.Count, Is.EqualTo(2));
            
            var blogItem = contentItems.Values.FirstOrDefault(x => x.Category == "Blog");
            Assert.That(blogItem, Is.Not.Null);
            Assert.That(blogItem.Title, Is.EqualTo("Test Blog Post"));
            Assert.That(blogItem.Slug, Is.EqualTo("test-blog-post"));

            var bookItem = contentItems.Values.FirstOrDefault(x => x.Category == "Books");
            Assert.That(bookItem, Is.Not.Null);
            Assert.That(bookItem.Title, Is.EqualTo("Test Book Review"));
            Assert.That(bookItem.Slug, Is.EqualTo("test-book-review"));
        }

        [Test]
        public void GetCategories_ShouldReturnDistinctCategories()
        {
            // Arrange
            var contentDir = Path.Combine(_testDirectory, "content");
            Directory.CreateDirectory(contentDir);

            var blogPost1 = @"---
category: Blog
title: Post 1
---
Content 1";
            File.WriteAllText(Path.Combine(contentDir, "post1.md"), blogPost1);

            var blogPost2 = @"---
category: Blog
title: Post 2
---
Content 2";
            File.WriteAllText(Path.Combine(contentDir, "post2.md"), blogPost2);

            var bookReview = @"---
category: Books
title: Book Review
---
Book content";
            File.WriteAllText(Path.Combine(contentDir, "book.md"), bookReview);

            // Act
            ContentItemFetcher.GetContentItems(_testDirectory);
            var categories = ContentItemFetcher.GetCategories().ToList();

            // Assert
            Assert.That(categories.Count, Is.EqualTo(2));
            Assert.That(categories, Does.Contain("Blog"));
            Assert.That(categories, Does.Contain("Books"));
        }
    }
}