using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Sortcery.Engine;
using Sortcery.Model;

namespace Sortcery.Api.Controllers;

[ApiController]
[Route("api/links")]
public class LinksController : ControllerBase
{
    private readonly IOptions<FoldersOptions> _foldersOptions;
    
    public LinksController(IOptions<FoldersOptions> foldersOptions)
    {
        _foldersOptions = foldersOptions;
    }
    
    [HttpGet]
    public IActionResult Get()
    {
        var sourceFolder = new FolderInfo(_foldersOptions.Value.Source);
        var moviesFolder = new FolderInfo(_foldersOptions.Value.Movies);
        var seriesFolder = new FolderInfo(_foldersOptions.Value.Series);
        
        var linker = new Linker();
        var links = linker.FindLinks(sourceFolder, new []{moviesFolder, seriesFolder});

        return Ok(links);
    }
}