using MoonPress.Core.Models;

namespace MoonPress.Core.Tests.Content;

[TestFixture]
public class ContentItemTests
{
    [Test]
    public void Sanitize_ShouldReturnEmptyString_WhenInputIsNull()
    {
        // Arrange
        string? input = null;

        // Act
        var result = ContentItem.Sanitize(input);

        // Assert
        Assert.That(result, Is.EqualTo(string.Empty));
    }

    [Test]
    public void Sanitize_ShouldReturnEmptyString_WhenInputIsEmpty()
    {
        // Arrange
        string input = string.Empty;

        // Act
        var result = ContentItem.Sanitize(input);

        // Assert
        Assert.That(result, Is.EqualTo(string.Empty));
    }

    [Test]
    public void Sanitize_ShouldReturnSanitizedString_WhenInputIsValid()
    {
        // Arrange
        string input = "Hello World! This is a test | string.";

        // Act
        var result = ContentItem.Sanitize(input);

        // Assert
        Assert.That(result, Is.EqualTo("hello-world-this-is-a-test-string"));
    }

    [Test]
    public void Sanitize_ShouldReturnSanitizedString_WhenInputContainsMultipleSpaces()
    {
        // Arrange
        string input = "Hello   World!  This is a test | string.";

        // Act
        var result = ContentItem.Sanitize(input);

        // Assert
        Assert.That(result, Is.EqualTo("hello-world-this-is-a-test-string"));
    }

    [Test]
    public void Sanitize_ShouldReturnSanitizedString_WhenInputContainsInvalidCharacters()
    {
        // Arrange
        string input = "Hello@World! This is a test | string.";

        // Act
        var result = ContentItem.Sanitize(input);

        // Assert
        Assert.That(result, Is.EqualTo("hello-world-this-is-a-test-string"));
    }

    [Test]
    public void Sanitize_ShouldReturnSanitizedString_WhenInputContainsUnderscores()
    {
        // Arrange
        string input = "Hello_World! This is a test | string.";

        // Act
        var result = ContentItem.Sanitize(input);

        // Assert
        Assert.That(result, Is.EqualTo("hello-world-this-is-a-test-string"));
    }

    [Test]
    public void Sanitize_ShouldReturnSanitizedString_WhenInputContainsSpecialCharacters()
    {
        // Arrange
        string input = "Hello@#$%^&*()_+World! This is a test | string.";
        // Act
        var result = ContentItem.Sanitize(input);
        // Assert
        Assert.That(result, Is.EqualTo("hello-world-this-is-a-test-string"));
    }

    [Test]
    public void FileNameOnly_ShouldReturnSanitizedFileName_WhenFilePathIsValid()
    {
        // Arrange
        var contentItem = new ContentItem
        {
            FilePath = "/path/to/file/Hello World! This is a test | string.md"
        };

        // Act
        var result = contentItem.FileNameOnly;

        // Assert
        Assert.That(result, Is.EqualTo("hello-world-this-is-a-test-string"));
    }

    [Test]
    public void Slug_ShouldReturnSanitizedSlug_WhenTitleIsValid()
    {
        // Arrange
        var contentItem = new ContentItem
        {
            Title = "Hello World! This is a test | string."
        };

        // Act
        var result = contentItem.Slug;

        // Assert
        Assert.That(result, Is.EqualTo("hello-world-this-is-a-test-string"));
    }

    [Test]
    public void Slug_ShouldReturnEmptyString_WhenTitleIsNull()
    {
        // Arrange
        var contentItem = new ContentItem
        {
            Title = null!
        };

        // Act
        var result = contentItem.Slug;

        // Assert
        Assert.That(result, Is.EqualTo(string.Empty));
    }

    [Test]
    public void Status_ShouldReturnDraft_WhenIsDraftIsTrue()
    {
        // Arrange
        var contentItem = new ContentItem
        {
            IsDraft = true
        };

        // Act
        var result = contentItem.Status;

        // Assert
        Assert.That(result, Is.EqualTo("Draft"));
    }

    [Test]
    public void Status_ShouldReturnPublished_WhenIsDraftIsFalse()
    {
        // Arrange
        var contentItem = new ContentItem
        {
            IsDraft = false
        };

        // Act
        var result = contentItem.Status;

        // Assert
        Assert.That(result, Is.EqualTo("Published"));
    }

    [Test]
    public void DatePublished_ShouldBeSetToCurrentDateTime_WhenContentItemIsCreated()
    {
        // Arrange
        var contentItem = new ContentItem();

        // Act
        var result = contentItem.DatePublished;

        // Assert
        Assert.That(result, Is.EqualTo(DateTime.UtcNow).Within(TimeSpan.FromSeconds(1)));
    }

    [Test]
    public void DateUpdated_ShouldBeSetToCurrentDateTime_WhenContentItemIsCreated()
    {
        // Arrange
        var contentItem = new ContentItem();

        // Act
        var result = contentItem.DateUpdated;

        // Assert
        Assert.That(result, Is.EqualTo(DateTime.UtcNow).Within(TimeSpan.FromSeconds(1)));
    }
}