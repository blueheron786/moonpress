using NUnit.Framework;
using MoonPress.Core;
using MoonPress.Core.Models;
using MoonPress.Rendering;
using System.Text;

namespace MoonPress.Core.Tests;

[TestFixture]
public class NavbarGenerationTests
{
    private StaticSiteGenerator _generator;
    private string _testDirectory;

    [SetUp]
    public void Setup()
    {
        // Clear the static cache to ensure test isolation
        MoonPress.Core.Content.ContentItemFetcher.ClearContentItems();
        
        _generator = new StaticSiteGenerator(new ContentItemHtmlRenderer());
        _testDirectory = Path.Combine(Path.GetTempPath(), $"moonpress_navbar_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
        
        // Create test content structure
        var contentDir = Path.Combine(_testDirectory, "content", "pages");
        Directory.CreateDirectory(contentDir);
        
        // Create test theme
        var themeDir = Path.Combine(_testDirectory, "themes", "default");
        Directory.CreateDirectory(themeDir);
        
        // Create layout.html with navbar placeholder
        var layoutHtml = @"<!DOCTYPE html>
<html>
<head><title>{{title}}</title></head>
<body>
    <nav>
        <a href=""index.html"">Home</a>
        {{navbar}}
    </nav>
    <main>{{content}}</main>
</body>
</html>";
        File.WriteAllText(Path.Combine(themeDir, "layout.html"), layoutHtml);
        
        // Create test pages
        var aboutPage = @"---
id: about
title: About Us
slug: about
datePublished: 2025-09-12 10:00:00
isDraft: false
---
# About Us
This is the about page.";
        File.WriteAllText(Path.Combine(contentDir, "about.md"), aboutPage);
        
        var servicesPage = @"---
id: services
title: Our Services
slug: services
datePublished: 2025-09-12 10:00:00
isDraft: false
---
# Our Services
This is the services page.";
        File.WriteAllText(Path.Combine(contentDir, "services.md"), servicesPage);
        
        var contactPage = @"---
id: contact
title: Contact Us
slug: contact
datePublished: 2025-09-12 10:00:00
isDraft: false
---
# Contact Us
This is the contact page.";
        File.WriteAllText(Path.Combine(contentDir, "contact.md"), contactPage);
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
    public async Task GenerateSite_WithPagesInContentPagesDirectory_GeneratesNavbarLinks()
    {
        // Arrange
        var project = new StaticSiteProject
        {
            RootFolder = _testDirectory,
            Theme = "default",
            ProjectName = "Test Site"
        };
        
        var outputDir = Path.Combine(_testDirectory, "output");
        
        // Act
        var result = await _generator.GenerateSiteAsync(project, outputDir);
        
        // Assert
        Assert.That(result.Success, Is.True, $"Site generation failed: {result.Message}");
        Assert.That(result.PagesGenerated, Is.GreaterThan(0), "No pages were generated");
        
        // Check that individual page files were created
        Assert.That(File.Exists(Path.Combine(outputDir, "about.html")), Is.True, "about.html was not generated");
        Assert.That(File.Exists(Path.Combine(outputDir, "services.html")), Is.True, "services.html was not generated");
        Assert.That(File.Exists(Path.Combine(outputDir, "contact.html")), Is.True, "contact.html was not generated");
        
        // Check that the navbar links are present in the generated HTML
        var aboutHtml = await File.ReadAllTextAsync(Path.Combine(outputDir, "about.html"));
        Assert.That(aboutHtml, Does.Contain(@"<a href=""/about.html"" class=""nav-link"">About Us</a>"), "About link not found in navbar");
        Assert.That(aboutHtml, Does.Contain(@"<a href=""/contact.html"" class=""nav-link"">Contact Us</a>"), "Contact link not found in navbar");
        Assert.That(aboutHtml, Does.Contain(@"<a href=""/services.html"" class=""nav-link"">Our Services</a>"), "Services link not found in navbar");
        
        // Check that the navbar links are in alphabetical order by title
        var navbarStartIndex = aboutHtml.IndexOf(@"<a href=""/about.html"" class=""nav-link"">About Us</a>");
        var contactIndex = aboutHtml.IndexOf(@"<a href=""/contact.html"" class=""nav-link"">Contact Us</a>");
        var servicesIndex = aboutHtml.IndexOf(@"<a href=""/services.html"" class=""nav-link"">Our Services</a>");
        
        Assert.That(navbarStartIndex, Is.LessThan(contactIndex), "About link should come before Contact link");
        Assert.That(contactIndex, Is.LessThan(servicesIndex), "Contact link should come before Services link");
    }

    [Test]
    public async Task GenerateSite_WithDraftPages_ExcludesDraftFromNavbar()
    {
        // Arrange
        var contentDir = Path.Combine(_testDirectory, "content", "pages");
        
        // Create a draft page
        var draftPage = @"---
id: draft
title: Draft Page
slug: draft
datePublished: 2025-09-12 10:00:00
isDraft: true
---
# Draft Page
This page is a draft and should not appear in navbar.";
        File.WriteAllText(Path.Combine(contentDir, "draft.md"), draftPage);
        
        var project = new StaticSiteProject
        {
            RootFolder = _testDirectory,
            Theme = "default",
            ProjectName = "Test Site"
        };
        
        var outputDir = Path.Combine(_testDirectory, "output");
        
        // Act
        var result = await _generator.GenerateSiteAsync(project, outputDir);
        
        // Assert
        Assert.That(result.Success, Is.True, $"Site generation failed: {result.Message}");
        
        // Check that draft page was not generated
        Assert.That(File.Exists(Path.Combine(outputDir, "draft.html")), Is.False, "draft.html should not be generated");
        
        // Check that draft page link is not in navbar
        var aboutHtml = await File.ReadAllTextAsync(Path.Combine(outputDir, "about.html"));
        Assert.That(aboutHtml, Does.Not.Contain("Draft Page"), "Draft page should not appear in navbar");
        Assert.That(aboutHtml, Does.Not.Contain("draft.html"), "Draft page link should not appear in navbar");
    }

    [Test]
    public async Task GenerateSite_WithNonPageContent_ExcludesFromNavbar()
    {
        // Arrange
        var contentDir = Path.Combine(_testDirectory, "content");
        
        // Create a blog post (not a page) - put it in main content directory
        var blogPost = @"---
id: my-blog-post
title: My Blog Post
slug: my-blog-post
category: blog
datePublished: 2025-09-12 10:00:00
isDraft: false
---
# My Blog Post
This is a blog post, not a page.";
        File.WriteAllText(Path.Combine(contentDir, "blog-post.md"), blogPost);
        
        var project = new StaticSiteProject
        {
            RootFolder = _testDirectory,
            Theme = "default",
            ProjectName = "Test Site"
        };
        
        var outputDir = Path.Combine(_testDirectory, "output");
        
        // Act
        var result = await _generator.GenerateSiteAsync(project, outputDir);
        
        // Assert
        Assert.That(result.Success, Is.True, $"Site generation failed: {result.Message}");
        
        // Check that blog post link is not in navbar (this is the main test)
        var aboutHtml = await File.ReadAllTextAsync(Path.Combine(outputDir, "about.html"));
        Assert.That(aboutHtml, Does.Not.Contain("My Blog Post"), "Blog post should not appear in navbar");
        Assert.That(aboutHtml, Does.Not.Contain("my-blog-post.html"), "Blog post link should not appear in navbar");
    }

    [Test]
    public async Task GenerateSite_WithPageMarkedDisplayFalse_ExcludesFromNavbar()
    {
        // Arrange
        var contentDir = Path.Combine(_testDirectory, "content", "pages");
        
        // Create a page with Display: false
        var hiddenPage = @"---
id: hidden
title: Hidden Page
slug: hidden
datePublished: 2025-09-12 10:00:00
isDraft: false
Display: false
---
# Hidden Page
This page should not appear in navbar due to Display: false.";
        File.WriteAllText(Path.Combine(contentDir, "hidden.md"), hiddenPage);
        
        var project = new StaticSiteProject
        {
            RootFolder = _testDirectory,
            Theme = "default",
            ProjectName = "Test Site"
        };
        
        var outputDir = Path.Combine(_testDirectory, "output");
        
        // Act
        var result = await _generator.GenerateSiteAsync(project, outputDir);
        
        // Assert
        Assert.That(result.Success, Is.True, $"Site generation failed: {result.Message}");
        
        // Check that hidden page was generated (it's not a draft)
        Assert.That(File.Exists(Path.Combine(outputDir, "hidden.html")), Is.True, "hidden.html should be generated even when Display is false");
        
        // Check that hidden page link is not in navbar
        var aboutHtml = await File.ReadAllTextAsync(Path.Combine(outputDir, "about.html"));
        Assert.That(aboutHtml, Does.Not.Contain("Hidden Page"), "Hidden page should not appear in navbar when Display is false");
        Assert.That(aboutHtml, Does.Not.Contain("hidden.html"), "Hidden page link should not appear in navbar when Display is false");
        
        // Verify other pages still appear
        Assert.That(aboutHtml, Does.Contain(@"<a href=""/about.html"" class=""nav-link"">About Us</a>"), "About link should still be in navbar");
        Assert.That(aboutHtml, Does.Contain(@"<a href=""/contact.html"" class=""nav-link"">Contact Us</a>"), "Contact link should still be in navbar");
    }

    [Test]
    public async Task GenerateSite_WithPageMarkedDisplayTrue_IncludesInNavbar()
    {
        // Arrange
        var contentDir = Path.Combine(_testDirectory, "content", "pages");
        
        // Create a page with explicit Display: true
        var visiblePage = @"---
id: visible
title: Visible Page
slug: visible
datePublished: 2025-09-12 10:00:00
isDraft: false
Display: true
---
# Visible Page
This page should appear in navbar due to explicit Display: true.";
        File.WriteAllText(Path.Combine(contentDir, "visible.md"), visiblePage);
        
        var project = new StaticSiteProject
        {
            RootFolder = _testDirectory,
            Theme = "default",
            ProjectName = "Test Site"
        };
        
        var outputDir = Path.Combine(_testDirectory, "output");
        
        // Act
        var result = await _generator.GenerateSiteAsync(project, outputDir);
        
        // Assert
        Assert.That(result.Success, Is.True, $"Site generation failed: {result.Message}");
        
        // Check that visible page was generated
        Assert.That(File.Exists(Path.Combine(outputDir, "visible.html")), Is.True, "visible.html should be generated");
        
        // Check that visible page link is in navbar
        var aboutHtml = await File.ReadAllTextAsync(Path.Combine(outputDir, "about.html"));
        Assert.That(aboutHtml, Does.Contain("Visible Page"), "Visible page should appear in navbar when Display is true");
        Assert.That(aboutHtml, Does.Contain("visible.html"), "Visible page link should appear in navbar when Display is true");
    }

    [Test]
    public async Task GenerateSite_WithPageUsingLowercaseDisplayFalse_ExcludesFromNavbar()
    {
        // Arrange
        var contentDir = Path.Combine(_testDirectory, "content", "pages");
        
        // Create a page with lowercase display: false
        var hiddenPage = @"---
id: lowercase-hidden
title: Lowercase Hidden Page
slug: lowercase-hidden
datePublished: 2025-09-12 10:00:00
isDraft: false
display: false
---
# Lowercase Hidden Page
This page should not appear in navbar due to lowercase display: false.";
        File.WriteAllText(Path.Combine(contentDir, "lowercase-hidden.md"), hiddenPage);
        
        var project = new StaticSiteProject
        {
            RootFolder = _testDirectory,
            Theme = "default",
            ProjectName = "Test Site"
        };
        
        var outputDir = Path.Combine(_testDirectory, "output");
        
        // Act
        var result = await _generator.GenerateSiteAsync(project, outputDir);
        
        // Assert
        Assert.That(result.Success, Is.True, $"Site generation failed: {result.Message}");
        
        // Check that page was generated but not in navbar
        Assert.That(File.Exists(Path.Combine(outputDir, "lowercase-hidden.html")), Is.True, "lowercase-hidden.html should be generated");
        
        var aboutHtml = await File.ReadAllTextAsync(Path.Combine(outputDir, "about.html"));
        Assert.That(aboutHtml, Does.Not.Contain("Lowercase Hidden Page"), "Page with lowercase display: false should not appear in navbar");
        Assert.That(aboutHtml, Does.Not.Contain("lowercase-hidden.html"), "Page link with lowercase display: false should not appear in navbar");
    }

    [Test]
    public async Task GenerateSite_WithMixedDisplaySettings_FiltersCorrectly()
    {
        // Arrange
        var contentDir = Path.Combine(_testDirectory, "content", "pages");
        
        // Create multiple pages with different display settings
        var explicitTruePage = @"---
id: explicit-true
title: Explicit True Page
slug: explicit-true
datePublished: 2025-09-12 10:00:00
isDraft: false
Display: true
---
# Explicit True Page";
        File.WriteAllText(Path.Combine(contentDir, "explicit-true.md"), explicitTruePage);
        
        var explicitFalsePage = @"---
id: explicit-false
title: Explicit False Page
slug: explicit-false
datePublished: 2025-09-12 10:00:00
isDraft: false
Display: false
---
# Explicit False Page";
        File.WriteAllText(Path.Combine(contentDir, "explicit-false.md"), explicitFalsePage);
        
        var defaultPage = @"---
id: default
title: Default Page
slug: default
datePublished: 2025-09-12 10:00:00
isDraft: false
---
# Default Page";
        File.WriteAllText(Path.Combine(contentDir, "default.md"), defaultPage);
        
        var project = new StaticSiteProject
        {
            RootFolder = _testDirectory,
            Theme = "default",
            ProjectName = "Test Site"
        };
        
        var outputDir = Path.Combine(_testDirectory, "output");
        
        // Act
        var result = await _generator.GenerateSiteAsync(project, outputDir);
        
        // Assert
        Assert.That(result.Success, Is.True, $"Site generation failed: {result.Message}");
        
        var aboutHtml = await File.ReadAllTextAsync(Path.Combine(outputDir, "about.html"));
        
        // Should be in navbar: About Us, Contact Us, Default Page, Explicit True Page, Our Services (alphabetical order)
        Assert.That(aboutHtml, Does.Contain("Explicit True Page"), "Page with explicit Display: true should be in navbar");
        Assert.That(aboutHtml, Does.Contain("Default Page"), "Page without Display property should default to appearing in navbar");
        
        // Should NOT be in navbar: Explicit False Page
        Assert.That(aboutHtml, Does.Not.Contain("Explicit False Page"), "Page with explicit Display: false should not be in navbar");
        
        // Verify proper ordering (alphabetical by title)
        var aboutIndex = aboutHtml.IndexOf("About Us");
        var contactIndex = aboutHtml.IndexOf("Contact Us");
        var defaultIndex = aboutHtml.IndexOf("Default Page");
        var explicitTrueIndex = aboutHtml.IndexOf("Explicit True Page");
        var servicesIndex = aboutHtml.IndexOf("Our Services");
        
        Assert.That(aboutIndex, Is.LessThan(contactIndex), "About should come before Contact");
        Assert.That(contactIndex, Is.LessThan(defaultIndex), "Contact should come before Default");
        Assert.That(defaultIndex, Is.LessThan(explicitTrueIndex), "Default should come before Explicit True");
        Assert.That(explicitTrueIndex, Is.LessThan(servicesIndex), "Explicit True should come before Services");
    }
}
