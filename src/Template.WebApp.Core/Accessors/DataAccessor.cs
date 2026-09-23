namespace Template.WebApp.Accessors;

[DataAccessor]
public sealed partial class DataAccessor
{
    [ExecuteScalar]
    public partial ValueTask<int> CountAsync(string? name, CancellationToken cancellationToken);

    [Query]
    public partial ValueTask<List<DataEntity>> QueryPageAsync(string? name, DataSort sort, bool desc, int limit, int offset, CancellationToken cancellationToken);

    [Query]
    public partial ValueTask<List<DataEntity>> QueryAllAsync(CancellationToken cancellationToken);

    // ReSharper disable once CSharpWarnings::CS8424 (SDA0305がEnumeratorCancellation注釈を要求するため。実装はジェネレータ側で処理される)
    [Query]
    public partial IAsyncEnumerable<DataEntity> QueryExportEnumerable(string? name, DataSort sort, bool desc, [EnumeratorCancellation] CancellationToken cancellationToken);

    [QueryFirst]
    public partial ValueTask<DataEntity?> QueryAsync(long id);

    [ExecuteScalar]
    public partial ValueTask<long> InsertAsync(string name, int value, DateTime createdAt);

    [Execute]
    public partial ValueTask<int> UpdateAsync(long id, string name, int value);

    [Execute]
    public partial ValueTask<int> DeleteAsync(long id);
}
