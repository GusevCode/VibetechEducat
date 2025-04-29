namespace Vibetech.Educat.Domain.Dto;

public class TeacherProfileDto
{
    public int TeacherProfileId { get; set; }
    public int UserId { get; set; }
    public required string Education { get; set; }
    public int ExperienceYears { get; set; }
    public required string VerificationStatus { get; set; }
    public decimal HourlyRate { get; set; }
    public required string AboutMe { get; set; }
    public required UserDto User { get; set; }
    public required List<TeacherSubjectDto> Subjects { get; set; } = new();
} 