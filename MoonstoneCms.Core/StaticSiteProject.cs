using System.Text.Json;

namespace MoonstoneCms.Core;

public class StaticSiteProject
{
    public string ProjectName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;

    public static StaticSiteProject Load(string path)
    {
        var jsonPath = Path.Combine(path, "project.json");
        var json = File.ReadAllText(jsonPath);
        var toReturn = JsonSerializer.Deserialize<StaticSiteProject>(json)!;
        toReturn.Location = path;
        return toReturn;
    }

    public void Save(string path)
    {
        var jsonPath = Path.Combine(path, "project.json");
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(jsonPath, json);
    }
}

