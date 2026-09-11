#pragma warning disable CA1716
namespace Template.WebApp.Host.Areas.Default.Controllers;
#pragma warning restore CA1716

using System.Buffers.Text;

using Microsoft.AspNetCore.Identity;

using Template.WebApp.Host.Areas.Default.Models;
using Template.WebApp.Host.Infrastructure.Identity;

// ログイン(パスワード / パスキー)・登録・パスキー管理。認証の流れはIdentity CoreのSignInManagerに任せる
#pragma warning disable CA1054
public sealed class AccountController : BaseDefaultController
{
    // 1アカウントが持てるパスキーの上限
    private const int MaxPasskeyCount = 5;

    private readonly SignInManager<AccountEntity> signInManager;

    private readonly UserManager<AccountEntity> userManager;

    private readonly ILogger<AccountController> log;

    public AccountController(
        SignInManager<AccountEntity> signInManager,
        UserManager<AccountEntity> userManager,
        ILogger<AccountController> log)
    {
        this.signInManager = signInManager;
        this.userManager = userManager;
        this.log = log;
    }

    //--------------------------------------------------------------------------------
    // Login
    //--------------------------------------------------------------------------------

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async ValueTask<IActionResult> Login([FromForm] AccountLoginForm form, string? returnUrl)
    {
        SignInResult result;
        if (!String.IsNullOrEmpty(form.Passkey?.CredentialJson))
        {
            // パスキー。ブラウザーが組み立てた資格情報をIdentityが検証する
            result = await signInManager.PasskeySignInAsync(form.Passkey.CredentialJson);
        }
        else if (!String.IsNullOrEmpty(form.Passkey?.Error))
        {
            ModelState.AddModelError(string.Empty, $"{Messages.PasskeyFailed}: {form.Passkey.Error}");
            return View(form);
        }
        else
        {
            if (String.IsNullOrEmpty(form.Password))
            {
                ModelState.AddModelError(nameof(form.Password), Messages.Required);
            }

            if (!ModelState.IsValid)
            {
                return View(form);
            }

            // 失敗をロックアウトに数える。回数と期間はIdentityOptions.Lockoutで調整する
            result = await signInManager.PasswordSignInAsync(form.Name, form.Password!, form.RememberMe, lockoutOnFailure: true);
        }

        if (result.Succeeded)
        {
            log.InfoUserLoggedIn();
            return !String.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl) : Redirect("~/");
        }

        ModelState.AddModelError(string.Empty, result.IsLockedOut ? Messages.LockedOut : Messages.LoginFailed);
        return View(form);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async ValueTask<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        log.InfoUserLoggedOut();

        return RedirectToAction(nameof(Login));
    }

    //--------------------------------------------------------------------------------
    // Register
    //--------------------------------------------------------------------------------

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async ValueTask<IActionResult> Register([FromForm] AccountRegisterForm form, string? returnUrl)
    {
        if (!ModelState.IsValid)
        {
            return View(form);
        }

        // ロール・スタンプ・作成日時はAccountStore.CreateAsyncとUserManagerが補う
        var user = new AccountEntity { Name = form.Name };
        var result = await userManager.CreateAsync(user, form.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(form);
        }

        log.InfoUserCreated();

        await signInManager.SignInAsync(user, isPersistent: false);
        return !String.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl) : Redirect("~/");
    }

    //--------------------------------------------------------------------------------
    // Passkey
    //--------------------------------------------------------------------------------

    [HttpGet]
    public async ValueTask<IActionResult> Passkeys()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return RedirectToAction(nameof(Login));
        }

        ViewBag.MaxPasskeyCount = MaxPasskeyCount;
        return View(await userManager.GetPasskeysAsync(user));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async ValueTask<IActionResult> AddPasskey([FromForm(Name = "Input")] PasskeyInputModel input)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return RedirectToAction(nameof(Login));
        }

        if (!String.IsNullOrEmpty(input.Error))
        {
            TempData.SetMessage($"{Messages.PasskeyFailed}: {input.Error}");
            return RedirectToAction(nameof(Passkeys));
        }

        if (String.IsNullOrEmpty(input.CredentialJson))
        {
            TempData.SetMessage("ブラウザーからパスキーが提供されませんでした");
            return RedirectToAction(nameof(Passkeys));
        }

        if ((await userManager.GetPasskeysAsync(user)).Count >= MaxPasskeyCount)
        {
            TempData.SetMessage("登録可能なパスキーの上限に達しています");
            return RedirectToAction(nameof(Passkeys));
        }

        // 登録時の検証(attestation)はIdentityが行う
        var attestation = await signInManager.PerformPasskeyAttestationAsync(input.CredentialJson);
        if (!attestation.Succeeded)
        {
            TempData.SetMessage($"{Messages.PasskeyFailed}: {attestation.Failure.Message}");
            return RedirectToAction(nameof(Passkeys));
        }

        var result = await userManager.AddOrUpdatePasskeyAsync(user, attestation.Passkey);
        if (!result.Succeeded)
        {
            TempData.SetMessage("パスキーをアカウントへ追加できませんでした");
            return RedirectToAction(nameof(Passkeys));
        }

        // 続けて名前の入力を促す
        return RedirectToAction(nameof(RenamePasskey), new { id = Base64Url.EncodeToString(attestation.Passkey.CredentialId) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async ValueTask<IActionResult> DeletePasskey([FromForm] string credentialId)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return RedirectToAction(nameof(Login));
        }

        if (!Base64Url.IsValid(credentialId))
        {
            return BadRequest();
        }

        await userManager.RemovePasskeyAsync(user, Base64Url.DecodeFromChars(credentialId));
        TempData.SetMessage("パスキーを削除しました");

        return RedirectToAction(nameof(Passkeys));
    }

    [HttpGet]
    public async ValueTask<IActionResult> RenamePasskey(string id)
    {
        var passkey = await FindPasskeyAsync(id);
        if (passkey is null)
        {
            return NotFound();
        }

        return View(new PasskeyRenameForm { Id = id, Name = passkey.Name ?? string.Empty });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async ValueTask<IActionResult> RenamePasskey([FromForm] PasskeyRenameForm form)
    {
        if (!ModelState.IsValid)
        {
            return View(form);
        }

        var user = await userManager.GetUserAsync(User);
        var passkey = await FindPasskeyAsync(form.Id);
        if ((user is null) || (passkey is null))
        {
            return NotFound();
        }

        passkey.Name = form.Name;
        await userManager.AddOrUpdatePasskeyAsync(user, passkey);
        TempData.SetMessage("パスキーの名前を変更しました");

        return RedirectToAction(nameof(Passkeys));
    }

    private async ValueTask<UserPasskeyInfo?> FindPasskeyAsync(string id)
    {
        if (!Base64Url.IsValid(id))
        {
            return null;
        }

        var user = await userManager.GetUserAsync(User);
        return user is null ? null : await userManager.GetPasskeyAsync(user, Base64Url.DecodeFromChars(id));
    }
}
#pragma warning restore CA1054
