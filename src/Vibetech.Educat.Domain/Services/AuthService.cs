using System.Text;
using Vibetech.Educat.Common.Models;
using Vibetech.Educat.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;

namespace Vibetech.Educat.Domain.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    public async Task<User?> AuthenticateAsync(string login, string password)
    {
        _logger.LogInformation("Попытка аутентификации пользователя {Login}", login);

        var user = await _userManager.FindByNameAsync(login);
        if (user == null)
        {
            _logger.LogWarning("Пользователь {Login} не найден", login);
            return null;
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, password, true);
        if (!result.Succeeded)
        {
            _logger.LogWarning("Неверный пароль для пользователя {Login}", login);
            return null;
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        _logger.LogInformation("Пользователь {Login} успешно аутентифицирован", login);
        return user;
    }

    public async Task<User> RegisterAsync(User user, string password)
    {
        _logger.LogInformation("Попытка регистрации пользователя {Login}", user.Login);

        // Check if user already exists
        var existingUser = await _userManager.FindByNameAsync(user.Login);
        if (existingUser != null)
        {
            _logger.LogWarning("Пользователь {Login} уже существует", user.Login);
            throw new Exception("Пользователь уже существует");
        }

        try
        {
            user.CreatedAt = DateTime.UtcNow;
            var result = await _userManager.CreateAsync(user, password);
            
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Ошибка при регистрации пользователя: {errors}");
            }

            _logger.LogInformation("Пользователь {Login} успешно зарегистрирован", user.Login);
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при регистрации пользователя {Login}", user.Login);
            throw;
        }
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _userManager.FindByIdAsync(id.ToString());
    }

    public async Task<User?> GetUserByLoginAsync(string login)
    {
        return await _userManager.FindByNameAsync(login);
    }

    public async Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
    {
        var user = await GetUserByIdAsync(userId);
        if (user == null)
            return false;

        var result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
        if (result.Succeeded)
        {
            user.UpdatedAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);
            return true;
        }

        return false;
    }

    public async Task<bool> UpdateUserAsync(User user)
    {
        user.UpdatedAt = DateTime.UtcNow;
        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }
} 