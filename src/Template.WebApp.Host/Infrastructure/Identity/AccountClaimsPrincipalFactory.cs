namespace Template.WebApp.Host.Infrastructure.Identity;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

// ログイン時にCookieへ載せるクレームを組み立てる。ロールや業務クレームをカスタムする入口はここ
public sealed class AccountClaimsPrincipalFactory : IUserClaimsPrincipalFactory<AccountEntity>
{
    private readonly IdentityOptions options;

    public AccountClaimsPrincipalFactory(IOptions<IdentityOptions> options)
    {
        this.options = options.Value;
    }

    public Task<ClaimsPrincipal> CreateAsync(AccountEntity user)
    {
        var identity = new ClaimsIdentity(
            IdentityConstants.ApplicationScheme,
            options.ClaimsIdentity.UserNameClaimType,
            options.ClaimsIdentity.RoleClaimType);

        identity.AddClaim(new Claim(options.ClaimsIdentity.UserIdClaimType, user.Id.ToString(CultureInfo.InvariantCulture)));
        identity.AddClaim(new Claim(options.ClaimsIdentity.UserNameClaimType, user.Name));
        // セキュリティスタンプはCookieの失効判定(SecurityStampValidator)が参照する
        identity.AddClaim(new Claim(options.ClaimsIdentity.SecurityStampClaimType, user.SecurityStamp));

        // ロールはAccount.Role列を正とする(IdentityのRoleManagerは使わない)
        identity.AddClaim(new Claim(options.ClaimsIdentity.RoleClaimType, user.Role));

        // [MEMO] 部署や権限フラグなど業務固有のクレームは、Accountの列や別テーブルから読んでここに足す

        return Task.FromResult(new ClaimsPrincipal(identity));
    }
}
