using Vibetech.Educat.Common.Interfaces;
using Vibetech.Educat.Common.Models;

namespace Vibetech.Educat.Domain.Services;

public class DatabaseStorageService : IStorageService
{
    private readonly IUnitOfWork _unitOfWork;

    public DatabaseStorageService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // User operations
    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _unitOfWork.Users.GetByIdAsync(id);
    }

    public async Task<User?> GetUserByLoginAsync(string login)
    {
        var users = await _unitOfWork.Users.FindAsync(u => u.Login == login);
        return users.FirstOrDefault();
    }

    public async Task<List<User>> GetUsersAsync()
    {
        return (await _unitOfWork.Users.GetAllAsync()).ToList();
    }

    public async Task AddUserAsync(User user)
    {
        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UpdateUserAsync(User user)
    {
        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();
    }

    // TeacherProfile operations
    public async Task<List<TeacherProfile>> GetTeacherProfilesAsync()
    {
        return (await _unitOfWork.TeacherProfiles.GetAllAsync()).ToList();
    }

    public async Task AddTeacherProfileAsync(TeacherProfile teacherProfile)
    {
        await _unitOfWork.TeacherProfiles.AddAsync(teacherProfile);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<TeacherProfile?> GetTeacherProfileByIdAsync(int id)
    {
        return await _unitOfWork.TeacherProfiles.GetByIdAsync(id);
    }

    public async Task<List<TeacherProfile>> GetTeacherProfilesBySubjectAsync(int subjectId)
    {
        var teacherProfiles = await _unitOfWork.TeacherProfiles.GetAllAsync();
        return teacherProfiles.Where(tp => tp.TeacherSubjects.Any(ts => ts.SubjectId == subjectId)).ToList();
    }

    public async Task<List<TeacherProfile>> GetTeacherProfilesByStudentAsync(int studentId)
    {
        var teacherProfiles = await _unitOfWork.TeacherProfiles.GetAllAsync();
        return teacherProfiles.Where(tp => tp.TeacherStudents.Any(ts => ts.StudentId == studentId)).ToList();
    }

    public async Task UpdateTeacherProfileAsync(TeacherProfile teacherProfile)
    {
        await _unitOfWork.TeacherProfiles.UpdateAsync(teacherProfile);
        await _unitOfWork.SaveChangesAsync();
    }

    // Subject operations
    public async Task<List<Subject>> GetSubjectsAsync()
    {
        return (await _unitOfWork.Subjects.GetAllAsync()).ToList();
    }

    public async Task AddSubjectAsync(Subject subject)
    {
        await _unitOfWork.Subjects.AddAsync(subject);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<Subject?> GetSubjectByIdAsync(int id)
    {
        return await _unitOfWork.Subjects.GetByIdAsync(id);
    }

    public async Task UpdateSubjectAsync(Subject subject)
    {
        await _unitOfWork.Subjects.UpdateAsync(subject);
        await _unitOfWork.SaveChangesAsync();
    }

    // Lesson operations
    public async Task<List<Lesson>> GetLessonsAsync()
    {
        return (await _unitOfWork.Lessons.GetAllAsync()).ToList();
    }

    public async Task AddLessonAsync(Lesson lesson)
    {
        await _unitOfWork.Lessons.AddAsync(lesson);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<Lesson?> GetLessonByIdAsync(int id)
    {
        return await _unitOfWork.Lessons.GetByIdAsync(id);
    }

    public async Task<List<Lesson>> GetLessonsByTeacherProfileIdAsync(int teacherProfileId)
    {
        var lessons = await _unitOfWork.Lessons.GetAllAsync();
        return lessons.Where(l => l.TeacherProfileId == teacherProfileId).ToList();
    }

    public async Task<List<Lesson>> GetLessonsByStudentIdAsync(int studentId)
    {
        var lessons = await _unitOfWork.Lessons.GetAllAsync();
        return lessons.Where(l => l.StudentId == studentId).ToList();
    }

    public async Task UpdateLessonAsync(Lesson lesson)
    {
        await _unitOfWork.Lessons.UpdateAsync(lesson);
        await _unitOfWork.SaveChangesAsync();
    }

    // Attachment operations
    public async Task<List<Attachment>> GetAttachmentsAsync()
    {
        return (await _unitOfWork.Attachments.GetAllAsync()).ToList();
    }

    public async Task AddAttachmentAsync(Attachment attachment)
    {
        await _unitOfWork.Attachments.AddAsync(attachment);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<Attachment?> GetAttachmentByIdAsync(int id)
    {
        return await _unitOfWork.Attachments.GetByIdAsync(id);
    }

    public async Task<List<Attachment>> GetAttachmentsByLessonIdAsync(int lessonId)
    {
        var attachments = await _unitOfWork.Attachments.GetAllAsync();
        return attachments.Where(a => a.LessonId == lessonId).ToList();
    }

    public async Task UpdateAttachmentAsync(Attachment attachment)
    {
        await _unitOfWork.Attachments.UpdateAsync(attachment);
        await _unitOfWork.SaveChangesAsync();
    }
} 