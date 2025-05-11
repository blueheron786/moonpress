using System.Text.Json;

namespace MoonstoneCms.Core;

using System.Text.Json;

public class StaticSiteProject
{
    public string ProjectName { get; set; } = string.Empty;

    public static StaticSiteProject Load(string path)
    {
        var jsonPath = Path.Combine(path, "project.json");
        var json = File.ReadAllText(jsonPath);
        return JsonSerializer.Deserialize<StaticSiteProject>(json)!;
    }

    public void Save(string path)
    {
        var jsonPath = Path.Combine(path, "project.json");
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(jsonPath, json);
    }
}

