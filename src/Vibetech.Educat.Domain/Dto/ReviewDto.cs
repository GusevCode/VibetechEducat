using System;

namespace Vibetech.Educat.Domain.Dto;

public class ReviewDto
{
    public int ReviewId { get; set; }
    public int StudentId { get; set; }
    public int TeacherId { get; set; }
    public int LessonId { get; set; }
    public int Rating { get; set; }
    public required string Comment { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public required UserDto Student { get; set; }
    public required UserDto Teacher { get; set; }
    public required LessonDto Lesson { get; set; }
} 