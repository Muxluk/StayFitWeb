using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StayFit.Application.DTOs;
using StayFit.Application.Interfaces;
using StayFit.Infrastructure.Identity;

namespace StayFit.Web.Controllers;

public class AccountController(
    IRegistrationService registrationService,
    IAuthService authService,
    IPasswordResetService passwordResetService,
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager)
    : BaseController
{
    // ─── Реєстрація ─────────────────────────────────────────────────────────

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Register() => View();

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterRequest model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await registrationService.RegisterAsync(new RegisterUserRequestDto
        {
            UserName = model.UserName,
            Email = model.Email,
            Password = model.Password,
        });

        if (result.IsFailure)
        {
            AddResultErrorsToModelState(result);
            return View(model);
        }

        // Після реєстрації — одразу входимо
        var loginResult = await authService.LoginAsync(new LoginRequestDto
        {
            Email = model.Email,
            Password = model.Password,
        });

        if (loginResult.IsSuccess)
        {
            var newUser = await userManager.FindByNameAsync(loginResult.Value!);
            await signInManager.SignInAsync(newUser!, isPersistent: false);
        }

        // Перенаправити на редагування профіля
        return RedirectToAction("Edit", "Profile");
    }

    // ─── Вхід ───────────────────────────────────────────────────────────────

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null) =>
        View(new LoginRequest { ReturnUrl = returnUrl });

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginRequest model)
    {
        if (!ModelState.IsValid)
            return View(model);

        // AuthService перевіряє credentials і логує результат
        var result = await authService.LoginAsync(new LoginRequestDto
        {
            Email = model.Email,
            Password = model.Password,
        });

        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Errors.FirstOrDefault() ?? "Невірний email або пароль.");
            return View(model);
        }

        // Встановлюємо auth cookie через SignInManager (Web-шар)
        var user = await userManager.FindByNameAsync(result.Value!);
        await signInManager.SignInAsync(user!, isPersistent: false);

        if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return Redirect(model.ReturnUrl);

        return RedirectToAction("Index", "Home");
    }

    // ─── Вихід ──────────────────────────────────────────────────────────────

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    // ─── Забули пароль ──────────────────────────────────────────────────────

    [AllowAnonymous]
    [HttpGet]
    public IActionResult ForgotPassword() => View();

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await passwordResetService.SendPasswordResetTokenAsync(new ForgotPasswordDto
        {
            Email = model.Email,
        });

        if (result.IsFailure)
        {
            AddResultErrorsToModelState(result);

            return View(model);
        }

        return RedirectToAction(nameof(ForgotPasswordConfirmation));
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult ForgotPasswordConfirmation() => View();

    // ─── Скидання пароля ────────────────────────────────────────────────────

    [AllowAnonymous]
    [HttpGet]
    public IActionResult ResetPassword(string? email, string? token)
    {
        if (email is null || token is null)
            return BadRequest("Невірне посилання для скидання пароля.");

        return View(new ResetPasswordRequest { Email = email, Token = token });
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await passwordResetService.ResetPasswordAsync(new ResetPasswordDto
        {
            Email = model.Email,
            Token = model.Token,
            NewPassword = model.NewPassword,
        });

        if (result.IsFailure)
        {
            AddResultErrorsToModelState(result);
            return View(model);
        }

        return RedirectToAction(nameof(ResetPasswordConfirmation));
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult ResetPasswordConfirmation() => View();

    // ─── View-моделі ────────────────────────────────────────────────────────

    public sealed class RegisterRequest
    {
        [Display(Name = "Електронна пошта")]
        [Required(ErrorMessage = "Вкажіть електронну пошту")]
        [EmailAddress(ErrorMessage = "Введіть коректну електронну пошту")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Ім'я користувача")]
        [Required(ErrorMessage = "Вкажіть ім'я користувача")]
        [MinLength(3, ErrorMessage = "Ім'я користувача має містити щонайменше 3 символи")]
        public string UserName { get; set; } = string.Empty;

        [Display(Name = "Пароль")]
        [Required(ErrorMessage = "Вкажіть пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Підтвердження пароля")]
        [Required(ErrorMessage = "Підтвердіть пароль")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Паролі не співпадають")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public sealed class LoginRequest
    {
        [Display(Name = "Електронна пошта")]
        [Required(ErrorMessage = "Вкажіть електронну пошту")]
        [EmailAddress(ErrorMessage = "Введіть коректну електронну пошту")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Пароль")]
        [Required(ErrorMessage = "Вкажіть пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; }
    }

    public sealed class ForgotPasswordRequest
    {
        [Display(Name = "Електронна пошта")]
        [Required(ErrorMessage = "Вкажіть електронну пошту")]
        [EmailAddress(ErrorMessage = "Введіть коректну електронну пошту")]
        public string Email { get; set; } = string.Empty;
    }

    public sealed class ResetPasswordRequest
    {
        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;

        [Display(Name = "Новий пароль")]
        [Required(ErrorMessage = "Вкажіть новий пароль")]
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "Пароль має містити щонайменше 8 символів")]
        public string NewPassword { get; set; } = string.Empty;

        [Display(Name = "Підтвердження пароля")]
        [Required(ErrorMessage = "Підтвердіть пароль")]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "Паролі не співпадають")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
