using Microsoft.AspNetCore.Components.Web;
using BlazorDesktop.Hosting;
using MoonstoneCms.Desktop.Components;

var builder = BlazorDesktopHostBuilder.CreateDefault(args);

builder.RootComponents.Add<Routes>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

if (builder.HostEnvironment.IsDevelopment())
{
    builder.UseDeveloperTools();
}

builder.Window.UseTitle("Moonstone CMS Desktop");

await builder.Build().RunAsync();
