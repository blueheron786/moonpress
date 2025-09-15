using System.Text.Json;
using System.IO;
using MoonPress.Core;

namespace MoonPress.BlazorDesktop.Services;

/// <summary>
/// Service to handle persistence of the last opened project state
/// </summary>
public class ProjectStateService
{
    private static readonly string ConfigFileName = ".moonpress.json";
    
    /// <summary>
    /// Gets the path to the .moonpress.json file in the application's bin directory
    /// </summary>
    private static string GetConfigFilePath()
    {
        // Get the directory where the executable is located
        var assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
        var assemblyDir = Path.GetDirectoryName(assemblyLocation)!;
        return Path.Combine(assemblyDir, ConfigFileName);
    }

    /// <summary>
    /// Saves the currently opened project information for auto-loading on next startup
    /// </summary>
    /// <param name="project">The project to save</param>
    /// <param name="rootFolder">The root folder path of the project</param>
    public static async Task SaveLastOpenedProjectAsync(StaticSiteProject project, string rootFolder)
    {
        try
        {
            var lastProjectInfo = new LastProjectInfo
            {
                ProjectName = project.ProjectName,
                RootFolder = rootFolder,
                LastOpened = DateTime.UtcNow
            };

            var configPath = GetConfigFilePath();
            var json = JsonSerializer.Serialize(lastProjectInfo, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            
            await File.WriteAllTextAsync(configPath, json);
        }
        catch (Exception ex)
        {
            // Log the error but don't throw - we don't want to break the app if persistence fails
            Console.WriteLine($"Failed to save last opened project: {ex.Message}");
        }
    }

    /// <summary>
    /// Loads the last opened project if it exists and is still valid
    /// </summary>
    /// <returns>The loaded project and root folder, or null if no valid project found</returns>
    public static async Task<(StaticSiteProject project, string rootFolder)?> LoadLastOpenedProjectAsync()
    {
        try
        {
            var configPath = GetConfigFilePath();
            
            if (!File.Exists(configPath))
            {
                return null;
            }

            var json = await File.ReadAllTextAsync(configPath);
            var lastProjectInfo = JsonSerializer.Deserialize<LastProjectInfo>(json);
            
            if (lastProjectInfo == null || string.IsNullOrWhiteSpace(lastProjectInfo.RootFolder))
            {
                return null;
            }

            // Check if the project folder and project.json still exist
            var projectFile = Path.Combine(lastProjectInfo.RootFolder, "project.json");
            if (!File.Exists(projectFile))
            {
                // Project no longer exists, remove the config file
                File.Delete(configPath);
                return null;
            }

            // Load the actual project
            var projectJson = await File.ReadAllTextAsync(projectFile);
            var project = JsonSerializer.Deserialize<StaticSiteProject>(projectJson);
            
            if (project != null)
            {
                project.RootFolder = lastProjectInfo.RootFolder;
                return (project, lastProjectInfo.RootFolder);
            }
        }
        catch (Exception ex)
        {
            // Log the error but don't throw - we don't want to break the app if loading fails
            Console.WriteLine($"Failed to load last opened project: {ex.Message}");
        }
        
        return null;
    }

    /// <summary>
    /// Clears the last opened project information
    /// </summary>
    public static void ClearLastOpenedProject()
    {
        try
        {
            var configPath = GetConfigFilePath();
            if (File.Exists(configPath))
            {
                File.Delete(configPath);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to clear last opened project: {ex.Message}");
        }
    }
}

/// <summary>
/// Information about the last opened project
/// </summary>
public class LastProjectInfo
{
    public string ProjectName { get; set; } = string.Empty;
    public string RootFolder { get; set; } = string.Empty;
    public DateTime LastOpened { get; set; }
}