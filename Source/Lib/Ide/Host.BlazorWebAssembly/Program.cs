using Walk.Common.RazorLib.Installations.Models;
using Walk.Ide.Wasm;
using Walk.Website.RazorLib;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

var hostingInformation = new WalkHostingInformation(
    WalkHostingKind.Wasm,
    WalkPurposeKind.Ide);

builder.Services.AddWalkWebsiteServices(hostingInformation);

await builder.Build().RunAsync();
