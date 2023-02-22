using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using Sortcery.Engine.Contracts;

namespace Sortcery.Engine;

public class GuessItApi : IGuessItApi
{
    private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
    {
        PropertyNamingPolicy = SnakeCaseJsonNamingPolicy.Instance
    };
    
    private readonly HttpClient _httpClient;

    public GuessItApi(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<Guess> GuessAsync(string filename)
    {
        var query = QueryHelpers.AddQueryString("", "filename", filename);
        var response = await _httpClient.GetAsync(query, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();
        var jsonStream = await response.Content.ReadAsStreamAsync();
        return JsonSerializer.Deserialize<Guess>(jsonStream, Options)!;
    }
}