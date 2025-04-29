using Vibetech.Educat.Common.Models;

namespace Vibetech.Educat.Domain.Services;

public interface IStorageService
{
    // User operations
    Task<User?> GetUserByIdAsync(int id);
    Task<User?> GetUserByLoginAsync(string login);
    Task<List<User>> GetUsersAsync();
    Task AddUserAsync(User user);
    Task UpdateUserAsync(User user);

    // TeacherProfile operations
    Task<List<TeacherProfile>> GetTeacherProfilesAsync();
    Task AddTeacherProfileAsync(TeacherProfile teacherProfile);
    Task<TeacherProfile?> GetTeacherProfileByIdAsync(int id);
    Task<List<TeacherProfile>> GetTeacherProfilesBySubjectAsync(int subjectId);
    Task<List<TeacherProfile>> GetTeacherProfilesByStudentAsync(int studentId);
    Task UpdateTeacherProfileAsync(TeacherProfile teacherProfile);

    // Subject operations
    Task<List<Subject>> GetSubjectsAsync();
    Task AddSubjectAsync(Subject subject);
    Task<Subject?> GetSubjectByIdAsync(int id);
    Task UpdateSubjectAsync(Subject subject);

    // Lesson operations
    Task<List<Lesson>> GetLessonsAsync();
    Task AddLessonAsync(Lesson lesson);
    Task<Lesson?> GetLessonByIdAsync(int id);
    Task<List<Lesson>> GetLessonsByTeacherProfileIdAsync(int teacherProfileId);
    Task<List<Lesson>> GetLessonsByStudentIdAsync(int studentId);
    Task UpdateLessonAsync(Lesson lesson);

    // Attachment operations
    Task<List<Attachment>> GetAttachmentsAsync();
    Task AddAttachmentAsync(Attachment attachment);
    Task<Attachment?> GetAttachmentByIdAsync(int id);
    Task<List<Attachment>> GetAttachmentsByLessonIdAsync(int lessonId);
    Task UpdateAttachmentAsync(Attachment attachment);
} 