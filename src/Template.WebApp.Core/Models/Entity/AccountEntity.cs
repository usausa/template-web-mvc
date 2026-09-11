namespace Template.WebApp.Models.Entity;

// Identity Coreの利用者ストア(AccountStore)が読み書きする。EF Coreは使わず、Smart.Data.Accessorで永続化する
public sealed class AccountEntity
{
    public long Id { get; set; }

    public string Name { get; set; } = default!;

    // 大文字小文字を区別しない検索用。Identityの規約に合わせて正規化した値を持つ
    public string NormalizedName { get; set; } = default!;

#pragma warning disable CA1819
    public byte[] Password { get; set; } = default!;
#pragma warning restore CA1819

    public string Role { get; set; } = default!;

    // 資格情報が変わるたびに更新され、既存のCookieを失効させる
    public string SecurityStamp { get; set; } = default!;

    public int AccessFailedCount { get; set; }

    public DateTime? LockoutEnd { get; set; }

    public DateTime CreatedAt { get; set; }
}
