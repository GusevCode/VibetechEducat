namespace Vibetech.Educat.Domain.Dto;

public class TeacherSubjectDto
{
    public int TeacherSubjectId { get; set; }
    public int TeacherProfileId { get; set; }
    public int SubjectId { get; set; }
    public required SubjectDto Subject { get; set; }
    public required List<string> PreparationPrograms { get; set; } = new();
} 