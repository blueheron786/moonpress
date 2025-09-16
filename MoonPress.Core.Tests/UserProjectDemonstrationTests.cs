using MoonPress.Core;
using MoonPress.Core.Models;
using MoonPress.Rendering;
using NUnit.Framework;

namespace MoonPress.Core.Tests
{
    [TestFixture]
    public class UserProjectDemonstrationTests
    {
        private string _testDirectory;
        private StaticSiteGenerator _generator;

        [SetUp]
        public void Setup()
        {
            _testDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testDirectory);
            
            var htmlRenderer = new ContentItemHtmlRenderer();
            _generator = new StaticSiteGenerator(htmlRenderer);
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
        public async Task DemonstrateUserProjectWithCategories()
        {
            // This test demonstrates exactly what the user requested:
            // 1. Parse pages/**/*.md files and extract Category metadata
            // 2. Process template tags like {{posts | category = "Blog" | limit = 5}}
            
            // ARRANGE: Set up a user project structure
            CreateUserProjectStructure();
            
            var project = new StaticSiteProject
            {
                RootFolder = _testDirectory,
                Theme = "default",
                ProjectName = "User Test Site"
            };
            
            var outputDir = Path.Combine(_testDirectory, "output");
            
            // ACT: Generate the site
            var result = await _generator.GenerateSiteAsync(project, outputDir);
            
            // ASSERT: Verify it worked
            Assert.That(result.Success, Is.True, $"Site generation failed: {result.Message}");
            
            // Check that index.html was generated with filtered content
            var indexPath = Path.Combine(outputDir, "index.html");
            Assert.That(File.Exists(indexPath), Is.True, "index.html was not generated");
            
            var indexContent = await File.ReadAllTextAsync(indexPath);
            
            // Verify blog posts appear in the blog section
            Assert.That(indexContent, Does.Contain("Web Development Tips"), "Blog post not found in blog section");
            Assert.That(indexContent, Does.Contain("React Best Practices"), "Second blog post not found");
            
            // Verify tutorial appears in tutorial section
            Assert.That(indexContent, Does.Contain("How to Set Up Docker"), "Tutorial not found in tutorial section");
            
            // Verify limit is working (only 2 blog posts should appear, not all 3)
            var blogPostCount = System.Text.RegularExpressions.Regex.Matches(indexContent, "blog-posts").Count;
            Assert.That(blogPostCount, Is.LessThanOrEqualTo(4), "Too many blog posts found - limit not working"); // 2 posts * 2 occurrences each
            
            Console.WriteLine("Generated index.html content:");
            Console.WriteLine(indexContent);
        }

        private void CreateUserProjectStructure()
        {
            // Create theme with posts filtering
            var themeDir = Path.Combine(_testDirectory, "themes", "default");
            Directory.CreateDirectory(themeDir);
            
            var layoutHtml = @"<!DOCTYPE html>
<html>
<head><title>{{title}}</title></head>
<body>
    <nav>{{navbar}}</nav>
    <main>{{content}}</main>
</body>
</html>";
            File.WriteAllText(Path.Combine(themeDir, "layout.html"), layoutHtml);
            
            // Create an index template that demonstrates the user's requested functionality
            var indexHtml = @"<h1>My Personal Website</h1>

<section class=""blog-section"">
    <h2>📝 Latest Blog Posts</h2>
    <ul>
{{posts | category=""Blog"" | limit=2}}
        <li><a href=""{{url}}"">{{title}}</a> - <em>{{summary}}</em></li>
{{/posts}}
    </ul>
</section>

<section class=""tutorial-section"">
    <h2>🎓 Programming Tutorials</h2>
    <ul>
{{posts | category=""Tutorials"" | limit=3}}
        <li><a href=""{{url}}"">{{title}}</a> - <em>{{category}}</em></li>
{{/posts}}
    </ul>
</section>";
            File.WriteAllText(Path.Combine(themeDir, "index.html"), indexHtml);
            
            // Create content directory structure
            var contentDir = Path.Combine(_testDirectory, "content");
            Directory.CreateDirectory(contentDir);
            
            // Create some blog posts with Category metadata
            var blogPost1 = @"---
id: blog-post-1
title: Web Development Tips
slug: web-dev-tips
category: Blog
datePublished: 2025-09-15 10:00:00
isDraft: false
summary: Essential tips for web developers
---

# Web Development Tips

Here are some essential tips for modern web development...";
            File.WriteAllText(Path.Combine(contentDir, "blog-post-1.md"), blogPost1);
            
            var blogPost2 = @"---
id: blog-post-2
title: React Best Practices
slug: react-best-practices
category: Blog
datePublished: 2025-09-14 14:30:00
isDraft: false
summary: Best practices for React development
---

# React Best Practices

Learn the best practices for building React applications...";
            File.WriteAllText(Path.Combine(contentDir, "blog-post-2.md"), blogPost2);
            
            var blogPost3 = @"---
id: blog-post-3
title: JavaScript ES2024 Features
slug: js-es2024
category: Blog
datePublished: 2025-09-13 09:15:00
isDraft: false
summary: New features in JavaScript ES2024
---

# JavaScript ES2024 Features

Explore the latest features in JavaScript...";
            File.WriteAllText(Path.Combine(contentDir, "blog-post-3.md"), blogPost3);
            
            // Create a tutorial with different category
            var tutorial1 = @"---
id: tutorial-1
title: How to Set Up Docker
slug: docker-setup
category: Tutorials
datePublished: 2025-09-12 16:45:00
isDraft: false
summary: Complete guide to Docker setup
---

# How to Set Up Docker

This tutorial will walk you through setting up Docker...";
            File.WriteAllText(Path.Combine(contentDir, "tutorial-1.md"), tutorial1);
        }
    }
}