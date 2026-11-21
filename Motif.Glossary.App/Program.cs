using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Motif.Glossary;
using Motif.Glossary.Adapters;
using Motif.Glossary.App;
using Motif.Suite;

// The composition root: the only place that knows which adapters are used.
var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSingleton(_ => new ForOpeningReposViaGitHub(new HttpClient()));
builder.Services.AddSingleton<ForOpeningFilesViaPickerAndDrop>();
builder.Services.AddSingleton(sp => new MotifGlossaryApplication(
    sp.GetRequiredService<ForOpeningReposViaGitHub>(),
    sp.GetRequiredService<ForOpeningFilesViaPickerAndDrop>()));
builder.Services.AddSingleton<Browser>();
builder.Services.AddSingleton<Toaster>();

var host = builder.Build();
await host.Services.GetRequiredService<ForOpeningFilesViaPickerAndDrop>().Start();
await host.RunAsync();
