using Microsoft.AspNetCore.Mvc;
using Sortcery.Api.Mapper;
using Sortcery.Engine.Contracts;

namespace Sortcery.Api.Controllers;

[ApiController]
[Route("api/folders")]
public class FoldersController : ControllerBase
{
    private readonly IFoldersProvider _foldersProvider;

    public FoldersController(IFoldersProvider foldersProvider)
    {
        _foldersProvider = foldersProvider;
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            Source = _foldersProvider.Source.ToApi(),
            DestinationFolders = _foldersProvider.DestinationFolders.Select(x => x.ToApi())
        });
    }
}
