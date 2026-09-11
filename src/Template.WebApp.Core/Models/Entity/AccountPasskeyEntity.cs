namespace Template.WebApp.Models.Entity;

// パスキー1件。公開鍵などの詳細はIdentity側の型をJSONにして保持する(EF Coreの既定実装と同じ方式)
public sealed class AccountPasskeyEntity
{
#pragma warning disable CA1819
    public byte[] CredentialId { get; set; } = default!;
#pragma warning restore CA1819

    public long AccountId { get; set; }

    public string Data { get; set; } = default!;
}
