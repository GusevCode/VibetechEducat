using Vibetech.Educat.Common.Models;
using Microsoft.AspNetCore.Identity;

namespace Vibetech.Educat.Common.Interfaces;

public interface IAuthService
{
    Task<User?> AuthenticateAsync(string login, string password);
    Task<User> RegisterAsync(User user, string password);
    Task<User?> GetUserByIdAsync(int id);
    Task<User?> GetUserByLoginAsync(string login);
    Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword);
    Task<bool> UpdateUserAsync(User user);
} 