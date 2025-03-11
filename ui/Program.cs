using VideoToSpeechPOC.Components;
using VideoIndexer.Extensions;
using VideoToSpeechPOC.Options;
using VideoToSpeechPOC.Data;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAzureVideoIndexer(builder.Configuration.GetSection("AzureVideoIndexer"));
builder.Services.Configure<AzureSpeechOptions>(builder.Configuration.GetSection("AzureSpeech"));
var fileUploadSection = builder.Configuration.GetSection("FileUpload");
var fileUploadOptions = fileUploadSection.Get<FileUploadOptions>()
    ?? new FileUploadOptions { MaxFileSize = 536870912 }; // 512MB
builder.Services.Configure<FileUploadOptions>(options =>
{
    options.MaxFileSize = fileUploadOptions.MaxFileSize;
});
builder.Services.AddSingleton<FileService>();
builder.Services.AddSingleton<AzureSpeechService>();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddControllers();


// Configure the maximum file size
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = fileUploadOptions.MaxFileSize;
});

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
