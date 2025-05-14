using System.Text.Json;
using System.Text.Json.Serialization;

namespace MoonPress.Core;

public class StaticSiteProject
{
    public string ProjectName { get; set; } = string.Empty;
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

