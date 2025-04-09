using LegendsViewer.Backend.Contracts;
using LegendsViewer.Backend.Extensions;
using LegendsViewer.Backend.Legends;
using LegendsViewer.Backend.Legends.Interfaces;
using LegendsViewer.Backend.Legends.Maps;
using Microsoft.AspNetCore.Mvc;

namespace LegendsViewer.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorldController(IWorld worldDataService, IWorldMapImageGenerator worldMapImageGenerator) : ControllerBase
{
    private const int DefaultPageSize = 10;
    private readonly IWorld _worldDataService = worldDataService;
    private readonly IWorldMapImageGenerator _worldMapImageGenerator = worldMapImageGenerator;

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<WorldDto> Get()
    {
        return Ok(new WorldDto(_worldDataService, _worldMapImageGenerator));
    }
    [HttpGet("events")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<PaginatedResponse<WorldEventDto>> GetEvents(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = DefaultPageSize,
        [FromQuery] string? sortKey = null,
        [FromQuery] string? sortOrder = null)
    {

        // Validate pagination parameters
        if (pageNumber <= 0 || pageSize <= 0)
        {
            return BadRequest("Page number and page size must be greater than zero.");
        }

        // Get total number of elements
        int totalElements = _worldDataService.Events.Count;

        // Calculate how many elements to skip based on the page number and size
        var paginatedElements = _worldDataService.Events
            .SortByProperty(sortKey, sortOrder)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new WorldEventDto(e))
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

    [HttpGet("eventcollections")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<PaginatedResponse<WorldObjectDto>> GetEventCollections(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = DefaultPageSize,
        [FromQuery] string? sortKey = null,
        [FromQuery] string? sortOrder = null)
    {
        // Validate pagination parameters
        if (pageNumber <= 0 || pageSize <= 0)
        {
            return BadRequest("Page number and page size must be greater than zero.");
        }

        // Get total number of elements
        int totalElements = _worldDataService.EventCollections.Count;

        // Calculate how many elements to skip based on the page number and size
        var paginatedElements = _worldDataService.EventCollections
            .SortByProperty(sortKey, sortOrder)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new WorldObjectDto(e))
            .ToList();

        // Create a response object to include pagination metadata
        var response = new PaginatedResponse<WorldObjectDto>
        {
            Items = paginatedElements,
            TotalCount = totalElements,
            PageSize = pageSize,
            PageNumber = pageNumber,
            TotalPages = (int)Math.Ceiling(totalElements / (double)pageSize)
        };

        return Ok(response);
    }

    [HttpGet("eventchart")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<ChartDataDto> GetEventChart()
    {
        var response = new ChartDataDto();
        var dataset = new ChartDatasetDto
        {
            Label = "Events per Year"
        };

        // Group by year and count events per year
        var eventCounts = _worldDataService.Events
            .GroupBy(e => e.Year)
            .ToDictionary(g => g.Key, g => g.Count());

        const int startYear = 0;
        int endYear = _worldDataService.CurrentYear;

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

    [HttpGet("eventtypechart")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<ChartDataDto> GetEventTypeChart()
    {
        var response = new ChartDataDto();
        var dataset = new ChartDatasetDto
        {
            Label = "Occurrences per Event Type"
        };

        // Group by type and count events per type
        var eventCounts = _worldDataService.Events
            .GroupBy(e => e.Type)
            .ToDictionary(g => g.Key, g => g.Count());

        // Output the results (sorted by count)
        foreach (var eventItem in eventCounts.OrderByDescending(kv => kv.Value))
        {
            response.Labels.Add($"{WorldEventExtensions.GetEventInfo(eventItem.Key)} ({eventItem.Key}) {eventItem.Value,10}");
            dataset.Data.Add(eventItem.Value);
        }

        response.Datasets.Add(dataset);
        return Ok(response);
    }
}
