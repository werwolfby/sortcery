using Microsoft.AspNetCore.Mvc;
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

        return Ok(links.ToHardLinkInfo(_foldersService.FoldersToNameMap));
    }

    [HttpPost]
    public async Task<IActionResult> Guess([FromQuery]string filename)
    {
        var guess = await _guessItApi.GuessAsync(filename);

        var dir = guess.Type == "movie"
            ? _foldersService.DestinationFolders[0]
            : _foldersService.DestinationFolders[1];
        var fileInfo = new FileInfo(dir, filename);

        return Ok(fileInfo.ToFileInfo(_foldersService.FoldersToNameMap));
    }

    [HttpPost("{dir}/{*relativePath}")]
    public IActionResult Link(string dir, string relativePath, [FromBody]Contracts.Models.FileInfo body)
    {
        if (!_foldersService.NameToFolderMap.TryGetValue(dir, out var sourceFolder))
        {
            return NotFound($"Unknown folder: {dir}");
        }

        if (!_foldersService.NameToFolderMap.TryGetValue(body.Dir, out var destinationFolder))
        {
            return BadRequest($"Unknown folder: {body.Dir}");
        }

        var sourceFile = new FileInfo(sourceFolder, relativePath);
        var destinationFile = new FileInfo(destinationFolder, body.RelativePath);

        var linker = new Linker();
        linker.Link(sourceFile, destinationFile);

        return Created($"{dir}/{relativePath}", null);
    }
}
