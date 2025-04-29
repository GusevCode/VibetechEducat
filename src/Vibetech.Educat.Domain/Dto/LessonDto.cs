using System;
using System.ComponentModel.DataAnnotations;
using Vibetech.Educat.Common.Models;

namespace Vibetech.Educat.Domain.Dto;

public class LessonDto
{
    public int Id { get; set; }
    public int TeacherProfileId { get; set; }
    public int? StudentId { get; set; }
    public int SubjectId { get; set; }
    public string? VideoCallUrl { get; set; }
    public string? WhiteboardUrl { get; set; }
    public DateTime ScheduledStart { get; set; }
    public DateTime ScheduledEnd { get; set; }

    public static LessonDto FromModel(Lesson lesson)
    {
        return new LessonDto
        {
            Id = lesson.Id,
            TeacherProfileId = lesson.TeacherProfileId,
            StudentId = lesson.StudentId,
            SubjectId = lesson.SubjectId,
            VideoCallUrl = lesson.VideoCallUrl,
            WhiteboardUrl = lesson.WhiteboardUrl,
            ScheduledStart = lesson.ScheduledStart,
            ScheduledEnd = lesson.ScheduledEnd
        };
    }

    public Lesson ToModel()
    {
        return new Lesson
        {
            Id = Id,
            TeacherProfileId = TeacherProfileId,
            StudentId = StudentId,
            SubjectId = SubjectId,
            VideoCallUrl = VideoCallUrl,
            WhiteboardUrl = WhiteboardUrl,
            ScheduledStart = ScheduledStart,
            ScheduledEnd = ScheduledEnd
        };
    }
} 