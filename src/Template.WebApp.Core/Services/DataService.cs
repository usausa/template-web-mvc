namespace Template.WebApp.Services;

using Template.WebApp.Accessors;
using Template.WebApp.Models;
using Template.WebApp.Models.Entity;
using Template.WebApp.Models.Paging;

public sealed class DataService
{
    private readonly IDialect dialect;

    private readonly DataAccessor dataAccessor;

    private readonly ServiceContextProvider contextProvider;

    public DataService(
        IDialect dialect,
        DataAccessor dataAccessor,
        ServiceContextProvider contextProvider)
    {
        this.dialect = dialect;
        this.dataAccessor = dataAccessor;
        this.contextProvider = contextProvider;
    }

    public ValueTask<int> CountAsync(string? name, CancellationToken cancellationToken = default) =>
        dataAccessor.CountAsync(dialect.Match(name), cancellationToken);

    // API向け。ページ番号と件数で扱い、総件数と合わせて返す
    public async ValueTask<PagedResult<DataEntity>> QueryPageAsync(string? name, DataSort sort, bool desc, int page, int size, CancellationToken cancellationToken = default)
    {
        var pattern = dialect.Match(name);
        var total = await dataAccessor.CountAsync(pattern, cancellationToken);
        var items = await dataAccessor.QueryPageAsync(pattern, sort, desc, size, page * size, cancellationToken);
        return new PagedResult<DataEntity>(total, page, size, items);
    }

    // MVC画面向け。ページャー表示に必要な情報をPagedで返す
    public async ValueTask<Paged<DataEntity>> QueryPagedAsync(string? name, DataSort sort, bool desc, Pageable pageable, CancellationToken cancellationToken = default)
    {
        var pattern = dialect.Match(name);
        var count = await dataAccessor.CountAsync(pattern, cancellationToken);
        var items = await dataAccessor.QueryPageAsync(pattern, sort, desc, pageable.Size, pageable.Offset, cancellationToken);
        // ReSharper disable once UseCollectionExpression
        return new Paged<DataEntity>(pageable, items, count);
    }

    public ValueTask<List<DataEntity>> QueryAllAsync(CancellationToken cancellationToken = default) =>
        dataAccessor.QueryAllAsync(cancellationToken);

    public IAsyncEnumerable<DataEntity> QueryExportEnumerable(string? name, DataSort sort, bool desc, CancellationToken cancellationToken) =>
        dataAccessor.QueryExportEnumerable(dialect.Match(name), sort, desc, cancellationToken);

    public ValueTask<DataEntity?> QueryAsync(long id) =>
        dataAccessor.QueryAsync(id);

    public async ValueTask<DataWriteStatus> InsertAsync(DataEntity entity)
    {
        var context = contextProvider.Current;

        try
        {
            entity.CreatedAt = context.Now.DateTime;
            entity.Id = await dataAccessor.InsertAsync(entity.Name, entity.Value, entity.CreatedAt);
            return DataWriteStatus.Success;
        }
        catch (DbException ex) when (dialect.IsDuplicate(ex))
        {
            return DataWriteStatus.Duplicate;
        }
    }

    public async ValueTask<DataWriteStatus> UpdateAsync(long id, string name, int value)
    {
        try
        {
            var rows = await dataAccessor.UpdateAsync(id, name, value);
            return rows > 0 ? DataWriteStatus.Success : DataWriteStatus.NotFound;
        }
        catch (DbException ex) when (dialect.IsDuplicate(ex))
        {
            return DataWriteStatus.Duplicate;
        }
    }

    public async ValueTask<DataWriteStatus> DeleteAsync(long id) =>
        await dataAccessor.DeleteAsync(id) > 0 ? DataWriteStatus.Success : DataWriteStatus.NotFound;
}
