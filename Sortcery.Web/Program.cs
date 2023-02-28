using Sortcery.Web;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Sortcery.Web.Services;
using Sortcery.Web.Services.Contracts;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddTransient(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddSingleton<IFoldersService, FoldersService>();

var host = builder.Build();

var foldersService = host.Services.GetRequiredService<IFoldersService>();
await foldersService.InitializeAsync();

await host.RunAsync();
