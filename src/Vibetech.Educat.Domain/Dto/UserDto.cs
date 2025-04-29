using System;
using System.ComponentModel.DataAnnotations;
using Vibetech.Educat.Common.Models;

namespace Vibetech.Educat.Domain.Dto;

public class UserDto
{
    public int UserId { get; set; }
    public required string Login { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string MiddleName { get; set; }
    public DateTime BirthDate { get; set; }
    public required string Gender { get; set; }
    public string? PhotoUrl { get; set; }
    public string? ContactInformation { get; set; }
    public required string Role { get; set; } // Student or Teacher
} 