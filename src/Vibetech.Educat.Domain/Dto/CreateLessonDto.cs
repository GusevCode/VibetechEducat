using System.ComponentModel.DataAnnotations;

namespace Vibetech.Educat.Domain.Dto;

public class CreateLessonDto
{
    [Required(ErrorMessage = "Выберите предмет")]
    public int SubjectId { get; set; }
    
    public int? StudentId { get; set; }
    
    [Required(ErrorMessage = "Выберите дату и время начала урока")]
    public DateTime StartTime { get; set; } = DateTime.UtcNow.AddHours(1);
    
    [Required(ErrorMessage = "Выберите дату и время окончания урока")]
    public DateTime EndTime { get; set; } = DateTime.UtcNow.AddHours(2);
    
    [Url(ErrorMessage = "Введите корректную ссылку на видеоконференцию")]
    public string? MeetingLink { get; set; }
    
    [Url(ErrorMessage = "Введите корректную ссылку на виртуальную доску")]
    public string? WhiteboardLink { get; set; }
    
    // Ensure datetime values are in UTC before sending to the database
    public void EnsureUtcDateTimes()
    {
        if (StartTime.Kind != DateTimeKind.Utc)
        {
            StartTime = DateTime.SpecifyKind(StartTime, DateTimeKind.Utc);
        }
        
        if (EndTime.Kind != DateTimeKind.Utc)
        {
            EndTime = DateTime.SpecifyKind(EndTime, DateTimeKind.Utc);
        }
    }
} 