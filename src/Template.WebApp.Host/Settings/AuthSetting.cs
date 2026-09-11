namespace Template.WebApp.Host.Settings;

public sealed class AuthSetting
{
    // falseのとき[Authorize]は匿名でも通る。認証の仕組みは残したまま、利用者に何も要求しない状態にする
    public bool Enabled { get; set; }

    [Range(1, 43200)]
    public int ExpireMinutes { get; set; }

    [Required]
    public string InitialId { get; set; } = default!;

    [Required]
    public string InitialPassword { get; set; } = default!;
}
