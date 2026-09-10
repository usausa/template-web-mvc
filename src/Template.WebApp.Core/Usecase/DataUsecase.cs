namespace Template.WebApp.Usecase;

using Template.WebApp.Models;
using Template.WebApp.Models.Entity;
using Template.WebApp.Models.Paging;
using Template.WebApp.Services;

public sealed class DataUsecase
{
    private readonly DataService dataService;

    public DataUsecase(DataService dataService)
    {
        this.dataService = dataService;
    }

    // API向け。ページ番号と件数で扱う(他テンプレートと同一シグネチャ)
    public async ValueTask<PagedResult<DataEntity>> QueryPageAsync(string? name, string? sort, bool desc, int page, int size, CancellationToken cancellationToken = default)
    {
        var total = await dataService.CountAsync(name, cancellationToken);
        var items = await dataService.QueryPageAsync(name, sort, desc, page * size, size, cancellationToken);
        return new PagedResult<DataEntity>(total, page, size, items);
    }

    // MVC画面向け。ページャー表示に必要な情報をPagedで返す
    public async ValueTask<Paged<DataEntity>> QueryPagedAsync(string? name, string? sort, bool desc, Pageable pageable, CancellationToken cancellationToken = default)
    {
        var count = await dataService.CountAsync(name, cancellationToken);
        var items = await dataService.QueryPageAsync(name, sort, desc, pageable.Offset, pageable.Size, cancellationToken);
        // ReSharper disable once UseCollectionExpression
        return new Paged<DataEntity>(pageable, items, count);
    }
}
