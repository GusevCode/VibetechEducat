using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vibetech.Educat.Common.Models;

public class TeacherProfile : BaseEntity
{
    [Required]
    public int UserId { get; set; }

    [Required]
    [MaxLength(512)]
    public string Education { get; set; } = string.Empty;

    [Required]
    [Range(0, 50)]
    public int ExperienceYears { get; set; }

    [Required]
    [Range(0, 10000)]
    public decimal HourlyRate { get; set; }

    [Required]
    public bool IsModerated { get; set; } = false;

    [Required]
    public string[] PreparationPrograms { get; set; } = Array.Empty<string>();

    public double Rating { get; set; } = 0;
    public int ReviewsCount { get; set; } = 0;

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual ICollection<TeacherSubject> TeacherSubjects { get; set; } = new List<TeacherSubject>();
    public virtual ICollection<TeacherStudent> TeacherStudents { get; set; } = new List<TeacherStudent>();
    public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
} 