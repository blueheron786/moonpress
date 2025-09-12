using System.IO;
using System.Threading.Tasks;
using MoonPress.Core;
using MoonPress.Core.Models;
using NSubstitute;
using NUnit.Framework;

namespace MoonPress.Core.Tests
{
    [TestFixture]
    public class ThemeLayoutRewriteTests
    {
        [Test]
        public async Task LoadThemeLayoutAsync_RewritesAssetLinksToFlatPaths()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDir);
            var themeName = "testtheme";
            var themeDir = Path.Combine(tempDir, "themes", themeName);
            Directory.CreateDirectory(themeDir);
            var layoutPath = Path.Combine(themeDir, "layout.html");
            var originalLayout = "<html><head><link rel=\"stylesheet\" href=\"/themes/testtheme/style.css\"><script src=\"themes/testtheme/app.js\"></script></head><body>{{CONTENT}}</body></html>";
            await File.WriteAllTextAsync(layoutPath, originalLayout);
            var project = new StaticSiteProject { RootFolder = tempDir, Theme = themeName };
            var renderer = Substitute.For<MoonPress.Core.Renderers.IHtmlRenderer>();
            var generator = new StaticSiteGenerator(renderer);

            // Act
            var method = generator.GetType().GetMethod("LoadThemeLayoutAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var result = method?.Invoke(generator, new object[] { project });
            Assert.That(result, Is.InstanceOf<Task<string>>(), "Reflection did not return Task<string>");
            var rewritten = (Task<string>)result;
            var layout = await rewritten;

            // Assert
            Assert.That(layout, Does.Contain("href=\"style.css\""));
            Assert.That(layout, Does.Contain("src=\"app.js\""));
            Assert.That(layout, Does.Not.Contain($"/themes/{themeName}/"));
            Assert.That(layout, Does.Not.Contain($"themes/{themeName}/"));

            // Cleanup
            Directory.Delete(tempDir, true);
        }
    }
}
