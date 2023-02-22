using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Sortcery.Api.Mapper;
using Sortcery.Api.Services.Contracts;
using Sortcery.Engine;

namespace Sortcery.Api.Controllers;

[ApiController]
[Route("api/links")]
public class LinksController : ControllerBase
{
    private readonly IFoldersService _foldersService;

    public LinksController(IFoldersService foldersService)
    {
        _foldersService = foldersService;
    }
    
    [HttpGet]
    public IActionResult Get()
    {
        var linker = new Linker();
        var links = 
            linker.FindLinks(_foldersService.SourceFolder, _foldersService.DestinationFolders);

        return Ok(links.ToHardLinkInfo(_foldersService.FoldersMap));
    }
}