using Vibetech.Educat.Common.Models;
using Vibetech.Educat.Domain.Dto;

namespace Vibetech.Educat.Services.Services.TeacherService;

public interface ITeacherService
{
    Task<List<TeacherProfile>> GetTeachersAsync(string? subject = null);
    Task<TeacherProfile?> GetTeacherByIdAsync(int id);
    Task<List<Subject>> GetSubjectsAsync();
    Task<IEnumerable<TeacherProfile>> SearchTeachersAsync(string? subject, decimal? minPrice, decimal? maxPrice, int page = 1, int pageSize = 10);
    Task<List<Subject>> GetAvailableSubjectsAsync();
    Task<bool> CreateStudentRequestAsync(int studentId, int teacherProfileId);
    Task<bool> HasStudentRequestAsync(int studentId, int teacherProfileId);
    Task<RequestStatus?> GetRequestStatusAsync(int studentId, int teacherProfileId);
    Task<List<TeacherProfile>> GetAcceptedTeachersAsync(int studentId);
    Task<List<Lesson>> GetTeacherLessonsAsync(int teacherProfileId);
    Task<bool> CancelLessonAsync(int lessonId);
    Task<bool> CreateLessonAsync(int teacherProfileId, CreateLessonDto input);
}