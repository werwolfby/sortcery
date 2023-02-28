using System.Net.Http.Json;
using Sortcery.Api.Contracts.Models;
using Sortcery.Web.Services.Contracts;

namespace Sortcery.Web.Services;

public class FoldersService : IFoldersService
{
    private readonly HttpClient _httpClient;

    public FoldersService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public FolderData Source { get; private set; } = default!;

    public IReadOnlyList<FolderData> DestinationFolders { get; private set; } = Array.Empty<FolderData>();

    public async Task InitializeAsync()
    {
        var response = await _httpClient.GetAsync("api/folders");
        var settings = await response.Content.ReadFromJsonAsync<Folders>();

        Source = settings.Source;
        DestinationFolders = settings.DestinationFolders;
    }
}
