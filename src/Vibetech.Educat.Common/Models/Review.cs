using System.ComponentModel.DataAnnotations;

namespace Vibetech.Educat.Common.Models;

public class Review : BaseEntity
{
    [Required]
    public int TeacherProfileId { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    [Range(1, 5)]
    public int Rating { get; set; }

    [Required]
    [StringLength(1000)]
    public string Comment { get; set; } = string.Empty;

    // Navigation properties
    public virtual TeacherProfile TeacherProfile { get; set; } = null!;
    public virtual User User { get; set; } = null!;
} 