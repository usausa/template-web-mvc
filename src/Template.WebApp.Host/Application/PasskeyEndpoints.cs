namespace Template.WebApp.Host.Application;

using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;

// パスキー操作(passkey-submit.js)が使うWebAuthnオプション発行エンドポイント
public static class PasskeyEndpoints
{
    //--------------------------------------------------------------------------------
    // Mapping
    //--------------------------------------------------------------------------------

    public static void MapPasskeyEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/account/passkey");

        group.MapPost("/creation-options", HandleCreationOptionsAsync)
            .RequireAuthorization()
            .WithName("PasskeyCreationOptions")
            .Produces(StatusCodes.Status200OK, contentType: "application/json")
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);
        group.MapPost("/request-options", HandleRequestOptionsAsync)
            .AllowAnonymous()
            .WithName("PasskeyRequestOptions")
            .Produces(StatusCodes.Status200OK, contentType: "application/json")
            .Produces(StatusCodes.Status400BadRequest);
    }

    //--------------------------------------------------------------------------------
    // Handler
    //--------------------------------------------------------------------------------

    // パスキー登録用オプション(WebAuthn creation options)を発行する
    [RequireAntiforgeryToken]
    private static async ValueTask<IResult> HandleCreationOptionsAsync(
        HttpContext context,
        UserManager<AccountEntity> userManager,
        SignInManager<AccountEntity> signInManager)
    {
        if (context.Features.Get<IAntiforgeryValidationFeature>() is not { IsValid: true })
        {
            return TypedResults.BadRequest();
        }

        var user = await userManager.GetUserAsync(context.User);
        if (user is null)
        {
            return TypedResults.NotFound();
        }

        var optionsJson = await signInManager.MakePasskeyCreationOptionsAsync(new()
        {
            Id = await userManager.GetUserIdAsync(user),
            Name = user.Name,
            DisplayName = user.Name
        });

        return TypedResults.Content(optionsJson, contentType: "application/json");
    }

    // パスキーログイン用オプション(WebAuthn request options)を発行する
    [RequireAntiforgeryToken]
    private static async ValueTask<IResult> HandleRequestOptionsAsync(
        HttpContext context,
        UserManager<AccountEntity> userManager,
        SignInManager<AccountEntity> signInManager,
        [FromQuery] string? username)
    {
        if (context.Features.Get<IAntiforgeryValidationFeature>() is not { IsValid: true })
        {
            return TypedResults.BadRequest();
        }

        var user = String.IsNullOrEmpty(username) ? null : await userManager.FindByNameAsync(username);
        var optionsJson = await signInManager.MakePasskeyRequestOptionsAsync(user);

        return TypedResults.Content(optionsJson, contentType: "application/json");
    }
}
