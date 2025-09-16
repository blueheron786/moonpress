using MoonPress.Core;
using MoonPress.Rendering;

namespace MoonPress.CLI;

class Program
{
    static async Task<int> Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: moonpress <project-directory>");
            Console.WriteLine("Generates a static site from the project directory to <project-directory>/output");
            return 1;
        }

        var projectPath = Path.GetFullPath(args[0]);

        if (!Directory.Exists(projectPath))
        {
            Console.WriteLine($"❌ Project directory not found: {projectPath}");
            return 1;
        }

        var outputPath = Path.Combine(projectPath, "output");
        var projectName = Path.GetFileName(projectPath);

        Console.WriteLine($"🌙 Generating {projectName} -> {outputPath}");

        try
        {
            // Clear output directory
            if (Directory.Exists(outputPath))
            {
                foreach (var item in Directory.GetFileSystemEntries(outputPath))
                {
                    if (Directory.Exists(item))
                        Directory.Delete(item, true);
                    else
                        File.Delete(item);
                }
            }
            else
            {
                Directory.CreateDirectory(outputPath);
            }

            // Generate the site
            var project = new StaticSiteProject
            {
                RootFolder = projectPath,
                Theme = "default",
                ProjectName = projectName
            };

            var renderer = new ContentItemHtmlRenderer();
            var generator = new StaticSiteGenerator(renderer);
            var result = await generator.GenerateSiteAsync(project, outputPath);

            if (result.Success)
            {
                Console.WriteLine($"✅ Generated {result.PagesGenerated} pages in {result.Duration.TotalSeconds:F1}s");
                return 0;
            }
            else
            {
                Console.WriteLine($"❌ {result.Message}");
                return 1;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            return 1;
        }
    }
}