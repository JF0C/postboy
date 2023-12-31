using Microsoft.Extensions.Hosting.WindowsServices;
using Postboy.Helpers;
using Postboy.Services;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = WindowsServiceHelpers.IsWindowsService() ? AppContext.BaseDirectory : default
});

builder.ParseArgs(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddAntDesign();
builder.Services.AddSingleton<IRequestStorageService, RequestStorageService>();
builder.Services.AddSingleton<IRequestExecutorService, RequestExecutorService>();
builder.Services.AddSingleton<IComponentInteractionService, ComponentInteractionService>();
builder.Host.UseWindowsService();

builder.WebHost.UseUrls($"http://localhost:{builder.Configuration["ApplicationPort"]}");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    //app.UseHsts();
}

//app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

System.Diagnostics.Process.Start("explorer.exe", $"http://localhost:{builder.Configuration["ApplicationPort"]}");

app.Run();
