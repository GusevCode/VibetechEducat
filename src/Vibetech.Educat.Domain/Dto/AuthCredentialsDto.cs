using System.ComponentModel.DataAnnotations;
using Vibetech.Educat.Common.Models;

namespace Vibetech.Educat.Domain.Dto;

public class AuthCredentialsDto
{
    [Required(ErrorMessage = "Пароль обязателен")]
    [MinLength(6, ErrorMessage = "Минимальная длина пароля - 6 символов")]
    public required string Password { get; set; }
} 