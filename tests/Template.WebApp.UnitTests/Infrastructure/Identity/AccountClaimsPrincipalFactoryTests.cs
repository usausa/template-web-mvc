namespace Template.WebApp.Infrastructure.Identity;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

using Template.WebApp.Host.Application;
using Template.WebApp.Host.Infrastructure.Identity;
using Template.WebApp.Models.Entity;

public sealed class AccountClaimsPrincipalFactoryTests
{
    [Fact]
    public async Task CreateAsyncAddsIdentityClaims()
    {
        // Arrange
        var options = new IdentityOptions();
        var factory = new AccountClaimsPrincipalFactory(Options.Create(options));
        var user = new AccountEntity { Id = 1, Name = "admin", Role = Roles.Administrator, SecurityStamp = "stamp" };

        // Act
        var principal = await factory.CreateAsync(user);

        // Assert
        Assert.True(principal.Identity!.IsAuthenticated);
        Assert.Equal(IdentityConstants.ApplicationScheme, principal.Identity.AuthenticationType);
        Assert.Equal("admin", principal.Identity.Name);
        Assert.Equal("1", principal.FindFirst(options.ClaimsIdentity.UserIdClaimType)?.Value);
        Assert.Equal("stamp", principal.FindFirst(options.ClaimsIdentity.SecurityStampClaimType)?.Value);
        Assert.True(principal.IsInRole(Roles.Administrator));
    }

    [Fact]
    public async Task CreateAsyncUsesAccountRole()
    {
        // Arrange
        var factory = new AccountClaimsPrincipalFactory(Options.Create(new IdentityOptions()));
        var user = new AccountEntity { Id = 2, Name = "user", Role = Roles.User, SecurityStamp = "stamp" };

        // Act
        var principal = await factory.CreateAsync(user);

        // Assert
        Assert.True(principal.IsInRole(Roles.User));
        Assert.False(principal.IsInRole(Roles.Administrator));
    }
}
