namespace Template.WebApp.Host.Areas.Default.Controllers;

using Microsoft.AspNetCore.Antiforgery;

using Template.WebApp.Host.Areas.Default.Models;

public sealed class SampleController : BaseDefaultController
{
    //--------------------------------------------------------------------------------
    // Validation
    //--------------------------------------------------------------------------------

    [HttpGet]
    public IActionResult Validation()
    {
        return View(new ValidationSampleForm());
    }

    [HttpPost]
    [RequireAntiforgeryToken]
    public IActionResult Validation([FromForm] ValidationSampleForm form)
    {
        if (ModelState.IsValid)
        {
            TempData.SetMessage("入力内容は正しいです");

            return RedirectToAction(nameof(Validation));
        }

        return View(form);
    }

    //--------------------------------------------------------------------------------
    // Conversion
    //--------------------------------------------------------------------------------

    [HttpGet]
    public IActionResult Conversion()
    {
        return View(new ConversionSampleForm());
    }

    [HttpPost]
    [RequireAntiforgeryToken]
    public IActionResult Conversion([FromForm] ConversionSampleForm form)
    {
        if (ModelState.IsValid)
        {
            ModelState.Clear();
            TempData.SetMessage("変換した値で受け付けました");
        }

        return View(form);
    }
}
