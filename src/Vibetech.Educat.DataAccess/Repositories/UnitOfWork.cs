using Microsoft.EntityFrameworkCore;
using Vibetech.Educat.Common.Interfaces;
using Vibetech.Educat.Common.Models;
using Vibetech.Educat.DataAccess.Data;
using Vibetech.Educat.DataAccess.Interfaces;

namespace Vibetech.Educat.DataAccess.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly EducatDbContext _context;
    private readonly IRepository<User> _users;
    private readonly IRepository<Subject> _subjects;
    private readonly IRepository<TeacherProfile> _teacherProfiles;
    private readonly IRepository<TeacherSubject> _teacherSubjects;
    private readonly IRepository<TeacherStudent> _teacherStudents;
    private readonly IRepository<Lesson> _lessons;
    private readonly IRepository<Attachment> _attachments;
    private readonly IRepository<Review> _reviews;

    public UnitOfWork(EducatDbContext context)
    {
        _context = context;
        _users = new Repository<User>(context);
        _subjects = new Repository<Subject>(context);
        _teacherProfiles = new Repository<TeacherProfile>(context);
        _teacherSubjects = new Repository<TeacherSubject>(context);
        _teacherStudents = new Repository<TeacherStudent>(context);
        _lessons = new Repository<Lesson>(context);
        _attachments = new Repository<Attachment>(context);
        _reviews = new Repository<Review>(context);
    }

    public IRepository<User> Users => _users;
    public IRepository<Subject> Subjects => _subjects;
    public IRepository<TeacherProfile> TeacherProfiles => _teacherProfiles;
    public IRepository<TeacherSubject> TeacherSubjects => _teacherSubjects;
    public IRepository<TeacherStudent> TeacherStudents => _teacherStudents;
    public IRepository<Lesson> Lessons => _lessons;
    public IRepository<Attachment> Attachments => _attachments;
    public IRepository<Review> Reviews => _reviews;

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
} 