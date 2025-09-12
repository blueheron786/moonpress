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
<head><title>{{TITLE}}</title></head>
<body>
    <nav>
        <a href=""index.html"">Home</a>
        {{NAVBAR}}
    </nav>
    <main>{{CONTENT}}</main>
</body>
</html>";
        File.WriteAllText(Path.Combine(themeDir, "layout.html"), layoutHtml);
        
        // Create test pages
        var aboutPage = @"---
id: about
title: About Us
slug: about
category: page
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
category: page
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
category: page
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
        Assert.That(aboutHtml, Does.Contain(@"<a href=""about.html"" class=""nav-link"">About Us</a>"), "About link not found in navbar");
        Assert.That(aboutHtml, Does.Contain(@"<a href=""contact.html"" class=""nav-link"">Contact Us</a>"), "Contact link not found in navbar");
        Assert.That(aboutHtml, Does.Contain(@"<a href=""services.html"" class=""nav-link"">Our Services</a>"), "Services link not found in navbar");
        
        // Check that the navbar links are in alphabetical order by title
        var navbarStartIndex = aboutHtml.IndexOf(@"<a href=""about.html"" class=""nav-link"">About Us</a>");
        var contactIndex = aboutHtml.IndexOf(@"<a href=""contact.html"" class=""nav-link"">Contact Us</a>");
        var servicesIndex = aboutHtml.IndexOf(@"<a href=""services.html"" class=""nav-link"">Our Services</a>");
        
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
category: page
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
}
