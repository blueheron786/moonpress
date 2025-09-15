using Microsoft.AspNetCore.Components.Web;
using BlazorDesktop.Hosting;
using MoonPress.BlazorDesktop.Components;
using MoonPress.BlazorDesktop.Services;
using MoonPress.BlazorDesktop;

var builder = BlazorDesktopHostBuilder.CreateDefault(args);

builder.RootComponents.Add<Routes>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

if (builder.HostEnvironment.IsDevelopment())
{
    builder.UseDeveloperTools();
}

builder.Window.UseTitle("MoonPress");

var app = builder.Build();

// Auto-load the last opened project if available
_ = Task.Run(async () =>
{
    try
    {
        var lastProject = await ProjectStateService.LoadLastOpenedProjectAsync();
        if (lastProject.HasValue)
        {
            ProjectState.Current = lastProject.Value.project;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to auto-load last project: {ex.Message}");
    }
});

await app.RunAsync();
