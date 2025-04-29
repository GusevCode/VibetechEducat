using System.ComponentModel.DataAnnotations;
using Vibetech.Educat.Common.Models;

namespace Vibetech.Educat.Domain.Dto;

public class AttachmentDto
{
    public int Id { get; set; }
    public int LessonId { get; set; }
    public int UploadedById { get; set; }
    public string FileBase64 { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;

    public static AttachmentDto FromModel(Attachment attachment)
    {
        return new AttachmentDto
        {
            Id = attachment.Id,
            LessonId = attachment.LessonId,
            UploadedById = attachment.UploadedById,
            FileBase64 = attachment.FileBase64,
            FileName = attachment.FileName,
            ContentType = attachment.ContentType
        };
    }

    public Attachment ToModel()
    {
        return new Attachment
        {
            Id = Id,
            LessonId = LessonId,
            UploadedById = UploadedById,
            FileBase64 = FileBase64,
            FileName = FileName,
            ContentType = ContentType
        };
    }
} 