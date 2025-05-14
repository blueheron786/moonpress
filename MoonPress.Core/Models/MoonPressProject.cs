namespace MoonPress.Core.Models;

public class MoonPressProject
{
    public string Name { get; set; } = "";
    public string RootFolder { get; set; } = "";
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow; // for "this project is 1 year old" etc.
    public DateTime LastModifiedOn { get; set; } = DateTime.UtcNow; // to sort in recent-projects list
}