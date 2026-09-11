namespace Template.WebApp;

// appsettingsの既定(Auth:Enabled=false)と同じ、認証なしで使える状態を検証する
public sealed class AuthDisabledApplicationFactory : TestApplicationFactory
{
    protected override bool AuthEnabled => false;
}
