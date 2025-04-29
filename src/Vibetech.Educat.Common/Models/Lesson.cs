using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace Vibetech.Educat.Common.Models;

public class Lesson : BaseEntity
{
    [Required]
    public int TeacherProfileId { get; set; }

    public int? StudentId { get; set; }

    [Required]
    public int SubjectId { get; set; }

    [Required]
    public DateTime ScheduledStart { get; set; }

    [Required]
    public DateTime ScheduledEnd { get; set; }

    public string? VideoCallUrl { get; set; }
    public string? WhiteboardUrl { get; set; }
    
    [Required]
    public bool IsCancelled { get; set; }

    [NotMapped]
    public LessonStatus Status
    {
        get
        {
            if (IsCancelled) return LessonStatus.Cancelled;

            var now = DateTime.UtcNow;
            if (now < ScheduledStart) return LessonStatus.Scheduled;
            if (now <= ScheduledEnd) return LessonStatus.InProgress;
            
            return LessonStatus.Completed;
        }
    }

    // Navigation properties
    public virtual TeacherProfile TeacherProfile { get; set; } = null!;
    public virtual User? Student { get; set; }
    public virtual Subject Subject { get; set; } = null!;
    public virtual ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
} 