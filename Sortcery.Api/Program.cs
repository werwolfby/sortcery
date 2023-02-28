using Microsoft.Extensions.Options;
using Sortcery.Api;
using Sortcery.Engine;
using Sortcery.Engine.Contracts;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services
    .AddOptions<GuessItOptions>()
    .Bind(builder.Configuration.GetSection(GuessItOptions.GuessIt))
    .ValidateOnStart();
builder.Services
    .AddOptions<FoldersOptions>()
    .Bind(builder.Configuration.GetSection(FoldersOptions.Folders))
    .Validate(o => o.IsValid, "Folders options are not valid")
    .ValidateOnStart();
builder.Services.AddSingleton<ITraverser, Traverser>();
builder.Services.AddSingleton<IFoldersProvider, FoldersProvider>(sp =>
{
    var options = sp.GetRequiredService<IOptions<FoldersOptions>>();
    var traverser = sp.GetRequiredService<ITraverser>();
    return new FoldersProvider(
        traverser,
        options.Value.Source,
        (FolderType.Movies, options.Value.Movies),
        (FolderType.Shows, options.Value.Series));
});
builder.Services.AddSingleton<IGuessItApi, GuessItApi>();
builder.Services.AddHttpClient<IGuessItApi, GuessItApi>((sp, client) =>
{
    var options = sp.GetService<IOptions<GuessItOptions>>();
    client.BaseAddress = new Uri(options!.Value.Url);
});
builder.Services.AddSingleton<ILinker, Linker>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseWebAssemblyDebugging();
}

app.UseHttpsRedirection();
app.UseBlazorFrameworkFiles();
app.UseWebAssemblyDebugging();
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
