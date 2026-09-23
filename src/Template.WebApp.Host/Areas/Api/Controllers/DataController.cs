namespace Template.WebApp.Host.Areas.Api.Controllers;

using Smart.Mapper;

using Template.WebApp.Host.Application;

//--------------------------------------------------------------------------------
// Models
//--------------------------------------------------------------------------------

public sealed class DataListEntry
{
    public long Id { get; set; }

    public string Name { get; set; } = default!;

    public int Value { get; set; }

    public DateTime CreatedAt { get; set; }
}

public sealed class DataListResponse
{
    public int Total { get; set; }

    public int Page { get; set; }

    public int Size { get; set; }

    public IReadOnlyList<DataListEntry> Items { get; set; } = default!;
}

public sealed class DataResponse
{
    public long Id { get; set; }

    public string Name { get; set; } = default!;

    public int Value { get; set; }

    public DateTime CreatedAt { get; set; }
}

// MVCのモデル検証はrecordのプロパティ側属性を無視(例外)するため、クラスのプロパティに検証属性を付ける
public sealed class DataCreateRequest
{
    [Required]
    [MaxLength(Length.Name)]
    public string Name { get; set; } = default!;

    [Range(0, 999_999_999)]
    public int Value { get; set; }
}

public sealed class DataCreateResponse
{
    public long Id { get; set; }
}

public sealed class DataUpdateRequest
{
    [Required]
    [MaxLength(Length.Name)]
    public string Name { get; set; } = default!;

    [Range(0, 999_999_999)]
    public int Value { get; set; }
}

//--------------------------------------------------------------------------------
// Mapper
//--------------------------------------------------------------------------------

public static partial class DataMapper
{
    [Mapper]
    public static partial DataListEntry ToListEntry(this DataEntity entity);

    [Mapper]
    public static partial DataResponse ToResponse(this DataEntity entity);
}

//--------------------------------------------------------------------------------
// Controller
//--------------------------------------------------------------------------------

public sealed class DataController : BaseApiController
{
    private DataService DataService { get; }

    public DataController(
        DataService dataService)
    {
        DataService = dataService;
    }

    //--------------------------------------------------------------------------------
    // Query
    //--------------------------------------------------------------------------------

    [HttpGet]
    [ProducesResponseType<DataListResponse>(StatusCodes.Status200OK)]
    public async ValueTask<IActionResult> List(
        [FromQuery] string? name,
        [FromQuery] string? sort,
        CancellationToken cancellationToken,
        [FromQuery] bool desc = false,
        [FromQuery][Range(0, Int32.MaxValue)] int page = 0,
        [FromQuery][Range(1, 100)] int size = 20)
    {
        var result = await DataService.QueryPageAsync(name, RequestHelper.Parse(sort, DataSort.Id), desc, page, size, cancellationToken);
        return Ok(new DataListResponse
        {
            Total = result.Total,
            Page = result.Page,
            Size = result.Size,
            Items = result.Items.Select(static x => x.ToListEntry()).ToList()
        });
    }

    // ReSharper disable once RouteTemplates.RouteTokenNotResolved
    [HttpGet("~/[area]/[controller]/[action]/{id:long}")]
    [ProducesResponseType<DataResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<IActionResult> Get(long id)
    {
        var entity = await DataService.QueryAsync(id);
        return entity is not null ? Ok(entity.ToResponse()) : NotFound();
    }

    //--------------------------------------------------------------------------------
    // Command
    //--------------------------------------------------------------------------------

    [HttpPost]
    [ProducesResponseType<DataCreateResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async ValueTask<IActionResult> Create([FromBody] DataCreateRequest request)
    {
        var entity = new DataEntity { Name = request.Name, Value = request.Value };
        if (await DataService.InsertAsync(entity) != DataWriteStatus.Success)
        {
            return Problem(statusCode: StatusCodes.Status409Conflict, title: "Duplicate name.");
        }

        return CreatedAtAction(nameof(Get), new { id = entity.Id }, new DataCreateResponse { Id = entity.Id });
    }

    // ReSharper disable once RouteTemplates.RouteTokenNotResolved
    [HttpPost("~/[area]/[controller]/[action]/{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async ValueTask<IActionResult> Update(long id, [FromBody] DataUpdateRequest request)
    {
        var result = await DataService.UpdateAsync(id, request.Name, request.Value);
        return result switch
        {
            DataWriteStatus.Success => NoContent(),
            DataWriteStatus.NotFound => NotFound(),
            _ => Problem(statusCode: StatusCodes.Status409Conflict, title: "Duplicate name.")
        };
    }

    // ReSharper disable once RouteTemplates.RouteTokenNotResolved
    [HttpPost("~/[area]/[controller]/[action]/{id:long}")]
    [Authorize(Policy = Policies.Administrator)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<IActionResult> Delete(long id)
    {
        return await DataService.DeleteAsync(id) == DataWriteStatus.Success ? NoContent() : NotFound();
    }
}
