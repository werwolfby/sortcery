using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Sortcery.Api.Mapper;
using Sortcery.Api.Services.Contracts;
using Sortcery.Engine;
using Sortcery.Engine.Contracts;
using FileInfo = Sortcery.Engine.Contracts.FileInfo;

namespace Sortcery.Api.Controllers;

[ApiController]
[Route("api/links")]
public class LinksController : ControllerBase
{
    private readonly IFoldersService _foldersService;
    private readonly IGuessItApi _guessItApi;

    public LinksController(IFoldersService foldersService, IGuessItApi guessItApi)
    {
        _foldersService = foldersService;
        _guessItApi = guessItApi;
    }
    
    [HttpGet]
    public IActionResult Get()
    {
        var linker = new Linker();
        var links = 
            linker.FindLinks(_foldersService.SourceFolder, _foldersService.DestinationFolders);

        return Ok(links.ToHardLinkInfo(_foldersService.FoldersMap));
    }
    
    [HttpPost]
    public async Task<IActionResult> Post([FromQuery]string filename)
    {
        var guess = await _guessItApi.GuessAsync(filename);

        var dir = guess.Type == "movie"
            ? _foldersService.DestinationFolders[0]
            : _foldersService.DestinationFolders[1];
        var fileInfo = new FileInfo(dir, filename);
        
        return Ok(fileInfo.ToFileInfo(_foldersService.FoldersMap));
    }
}