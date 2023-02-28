using Microsoft.AspNetCore.Mvc;
using Sortcery.Api.Contracts.Models;
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
        return Ok(new Folders
        {
            DirectorySeparator = Path.DirectorySeparatorChar.ToString(),
            Source = _foldersProvider.Source.ToApi(),
            DestinationFolders = _foldersProvider.DestinationFolders.Values.Select(x => x.ToApi()).ToArray()
        });
    }
}
