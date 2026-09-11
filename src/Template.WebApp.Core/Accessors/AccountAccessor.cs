namespace Template.WebApp.Accessors;

[DataAccessor]
public sealed partial class AccountAccessor
{
    [Execute]
    public partial void Create();

    [ExecuteScalar]
    public partial ValueTask<int> CountAsync();

    [QueryFirst]
    public partial ValueTask<AccountEntity?> QueryByIdAsync(long id);

    [QueryFirst]
    public partial ValueTask<AccountEntity?> QueryByNormalizedNameAsync(string normalizedName);

    [ExecuteScalar]
    public partial ValueTask<long> InsertAsync(string name, string normalizedName, byte[] password, string role, string securityStamp, DateTime createdAt);

    [Execute]
    public partial ValueTask<int> UpdateAsync(long id, string name, string normalizedName, byte[] password, string role, string securityStamp, int accessFailedCount, DateTime? lockoutEnd);

    [Execute]
    public partial ValueTask<int> DeleteAsync(long id);
}
