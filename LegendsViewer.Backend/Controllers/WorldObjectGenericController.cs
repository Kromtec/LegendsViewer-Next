﻿using LegendsViewer.Backend.Contracts;
using LegendsViewer.Backend.Extensions;
using LegendsViewer.Backend.Legends;
using Microsoft.AspNetCore.Mvc;

namespace LegendsViewer.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class WorldObjectGenericController<T>(List<T> allElements, Func<int, T?> getById) : ControllerBase where T : WorldObject
{
    private const int DefaultPageSize = 10;
    protected readonly List<T> AllElements = allElements;
    protected readonly Func<int, T?> GetById = getById;

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<PaginatedResponse<WorldObjectDto>> Get(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = DefaultPageSize,
        [FromQuery] string? sortKey = null,
        [FromQuery] string? sortOrder = null,
        [FromQuery] string? search = null)
    {
        // Validate pagination parameters
        if (pageNumber <= 0 || pageSize <= 0)
        {
            return BadRequest("Page number and page size must be greater than zero.");
        }

        // Filter world objects
        var filteredWorldObjects = AllElements
            .Where(worldObject =>
                string.IsNullOrWhiteSpace(search) ||
                worldObject.Name.Contains(search, StringComparison.InvariantCultureIgnoreCase) ||
                worldObject.Type?.Contains(search, StringComparison.InvariantCultureIgnoreCase) == true ||
                worldObject.Subtype?.Contains(search, StringComparison.InvariantCultureIgnoreCase) == true);

        // Get total number of elements
        int totalElements = AllElements.Count;

        // Get total number of filtered elements
        int totalFilteredElements = filteredWorldObjects.Count();

        // Calculate how many elements to skip based on the page number and size
        var paginatedElements = filteredWorldObjects
            .Select(worldObject => new WorldObjectDto(worldObject))
            .SortByProperty(sortKey, sortOrder)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // Create a response object to include pagination metadata
        var response = new PaginatedResponse<WorldObjectDto>
        {
            Items = paginatedElements,
            TotalCount = totalElements,
            TotalFilteredCount = totalFilteredElements,
            PageSize = pageSize,
            PageNumber = pageNumber,
            TotalPages = (int)Math.Ceiling(totalElements / (double)pageSize)
        };

        return Ok(response);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<T> Get([FromRoute] int id)
    {
        var item = GetById(id);
        if (item == null)
        {
            return NotFound();
        }
        return Ok(item);
    }

    [HttpGet("count")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<int> GetCount()
    {
        return Ok(AllElements.Count);
    }

    [HttpGet("{id}/events")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<PaginatedResponse<WorldEventDto>> GetEvents(
        [FromRoute] int id,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = DefaultPageSize,
        [FromQuery] string? sortKey = null,
        [FromQuery] string? sortOrder = null)
    {
        WorldObject? item = GetById(id);
        if (item == null)
        {
            return NotFound();
        }

        // Validate pagination parameters
        if (pageNumber <= 0 || pageSize <= 0)
        {
            return BadRequest("Page number and page size must be greater than zero.");
        }

        // Get total number of elements
        int totalElements = item.Events.Count;

        // Calculate how many elements to skip based on the page number and size
        var paginatedElements = item.Events
            .Select(e => new WorldEventDto(e, item))
            .SortByProperty(sortKey, sortOrder)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // Create a response object to include pagination metadata
        var response = new PaginatedResponse<WorldEventDto>
        {
            Items = paginatedElements,
            TotalCount = totalElements,
            PageSize = pageSize,
            PageNumber = pageNumber,
            TotalPages = (int)Math.Ceiling(totalElements / (double)pageSize)
        };

        return Ok(response);
    }

    [HttpGet("{id}/eventchart")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<ChartDataDto> GetEventChart([FromRoute] int id)
    {
        WorldObject? item = GetById(id);
        if (item == null)
        {
            return NotFound();
        }

        var response = new ChartDataDto();
        var dataset = new ChartDatasetDto
        {
            Label = "Events per Year"
        };

        // Group by year and count events per year
        var eventCounts = item.Events
            .GroupBy(e => e.Year)
            .ToDictionary(g => g.Key, g => g.Count());

        const int startYear = 0;
        int endYear = item.World?.CurrentYear ?? eventCounts.Keys.Max();

        // Fill in missing years with 0 events
        for (int year = startYear; year <= endYear; year++)
        {
            if (!eventCounts.ContainsKey(year))
            {
                eventCounts[year] = 0;
            }
        }

        // Output the results (sorted by year)
        foreach (var eventItem in eventCounts.OrderBy(kv => kv.Key))
        {
            response.Labels.Add(eventItem.Key.ToString());
            dataset.Data.Add(eventItem.Value);
        }

        response.Datasets.Add(dataset);
        return Ok(response);
    }
}

public class PaginatedResponse<T> where T : class
{
    public List<T> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int TotalFilteredCount { get; set; }
    public int PageSize { get; set; }
    public int PageNumber { get; set; }
    public int TotalPages { get; set; }
}