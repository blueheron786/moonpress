namespace MoonPress.Core.Models;

public class MoonPressProject
{
    public string ProjectName { get; set; } = "";
    public string ProjectFolder { get; set; } = "";
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow; // for funsies
    public DateTime LastModified { get; set; } = DateTime.UtcNow; // to sort in recent-projects list
}
