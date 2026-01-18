using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities; // Added for WebEncoders
using System.Text; // Added for Encoding
using System.Text.Encodings.Web; // Added for UrlEncoder
using SentimentHub.Web.Models;

namespace SentimentHub.Web.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailSender _emailSender;

    public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, IEmailSender emailSender)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _emailSender = emailSender;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (ModelState.IsValid)
        {
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);
                else
                    return RedirectToAction("Index", "Home");
            }
            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "User account locked out.");
                return View(model);
            }
            if (result.IsNotAllowed)
            {
                ModelState.AddModelError(string.Empty, "Email not confirmed.");
                return View(model);
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }
        }
        return View(model);
    }

    [HttpGet]
    public IActionResult Register(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (ModelState.IsValid)
        {
            var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                // Generate OTP
                var otp = new Random().Next(100000, 999999).ToString();
                user.EmailVerificationCode = otp;
                user.EmailVerificationCodeExpiresAt = DateTime.UtcNow.AddMinutes(10);
                await _userManager.UpdateAsync(user);

                // Send OTP
                await _emailSender.SendEmailAsync(model.Email, "E-posta Doğrulama Kodu",
                    $"Doğrulama kodunuz: {otp}");

                return RedirectToAction("VerifyCode", new { email = model.Email, returnUrl = returnUrl });
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
        return View(model);
    }

    [HttpGet]
    public IActionResult VerifyCode(string email, string? returnUrl = null)
    {
        if (email == null)
            return RedirectToAction("Index", "Home");

        return View(new VerifyCodeViewModel { Email = email, ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyCode(VerifyCodeViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Kullanıcı bulunamadı.");
            return View(model);
        }

        if (user.EmailConfirmed)
            return RedirectToAction("Index", "Home");

        if (user.EmailVerificationCode != model.Code)
        {
            ModelState.AddModelError("Code", "Geçersiz doğrulama kodu.");
            return View(model);
        }

        if (user.EmailVerificationCodeExpiresAt < DateTime.UtcNow)
        {
            ModelState.AddModelError("Code", "Doğrulama kodunun süresi dolmuş. Lütfen tekrar kayıt olun veya yeni kod isteyin.");
            return View(model);
        }

        // Verification Success
        user.EmailConfirmed = true;
        user.EmailVerificationCode = null;
        user.EmailVerificationCodeExpiresAt = null;
        await _userManager.UpdateAsync(user);
        
        await _signInManager.SignInAsync(user, isPersistent: false);
        
        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return Redirect(model.ReturnUrl);
        else
            return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}
