﻿using LegendsViewer.Backend.Contracts;
using LegendsViewer.Backend.Legends.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LegendsViewer.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorldController(IWorld worldDataService) : ControllerBase
{
    private readonly IWorld _worldDataService = worldDataService;

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<WorldDto> Get()
    {
        return Ok(new WorldDto(_worldDataService));
    }
}
