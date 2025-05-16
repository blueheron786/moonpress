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
}