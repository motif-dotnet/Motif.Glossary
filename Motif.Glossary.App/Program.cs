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

// Each adapter is built over its Seam's real Fitting.
builder.Services.AddSingleton<ForOpeningRepos>(_ =>
    new ForOpeningReposViaGitHub(new HttpClientHandler()));
builder.Services.AddSingleton<JsInteropLocalFiles>();
builder.Services.AddSingleton<LocalFiles>(sp =>
    sp.GetRequiredService<JsInteropLocalFiles>());
builder.Services.AddSingleton<
    ForOpeningFiles, ForOpeningFilesViaPickerAndDrop>();
builder.Services.AddSingleton<MotifGlossaryApplication>();
builder.Services.AddSingleton<Browser>();
builder.Services.AddSingleton<Toaster>();

var host = builder.Build();
await host.Services.GetRequiredService<JsInteropLocalFiles>().Listen();
await host.RunAsync();
