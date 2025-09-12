using MoonPress.Core;
using MoonPress.Rendering;

// Create project and generator
var project = StaticSiteProject.Load(@"d:\code\mp\test-output");
var htmlRenderer = new ContentItemHtmlRenderer();
var generator = new StaticSiteGenerator(htmlRenderer);

// Generate site with theme
var outputPath = @"d:\code\mp\test-output\generated";
var result = await generator.GenerateSiteAsync(project, outputPath);

Console.WriteLine($"Generation Result: {result.Success}");
Console.WriteLine($"Message: {result.Message}");
Console.WriteLine($"Pages Generated: {result.PagesGenerated}");
Console.WriteLine($"Files: {string.Join(", ", result.GeneratedFiles)}");

if (result.Errors.Any())
{
    Console.WriteLine("Errors:");
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"  - {error}");
    }
}

// Check if files were created correctly
if (File.Exists(Path.Combine(outputPath, "test-article.html")))
{
    Console.WriteLine("\nGenerated HTML content:");
    var htmlContent = File.ReadAllText(Path.Combine(outputPath, "test-article.html"));
    Console.WriteLine(htmlContent);
}
