using System.IO;
using System.Windows.Forms;
using Microsoft.AspNetCore.Components;
using MoonPress.Core;

namespace MoonPress.BlazorDesktop.Components.Pages.Projects;

public partial class NewProject : ComponentBase
{
    private const string ThemesFolderName = "themes";
    
    [Inject]
    private NavigationManager Nav { get; set; } = default!;
    
    private string _projectName = "";
    private string _selectedFolder = "";
    private bool _isCreationSuccessful = false;

    private bool IsCreateDisabled => string.IsNullOrWhiteSpace(_projectName) || string.IsNullOrWhiteSpace(_selectedFolder);

    private void SelectFolder()
    {
        using var dialog = new FolderBrowserDialog();
        var result = dialog.ShowDialog();

        if (result == DialogResult.OK)
        {
            _selectedFolder = dialog.SelectedPath;
        }
    }

    private void CreateProject()
    {
        try
        {
            var project = new StaticSiteProject { ProjectName = _projectName };
            
            // Create project directory structure
            var contentDir = Path.Combine(_selectedFolder, "content");
            var themeDir = Path.Combine(_selectedFolder, ThemesFolderName, "default");
            
            Directory.CreateDirectory(contentDir);
            Directory.CreateDirectory(themeDir);
            
            // Copy default theme files
            CopyDefaultThemeFiles(themeDir);
            
            // Save project file
            project.Save(_selectedFolder);
            
            _isCreationSuccessful = true;

            ProjectState.Current = project;
            ProjectState.Current.RootFolder = _selectedFolder;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to create project: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void CopyDefaultThemeFiles(string themeTargetDir)
    {
        // Get the path to the embedded theme templates
        var assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
        var assemblyDir = Path.GetDirectoryName(assemblyLocation)!;
        var templateDir = Path.Combine(assemblyDir, "Templates", "Themes", "default");
        
        // Fallback: look in source directory if running in development
        if (!Directory.Exists(templateDir))
        {
            var sourceDir = Path.GetDirectoryName(assemblyDir);
            while (sourceDir != null && !Path.GetFileName(sourceDir).Equals("MoonPress.BlazorDesktop", StringComparison.OrdinalIgnoreCase))
            {
                sourceDir = Path.GetDirectoryName(sourceDir);
            }
            if (sourceDir != null)
            {
                templateDir = Path.Combine(sourceDir, "Templates", "Themes", "default");
            }
        }
        
        if (Directory.Exists(templateDir))
        {
            // Copy all files from template directory
            foreach (var file in Directory.GetFiles(templateDir))
            {
                var fileName = Path.GetFileName(file);
                var targetFile = Path.Combine(themeTargetDir, fileName);
                File.Copy(file, targetFile, true);
            }
        }
        else
        {
            // Create minimal default files if template directory not found
            CreateMinimalThemeFiles(themeTargetDir);
        }
    }

    private void CreateMinimalThemeFiles(string themeDir)
    {
        // Create a basic layout.html if templates aren't available
        var layoutContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""utf-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
    <title>{{title}}</title>
    {{head}}
</head>
<body>
    <header>
        <h1>My Website</h1>
    </header>
    
    <main>
        {{content}}
    </main>
    
    <footer>
        <p>&copy; 2025 My Website. All rights reserved.</p>
    </footer>
</body>
</html>";

        var cssContent = @"body {
    font-family: Arial, sans-serif;
    margin: 0;
    padding: 20px;
    background-color: #f4f4f4;
}

main {
    max-width: 800px;
    margin: 0 auto;
    background-color: white;
    padding: 20px;
    border-radius: 8px;
    box-shadow: 0 2px 5px rgba(0,0,0,0.1);
}";

        File.WriteAllText(Path.Combine(themeDir, "layout.html"), layoutContent);
        File.WriteAllText(Path.Combine(themeDir, "style.css"), cssContent);
    }
}