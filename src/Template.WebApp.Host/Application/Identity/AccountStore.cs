namespace Template.WebApp.Host.Application.Identity;

using System.Text.Json;

using Microsoft.AspNetCore.Identity;

using Template.WebApp.Accessors;

// Identity Coreの利用者ストア。EF Coreを使わず、Smart.Data.Accessorで永続化する。
// UserManagerはSetXxxAsyncでエンティティを書き換えたあとUpdateAsyncで保存する規約のため、
// SetXxxAsyncはメモリ上の変更のみ、Create / Update / DeleteでDBへ反映する
public sealed class AccountStore :
    IUserPasswordStore<AccountEntity>,
    IUserSecurityStampStore<AccountEntity>,
    IUserLockoutStore<AccountEntity>,
    IUserPasskeyStore<AccountEntity>
{
    private static readonly JsonSerializerOptions PasskeyJsonOptions = new(JsonSerializerDefaults.Web);

    private readonly AccountAccessor accountAccessor;

    private readonly AccountPasskeyAccessor accountPasskeyAccessor;

    private readonly TimeProvider timeProvider;

    public AccountStore(
        AccountAccessor accountAccessor,
        AccountPasskeyAccessor accountPasskeyAccessor,
        TimeProvider timeProvider)
    {
        this.accountAccessor = accountAccessor;
        this.accountPasskeyAccessor = accountPasskeyAccessor;
        this.timeProvider = timeProvider;
    }

    public void Dispose()
    {
    }

    //--------------------------------------------------------------------------------
    // IUserStore
    //--------------------------------------------------------------------------------

    public Task<string> GetUserIdAsync(AccountEntity user, CancellationToken cancellationToken) =>
        Task.FromResult(user.Id.ToString(CultureInfo.InvariantCulture));

    public Task<string?> GetUserNameAsync(AccountEntity user, CancellationToken cancellationToken) =>
        Task.FromResult<string?>(user.Name);

    public Task SetUserNameAsync(AccountEntity user, string? userName, CancellationToken cancellationToken)
    {
        user.Name = userName ?? throw new ArgumentNullException(nameof(userName));
        return Task.CompletedTask;
    }

    public Task<string?> GetNormalizedUserNameAsync(AccountEntity user, CancellationToken cancellationToken) =>
        Task.FromResult<string?>(user.NormalizedName);

    public Task SetNormalizedUserNameAsync(AccountEntity user, string? normalizedName, CancellationToken cancellationToken)
    {
        user.NormalizedName = normalizedName ?? throw new ArgumentNullException(nameof(normalizedName));
        return Task.CompletedTask;
    }

    public async Task<IdentityResult> CreateAsync(AccountEntity user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // 登録経路ではロールを持たないため、既定を一般利用者にする
        if (String.IsNullOrEmpty(user.Role))
        {
            user.Role = Roles.User;
        }

        user.CreatedAt = timeProvider.GetLocalNow().DateTime;
        user.Id = await accountAccessor.InsertAsync(user.Name, user.NormalizedName, user.Password, user.Role, user.SecurityStamp, user.CreatedAt);

        return IdentityResult.Success;
    }

    public async Task<IdentityResult> UpdateAsync(AccountEntity user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var rows = await accountAccessor.UpdateAsync(user.Id, user.Name, user.NormalizedName, user.Password, user.Role, user.SecurityStamp, user.AccessFailedCount, user.LockoutEnd);

        return rows > 0 ? IdentityResult.Success : IdentityResult.Failed(new IdentityError { Code = "NotFound", Description = "Account not found." });
    }

    public async Task<IdentityResult> DeleteAsync(AccountEntity user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await accountAccessor.DeleteAsync(user.Id);

        return IdentityResult.Success;
    }

    public async Task<AccountEntity?> FindByIdAsync(string userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Int64.TryParse(userId, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id)
            ? await accountAccessor.QueryByIdAsync(id)
            : null;
    }

    public async Task<AccountEntity?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return await accountAccessor.QueryByNormalizedNameAsync(normalizedUserName);
    }

    //--------------------------------------------------------------------------------
    // IUserPasswordStore(ハッシュはAccountPasswordHasherが生成するBase64文字列)
    //--------------------------------------------------------------------------------

    public Task SetPasswordHashAsync(AccountEntity user, string? passwordHash, CancellationToken cancellationToken)
    {
        user.Password = passwordHash is null ? [] : Convert.FromBase64String(passwordHash);
        return Task.CompletedTask;
    }

    public Task<string?> GetPasswordHashAsync(AccountEntity user, CancellationToken cancellationToken) =>
        Task.FromResult(user.Password.Length > 0 ? Convert.ToBase64String(user.Password) : null);

    public Task<bool> HasPasswordAsync(AccountEntity user, CancellationToken cancellationToken) =>
        Task.FromResult(user.Password.Length > 0);

    //--------------------------------------------------------------------------------
    // IUserSecurityStampStore
    //--------------------------------------------------------------------------------

    public Task SetSecurityStampAsync(AccountEntity user, string stamp, CancellationToken cancellationToken)
    {
        user.SecurityStamp = stamp;
        return Task.CompletedTask;
    }

    public Task<string?> GetSecurityStampAsync(AccountEntity user, CancellationToken cancellationToken) =>
        Task.FromResult<string?>(user.SecurityStamp);

    //--------------------------------------------------------------------------------
    // IUserLockoutStore
    //--------------------------------------------------------------------------------

    public Task<DateTimeOffset?> GetLockoutEndDateAsync(AccountEntity user, CancellationToken cancellationToken) =>
        Task.FromResult<DateTimeOffset?>(user.LockoutEnd.HasValue ? new DateTimeOffset(user.LockoutEnd.Value) : null);

    public Task SetLockoutEndDateAsync(AccountEntity user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken)
    {
        user.LockoutEnd = lockoutEnd?.LocalDateTime;
        return Task.CompletedTask;
    }

    public Task<int> IncrementAccessFailedCountAsync(AccountEntity user, CancellationToken cancellationToken)
    {
        user.AccessFailedCount++;
        return Task.FromResult(user.AccessFailedCount);
    }

    public Task ResetAccessFailedCountAsync(AccountEntity user, CancellationToken cancellationToken)
    {
        user.AccessFailedCount = 0;
        return Task.CompletedTask;
    }

    public Task<int> GetAccessFailedCountAsync(AccountEntity user, CancellationToken cancellationToken) =>
        Task.FromResult(user.AccessFailedCount);

    public Task<bool> GetLockoutEnabledAsync(AccountEntity user, CancellationToken cancellationToken) =>
        Task.FromResult(true);

    public Task SetLockoutEnabledAsync(AccountEntity user, bool enabled, CancellationToken cancellationToken) =>
        Task.CompletedTask;

    //--------------------------------------------------------------------------------
    // IUserPasskeyStore(パスキーは即時に永続化する。UserManagerは続けてUpdateAsyncも呼ぶ)
    //--------------------------------------------------------------------------------

    public async Task AddOrUpdatePasskeyAsync(AccountEntity user, UserPasskeyInfo passkey, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var data = JsonSerializer.Serialize(PasskeyData.From(passkey), PasskeyJsonOptions);
        await accountPasskeyAccessor.UpsertAsync(passkey.CredentialId, user.Id, data);
    }

    public async Task<IList<UserPasskeyInfo>> GetPasskeysAsync(AccountEntity user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var entities = await accountPasskeyAccessor.QueryByAccountAsync(user.Id);
        return entities.Select(ToPasskeyInfo).ToList();
    }

    public async Task<AccountEntity?> FindByPasskeyIdAsync(byte[] credentialId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var entity = await accountPasskeyAccessor.QueryByCredentialIdAsync(credentialId);
        return entity is null ? null : await accountAccessor.QueryByIdAsync(entity.AccountId);
    }

    public async Task<UserPasskeyInfo?> FindPasskeyAsync(AccountEntity user, byte[] credentialId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var entity = await accountPasskeyAccessor.QueryByCredentialIdAsync(credentialId);
        return (entity is null) || (entity.AccountId != user.Id) ? null : ToPasskeyInfo(entity);
    }

    public async Task RemovePasskeyAsync(AccountEntity user, byte[] credentialId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await accountPasskeyAccessor.DeleteAsync(user.Id, credentialId);
    }

    private static UserPasskeyInfo ToPasskeyInfo(AccountPasskeyEntity entity)
    {
        var data = JsonSerializer.Deserialize<PasskeyData>(entity.Data, PasskeyJsonOptions)!;
        return data.ToPasskeyInfo(entity.CredentialId);
    }

    // 公開鍵などの詳細をJSONで保持するための転送用。UserPasskeyInfoの項目と対応する
    private sealed class PasskeyData
    {
        public byte[] PublicKey { get; set; } = default!;

        public string? Name { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public uint SignCount { get; set; }

        public string[]? Transports { get; set; }

        public bool IsUserVerified { get; set; }

        public bool IsBackupEligible { get; set; }

        public bool IsBackedUp { get; set; }

        public byte[] AttestationObject { get; set; } = default!;

        public byte[] ClientDataJson { get; set; } = default!;

        public static PasskeyData From(UserPasskeyInfo info) => new()
        {
            PublicKey = info.PublicKey,
            Name = info.Name,
            CreatedAt = info.CreatedAt,
            SignCount = info.SignCount,
            Transports = info.Transports,
            IsUserVerified = info.IsUserVerified,
            IsBackupEligible = info.IsBackupEligible,
            IsBackedUp = info.IsBackedUp,
            AttestationObject = info.AttestationObject,
            ClientDataJson = info.ClientDataJson
        };

        public UserPasskeyInfo ToPasskeyInfo(byte[] credentialId) =>
            new(credentialId, PublicKey, CreatedAt, SignCount, Transports, IsUserVerified, IsBackupEligible, IsBackedUp, AttestationObject, ClientDataJson)
            {
                Name = Name
            };
    }
}
