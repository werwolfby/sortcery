﻿using Microsoft.AspNetCore.Mvc;
using Sortcery.Api.Mapper;
using Sortcery.Engine.Contracts;

namespace Sortcery.Api.Controllers;

[ApiController]
[Route("api/guess")]
public class GuessController : ControllerBase
{
    private readonly ILinker _linker;
    private readonly IFoldersProvider _foldersProvider;

    public GuessController(ILinker linker, IFoldersProvider foldersProvider)
    {
        _linker = linker;
        _foldersProvider = foldersProvider;
    }

    [HttpGet("{dir}/{*filePath}")]
    public async Task<IActionResult> Get(string dir, string filePath)
    {
        if (_foldersProvider.Source.Name != dir)
        {
            return NotFound($"Unknown source folder: {dir}");
        }

        var fileData = await _linker.GuessAsync(new FileData(_foldersProvider.Source, filePath));

        return Ok(fileData.ToFileData());
    }
}
