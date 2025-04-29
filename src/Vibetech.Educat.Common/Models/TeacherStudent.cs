using System.ComponentModel.DataAnnotations;

namespace Vibetech.Educat.Common.Models;

public class TeacherStudent : BaseEntity
{
    [Required]
    public int TeacherProfileId { get; set; }

    [Required]
    public int StudentId { get; set; }

    [Required]
    public RequestStatus Status { get; set; } = RequestStatus.Pending;

    // Навигационные свойства
    public virtual TeacherProfile TeacherProfile { get; set; } = null!;
    public virtual User Student { get; set; } = null!;
} 