using MoonPress.Core.Models;

namespace MoonPress.Avalonia.Models;

public interface IAppContext
{
    MoonPressProject? CurrentProject { get; set; }
}

public class AppContext : IAppContext
{
    public MoonPressProject? CurrentProject { get; set; }
}