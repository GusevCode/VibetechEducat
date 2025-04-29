using System.ComponentModel.DataAnnotations;

namespace Vibetech.Educat.Common.Models;

public class Attachment : BaseEntity
{

    [Required]
    public int LessonId { get; set; }

    [Required]
    public int UploadedById { get; set; }

    [Required]
    [StringLength(256)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    public string ContentType { get; set; } = string.Empty; // "application/pdf", "image/jpeg", "image/png"

    [Required]
    [MaxLength(200 * 1024 * 1024)] // 200 MB
    public string FileBase64 { get; set; } = string.Empty;

    // Navigation properties
    public virtual Lesson Lesson { get; set; } = null!;
    public virtual User UploadedBy { get; set; } = null!;
} 