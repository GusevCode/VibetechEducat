using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vibetech.Educat.Common.Models;

public class TeacherSubject : BaseEntity
{
    [Required]
    public int TeacherProfileId { get; set; }

    [Required]
    public int SubjectId { get; set; }

    // Навигационные свойства
    public virtual TeacherProfile TeacherProfile { get; set; } = null!;
    public virtual Subject Subject { get; set; } = null!;
} 