using Microsoft.AspNetCore.Mvc;
using Sortcery.Api.Mapper;
using Sortcery.Api.Services.Contracts;
using Sortcery.Engine;
using Sortcery.Engine.Contracts;

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

        return Ok(links.ToHardLinkData());
    }

    [HttpPost]
    public async Task<IActionResult> Guess([FromQuery]string filename)
    {
        var guess = await _guessItApi.GuessAsync(filename);

        var dir = guess.Type == "movie"
            ? _foldersService.DestinationFolders[0]
            : _foldersService.DestinationFolders[1];
        var fileData = new FileData(dir, filename);

        return Ok(fileData.ToFileData());
    }

    [HttpPost("{dir}/{*relativePath}")]
    public IActionResult Link(string dir, string relativePath, [FromBody]Contracts.Models.FileData body)
    {
        if (_foldersService.SourceFolder.Name != dir)
        {
            return NotFound($"Unknown source folder: {dir}");
        }

        if (!_foldersService.TryGetDestinationFolder(body.Dir, out var destinationFolder))
        {
            return BadRequest($"Unknow destination folder: {body.Dir}");
        }

        var sourceFile = new FileData(_foldersService.SourceFolder, relativePath);
        var destinationFile = new FileData(destinationFolder, body.RelativePath);

        var linker = new Linker();
        linker.Link(sourceFile, destinationFile);

        return Created($"{dir}/{relativePath}", null);
    }
}
