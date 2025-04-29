using Vibetech.Educat.Common.Models;

namespace Vibetech.Educat.Common.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<User> Users { get; }
    IRepository<Subject> Subjects { get; }
    IRepository<TeacherProfile> TeacherProfiles { get; }
    IRepository<TeacherSubject> TeacherSubjects { get; }
    IRepository<TeacherStudent> TeacherStudents { get; }
    IRepository<Lesson> Lessons { get; }
    IRepository<Attachment> Attachments { get; }
    IRepository<Review> Reviews { get; }

    Task<int> SaveChangesAsync();
} 