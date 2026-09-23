namespace Template.WebApp.Accessors;

[DataAccessor]
public sealed partial class AccountPasskeyAccessor
{
    [Query]
    public partial ValueTask<List<AccountPasskeyEntity>> QueryByAccountAsync(long accountId);

    [QueryFirst]
    public partial ValueTask<AccountPasskeyEntity?> QueryByCredentialIdAsync(byte[] credentialId);

    [Execute]
    public partial ValueTask<int> UpsertAsync(byte[] credentialId, long accountId, string data);

    [Execute]
    public partial ValueTask<int> DeleteAsync(long accountId, byte[] credentialId);
}
