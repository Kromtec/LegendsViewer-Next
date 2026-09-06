using LegendsViewer.Backend.Contracts;
using LegendsViewer.Backend.DataAccess.Repositories.Interfaces;
using LegendsViewer.Backend.Extensions;
using LegendsViewer.Backend.Legends;
using Microsoft.AspNetCore.Mvc;

namespace LegendsViewer.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class WorldObjectGenericController<T>(IWorldObjectRepository<T> repository) : ControllerBase where T : WorldObject
{
    private const int DefaultPageSize = 10;
    private const int DefaultPageNumber = 1;
    protected readonly IWorldObjectRepository<T> Repository = repository;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<PaginatedResponse<WorldObjectDto>> Get(
        [FromQuery] int pageNumber = DefaultPageNumber,
        [FromQuery] int pageSize = DefaultPageSize,
        [FromQuery] string? sortKey = null,
        [FromQuery] string? sortOrder = null,
        [FromBody] WorldObjectFilterDto? filter = null)
    {
        // Validate pagination parameters
        if (pageNumber <= 0 || pageSize <= 0)
        {
            return BadRequest("Page number and page size must be greater than zero.");
        }

        // Filter world objects
        var filteredWorldObjects = filter != null ?
            Repository.GetAllElements().Where(worldObject => worldObject.MatchesFilterCriteria(filter)) :
            Repository.GetAllElements();

        // Get total number of elements
        int totalElements = Repository.GetCount();

        // Get total number of filtered elements
        int totalFilteredElements = filteredWorldObjects.Count();

        // Calculate how many elements to skip based on the page number and size
        var paginatedElements = filteredWorldObjects
            .SortByProperty(sortKey, sortOrder)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(worldObject => new WorldObjectDto(worldObject))
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
        var item = Repository.GetById(id);
        if (item == null)
        {
            return NotFound();
        }
        List<T> allElements = Repository.GetAllElements();

        int currentIndex = allElements.IndexOf(item);
        int previousIndex = currentIndex == 0 ? allElements.Count - 1 : currentIndex - 1;
        int nextIndex = allElements.Count == currentIndex + 1 ? 0 : currentIndex + 1;
        item.PreviousId = allElements[previousIndex].Id;
        item.NextId = allElements[nextIndex].Id;
        return Ok(item);
    }

    [HttpGet("count")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<int> GetCount()
    {
        return Ok(Repository.GetCount());
    }

    [HttpGet("{id}/events")]
    [HttpPost("{id}/events")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<PaginatedResponse<WorldEventDto>> GetEvents(
        [FromRoute] int id,
        [FromQuery] int pageNumber = DefaultPageNumber,
        [FromQuery] int pageSize = DefaultPageSize,
        [FromQuery] string? sortKey = null,
        [FromQuery] string? sortOrder = null,
        [FromBody] EventFilterDto? filter = null)
    {
        WorldObject? item = Repository.GetById(id);
        if (item == null)
        {
            return NotFound();
        }

        // Validate pagination parameters
        if (pageNumber <= 0 || pageSize <= 0)
        {
            return BadRequest("Page number and page size must be greater than zero.");
        }

        var filteredEvents = item.Events.Where(e => e.MatchesFilterCriteria(filter));

        // Get total number of elements
        int totalElements = filteredEvents.Count();

        // Calculate how many elements to skip based on the page number and size
        var paginatedElements = filteredEvents
            .SortByProperty(sortKey, sortOrder)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new WorldEventDto(e, item))
            .ToList();

        // Create a response object to include pagination metadata
        var response = new PaginatedResponse<WorldEventDto>
        {
            Items = paginatedElements,
            TotalCount = totalElements,
            TotalFilteredCount = totalElements,
            PageSize = pageSize,
            PageNumber = pageNumber,
            TotalPages = (int)Math.Ceiling(totalElements / (double)pageSize)
        };

        return Ok(response);
    }

    [HttpGet("{id}/eventcollections")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<PaginatedResponse<WorldObjectDto>> GetEventCollections(
        [FromRoute] int id,
        [FromQuery] int pageNumber = DefaultPageNumber,
        [FromQuery] int pageSize = DefaultPageSize,
        [FromQuery] string? sortKey = null,
        [FromQuery] string? sortOrder = null)
    {
        WorldObject? item = Repository.GetById(id);
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
        int totalElements = item.EventCollections.Count;

        // Calculate how many elements to skip based on the page number and size
        var paginatedElements = item.EventCollections
            .SortByProperty(sortKey, sortOrder)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new WorldObjectDto(e, item))
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

    [HttpGet("{id}/eventchart")]
    [HttpPost("{id}/eventchart")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<ChartDataDto> GetEventChart([FromRoute] int id, [FromBody] EventFilterDto? filter = null)
    {
        WorldObject? item = Repository.GetById(id);
        if (item == null)
        {
            return NotFound();
        }

        var response = new ChartDataDto();
        var dataset = new ChartDatasetDto
        {
            Label = "Events per Year"
        };

        var filteredEvents = item.Events.Where(e => e.MatchesFilterCriteria(filter));

        // Group by year and count events per year
        var eventCounts = filteredEvents
            .GroupBy(e => e.Year)
            .ToDictionary(g => g.Key, g => g.Count());

        const int startYear = 0;
        int endYear = item.World?.CurrentYear ?? (eventCounts.Keys.Count > 0 ? eventCounts.Keys.Max() : 0);

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

    [HttpGet("{id}/eventtypechart")]
    [HttpPost("{id}/eventtypechart")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<ChartDataDto> GetEventTypeChart([FromRoute] int id, [FromBody] EventFilterDto? filter = null)
    {
        WorldObject? item = Repository.GetById(id);
        if (item == null)
        {
            return NotFound();
        }

        var response = new ChartDataDto();
        var dataset = new ChartDatasetDto
        {
            Label = "Occurrences per Event Type"
        };

        // Group by type and count events per type (unfiltered so type selection popup retains all types)
        var eventCounts = item.Events
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
