using System.Text.Json;
using System.Text.Json.Serialization;

namespace MoonPress.Core;

/// <summary>
/// The in-memory representation of the current project.
/// This is the project that is currently open in the editor.
/// </summary>
public class StaticSiteProject
{
    public string ProjectName { get; set; } = string.Empty;
    public string Theme { get; set; } = "default";
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public string RootFolder { get; set; } = string.Empty;

    public static StaticSiteProject Load(string path)
    {
        var jsonPath = Path.Combine(path, "project.json");
        var json = File.ReadAllText(jsonPath);
        var toReturn = JsonSerializer.Deserialize<StaticSiteProject>(json)!;
        
        toReturn.RootFolder = path;
        return toReturn;
    }

    public void Save(string path)
    {
        var jsonPath = Path.Combine(path, "project.json");
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(jsonPath, json);
    }
}

