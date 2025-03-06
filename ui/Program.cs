using VideoToSpeechPOC.Components;
using VideoIndexer.Extensions;
using VideoToSpeechPOC.Options;
using VideoToSpeechPOC.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAzureVideoIndexer(builder.Configuration.GetSection("AzureVideoIndexer"));
builder.Services.Configure<AzureSpeechOptions>(builder.Configuration.GetSection("AzureSpeech"));
builder.Services.AddSingleton<FileService>();
builder.Services.AddSingleton<AzureSpeechService>();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.MapControllers();

app.Run();
