using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Vibetech.Educat.Domain.Services;
using Vibetech.Educat.Common.Models;
using Vibetech.Educat.Common.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Vibetech.Educat.Pages.Account;

public class LoginModel : PageModel
{
    private readonly IAuthService _authService;

    public LoginModel(IAuthService authService)
    {
        _authService = authService;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string ErrorMessage { get; set; } = string.Empty;

    public class InputModel
    {
        [Required(ErrorMessage = "Выберите роль")]
        public string Role { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите логин")]
        [StringLength(512, ErrorMessage = "Максимальная длина — 512 символов")]
        [RegularExpression("^[a-zA-Z0-9]+$", ErrorMessage = "Логин должен содержать только латинские буквы и цифры")] 
        public string Login { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите пароль")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Пароль должен быть не менее 8 символов")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Запомнить меня")]
        public bool RememberMe { get; set; }
    }

    public void OnGet()
    {
        // Очистка ошибок при открытии страницы
        ErrorMessage = string.Empty;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _authService.AuthenticateAsync(Input.Login, Input.Password);
        if (user == null)
        {
            ErrorMessage = "Некорректный логин или пароль";
            return Page();
        }

        // Проверяем соответствие роли
        var expectedRole = Input.Role == "Tutor" ? "Teacher" : "Student";
        if (user.Role != expectedRole)
        {
            ErrorMessage = "Выбранная роль не соответствует вашей учетной записи";
            return Page();
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Login),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var claimsIdentity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = Input.RememberMe,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
        };

        await HttpContext.SignInAsync(
            IdentityConstants.ApplicationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        return RedirectToPage("/Index");
    }
} 