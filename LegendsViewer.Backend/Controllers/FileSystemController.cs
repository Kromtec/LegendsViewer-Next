﻿using LegendsViewer.Backend.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace LegendsViewer.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FileSystemController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<FilesAndSubdirectoriesDto>(StatusCodes.Status200OK)]
    public ActionResult<FilesAndSubdirectoriesDto> Get()
    {
        return Ok(GetRootInformation());
    }

    [HttpGet("{path}")]
    [ProducesResponseType<FilesAndSubdirectoriesDto>(StatusCodes.Status200OK)]
    public ActionResult<FilesAndSubdirectoriesDto> Get([FromRoute] string path)
    {
        if (!Path.Exists(path) && !Directory.Exists(path))
        {
            return Ok(GetRootInformation());
        }
        string directoryName = Directory.GetCurrentDirectory();
        if (Directory.Exists(path))
        {
            directoryName = path;
        }
        else if (Path.Exists(path))
        {
            directoryName = Path.GetDirectoryName(path) ?? Directory.GetCurrentDirectory();
        }
        var response = new FilesAndSubdirectoriesDto
        {
            CurrentDirectory = directoryName,
            ParentDirectory = Directory.GetParent(directoryName)?.FullName,
            Subdirectories = Directory.GetDirectories(directoryName).Select(subDirectoryPath => Path.GetRelativePath(directoryName, subDirectoryPath)).ToArray(),
            Files = Directory.GetFiles(directoryName, "*.xml").Select(p => Path.GetFileName(p) ?? "").ToArray()
        };
        return Ok(response);
    }

    [HttpGet("combine/{path}/{fileName}")]
    [ProducesResponseType<string>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<string> Get([FromRoute] string path, [FromRoute] string fileName)
    {
        var fullPath = Path.Combine(path, fileName);
        if (!Path.Exists(fullPath))
        {
            return BadRequest("File does not exist!");
        }
        return Ok(fullPath);
    }

    private static FilesAndSubdirectoriesDto GetRootInformation()
    {
        var logicalDrives = Directory.GetLogicalDrives();
        var response = new FilesAndSubdirectoriesDto
        {
            CurrentDirectory = "/",
            ParentDirectory = null,
            Subdirectories = logicalDrives,
            Files = []
        };
        return response;
    }
}