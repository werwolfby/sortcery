using Microsoft.AspNetCore.Mvc;
using Sortcery.Api.Mapper;
using Sortcery.Engine.Contracts;

namespace Sortcery.Api.Controllers;

[ApiController]
[Route("api/links")]
public class LinksController : ControllerBase
{
    private readonly IFoldersProvider _foldersProvider;
    private readonly ILinker _linker;

    public LinksController(IFoldersProvider foldersProvider, ILinker linker)
    {
        _foldersProvider = foldersProvider;
        _linker = linker;
    }

    [HttpGet]
    public IActionResult Get()
    {
        _linker.Update();
        return Ok(_linker.Links.ToHardLinkData());
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
        var destinationFile = new FileData(destinationFolder, body.Path, body.Name);

        _linker.Link(sourceFile, destinationFile);

        return Created($"{dir}/{relativePath}", null);
    }
}
