# template-web-mvc

English | [日本語](README.ja.md)

ASP.NET Core MVC template. Data access uses Smart.Data.Accessor (SQLite), and authentication integrates ASP.NET Core Identity Core **without EF Core**.

## Authentication

- By default (`Auth:Enabled=false`), authentication is not required. `[Authorize]` and the `Administrator` policy all pass through, and all features can be used without logging in
- Setting `Auth:Enabled=true` enables cookie authentication. Login (password / passkey), registration, passkey management, and lockout work
- The authentication flow (cookie / passkey / lockout) is left to `SignInManager` / `UserManager` of Identity, and only the storage of users is done with `AccountStore` (Smart.Data.Accessor). EF Core is not used

| Screen | Path | Notes |
|---|---|---|
| Login | `/account/login` | Password, or login with a passkey (also supports autofill with the conditional UI) |
| Registration | `/account/register` | The role of a created account is `User` |
| Passkey management | `/account/passkeys` | Registration (up to 5), renaming, deletion. Opened from the menu after login |

### Structure

| Element | Location | Role |
|---|---|---|
| `AccountEntity` / `AccountPasskeyEntity` | `Template.WebApp.Core/Models/Entity` | Users and passkeys. Correspond to the tables in `Database.sql` |
| `AccountAccessor` / `AccountPasskeyAccessor` | `Template.WebApp.Core/Accessors` | Persistence with Smart.Data.Accessor |
| `AccountStore` | `Template.WebApp.Host/Infrastructure/Identity` | Implementation of `IUserStore` / `IUserPasswordStore` / `IUserSecurityStampStore` / `IUserLockoutStore` / `IUserPasskeyStore` |
| `AccountPasswordHasher` | Same as above | Delegates the hashing of Identity to the existing `IPasswordProvider` (PBKDF2) |
| `AccountClaimsPrincipalFactory` | Same as above | Builds the claims put in the cookie. The entry point for customizing roles and business claims |
| `PasskeyEndpoints` | `Template.WebApp.Host/Application` | APIs that create the WebAuthn options (`/account/passkey/creation-options`, `/account/passkey/request-options`) |
| `AccountController` | `Areas/Default/Controllers` | Screens for login, registration, and passkey management |

### Roles and claims

- The `Account.Role` column is the source of truth for roles (`RoleManager` of Identity is not used). The constants are in `Roles` / `Policies`, and the `Administrator` policy is used for delete operations
- Business-specific claims (such as departments and permission flags) are added in `AccountClaimsPrincipalFactory.CreateAsync` by reading columns of `Account` or other tables
- The initial user is `Auth:InitialId` / `Auth:InitialPassword` (admin / admin, `Administrator`). `AccountService.InitializeAsync` creates it at startup

## Notes

- The password policy is relaxed to allow the initial password for development (admin). In operation, strengthen `options.Password` in `ConfigureAuthentication`
- Adjust the lockout count and duration with `IdentityOptions.Lockout` (5 times / 5 minutes by default)
- Passkeys require HTTPS except on localhost (an IP address cannot be used as the RP ID of WebAuthn)
- The tests assume that authentication is on (`TestApplicationFactory` / `E2EApplicationFactory`), and `AuthDisabledTests` separately verifies the state with authentication off. `PasskeyTests` of the E2E tests verifies passkey registration through login with the CDP WebAuthn virtual authenticator of Playwright
- MFA (TOTP + recovery codes) is not implemented

## E2E tests

The E2E tests (`tests/Template.WebApp.E2ETests`) run with Playwright. They are not part of the daily runs, and are run when needed and before releases. Run the commands in PowerShell at the repository root.

```powershell
# Get the browsers (first time. Uses the script output by the build)
dotnet build tests/Template.WebApp.E2ETests
pwsh tests/Template.WebApp.E2ETests/bin/Debug/net10.0/playwright.ps1 install

# Run (Chromium and headless by default)
dotnet run --project tests/Template.WebApp.E2ETests

# Run with the browser shown
$env:HEADED = "1"; dotnet run --project tests/Template.WebApp.E2ETests; Remove-Item Env:HEADED

# Switch the browser (chromium / firefox / webkit)
$env:BROWSER = "firefox"; dotnet run --project tests/Template.WebApp.E2ETests; Remove-Item Env:BROWSER

# Run step by step with Playwright Inspector
$env:PWDEBUG = "1"; dotnet run --project tests/Template.WebApp.E2ETests; Remove-Item Env:PWDEBUG
```

- Only for failed tests, a trace that records the screen, DOM, network, and console of each action is saved to `tests/Template.WebApp.E2ETests/bin/Debug/net10.0/playwright-traces/<test display name>.zip`. Open it with `pwsh tests/Template.WebApp.E2ETests/bin/Debug/net10.0/playwright.ps1 show-trace <zip>` or https://trace.playwright.dev
- The passkey tests (`PasskeyTests`) use the CDP virtual authenticator, so they pass only on Chromium (they fail on firefox / webkit)
