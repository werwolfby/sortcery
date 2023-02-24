using Microsoft.AspNetCore.Mvc;
using Sortcery.Api.Mapper;
using Sortcery.Engine;
using Sortcery.Engine.Contracts;

namespace Sortcery.Api.Controllers;

[ApiController]
[Route("api/links")]
public class LinksController : ControllerBase
{
    private readonly IFoldersProvider _foldersProvider;
    private readonly ILinker _linker;
    private readonly IGuessItApi _guessItApi;

    public LinksController(IFoldersProvider foldersProvider, ILinker linker, IGuessItApi guessItApi)
    {
        _foldersProvider = foldersProvider;
        _linker = linker;
        _guessItApi = guessItApi;
    }

    [HttpGet]
    public IActionResult Get()
    {
        _linker.Update();
        return Ok(_linker.Links.ToHardLinkData());
    }

    [HttpPost]
    public async Task<IActionResult> Guess([FromQuery]string filename)
    {
        var guess = await _guessItApi.GuessAsync(filename);

        var dir = guess.Type == "movie"
            ? _foldersProvider.DestinationFolders[0]
            : _foldersProvider.DestinationFolders[1];
        var fileData = new FileData(dir, filename);

        return Ok(fileData.ToFileData());
    }

    [HttpPost("{dir}/{*relativePath}")]
    public IActionResult Link(string dir, string relativePath, [FromBody]Contracts.Models.FileData body)
    {
        if (_foldersProvider.Source.Name != dir)
        {
            return NotFound($"Unknown source folder: {dir}");
        }

        if (!_foldersProvider.TryGetDestinationFolder(body.Dir, out var destinationFolder))
        {
            return BadRequest($"Unknown destination folder: {body.Dir}");
        }

        var sourceFile = new FileData(_foldersProvider.Source, relativePath);
        var destinationFile = new FileData(destinationFolder!, body.RelativePath);

        _linker.Link(sourceFile, destinationFile);

        return Created($"{dir}/{relativePath}", null);
    }
}
