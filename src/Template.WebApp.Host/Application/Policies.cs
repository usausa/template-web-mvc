namespace Template.WebApp.Host.Application;

public static class Policies
{
    public const string Administrator = nameof(Administrator);
}

public static class Roles
{
    public const string Administrator = nameof(Administrator);

    // 登録経路で作られたアカウントの既定ロール
    public const string User = nameof(User);
}
