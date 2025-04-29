using Vibetech.Educat.Common.Interfaces;
using Vibetech.Educat.Common.Models;
using Vibetech.Educat.Domain.Dto;

namespace Vibetech.Educat.Services.Services.TeacherService;

public class TeacherService : ITeacherService
{
    private readonly IUnitOfWork _unitOfWork;

    public TeacherService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    public async Task<List<TeacherProfile>> GetTeachersAsync(string? subject = null)
    {
        var teachers = await _unitOfWork.TeacherProfiles.GetAllWithIncludesAsync(t => t.User);

        if (!string.IsNullOrEmpty(subject))
        {
            teachers = teachers.Where(t => t.TeacherSubjects.Any(ts => ts.Subject.Name == subject)).ToList();
        }

        return teachers.ToList();
    }

    public async Task<TeacherProfile?> GetTeacherByIdAsync(int id)
    {
        return await _unitOfWork.TeacherProfiles.GetByIdAsync(id);
    }

    public async Task<List<Subject>> GetSubjectsAsync()
    {
        return (await _unitOfWork.Subjects.GetAllAsync()).ToList();
    }

    public async Task<IEnumerable<TeacherProfile>> SearchTeachersAsync(string? subject, decimal? minPrice, decimal? maxPrice, int page = 1, int pageSize = 10)
    {
        var teachers = await GetTeachersAsync(subject);

        if (minPrice.HasValue)
        {
            teachers = teachers.Where(t => t.HourlyRate >= minPrice.Value).ToList();
        }

        if (maxPrice.HasValue)
        {
            teachers = teachers.Where(t => t.HourlyRate <= maxPrice.Value).ToList();
        }

        return teachers.Skip((page - 1) * pageSize).Take(pageSize);
    }

    public async Task<List<Subject>> GetAvailableSubjectsAsync()
    {
        return await GetSubjectsAsync();
    }
    
    public async Task<bool> ApproveRequest(int requestId) 
        => await UpdateStudentRequest(requestId, RequestStatus.Accepted);

    public async Task<bool> RejectRequest(int requestId) 
        => await UpdateStudentRequest(requestId, RequestStatus.Rejected);
    
    public async Task<bool> CreateStudentRequestAsync(int studentId, int teacherProfileId)
    {
        try
        {
            // Проверяем существование учителя
            var teacher = await _unitOfWork.TeacherProfiles.GetByIdAsync(teacherProfileId);
            if (teacher == null)
            {
                Console.WriteLine($"Teacher with ID {teacherProfileId} not found");
                return false;
            }

            // Проверяем существование студента
            var student = await _unitOfWork.Users.GetByIdAsync(studentId);
            if (student == null)
            {
                Console.WriteLine($"Student with ID {studentId} not found");
                return false;
            }

            // Проверяем, не существует ли уже заявка
            var existingRequest = (await _unitOfWork.TeacherStudents.GetAllAsync())
                .FirstOrDefault(r => r.StudentId == studentId && r.TeacherProfileId == teacherProfileId);
            
            if (existingRequest != null)
            {
                await UpdateStudentRequest(existingRequest.Id, RequestStatus.Pending);
                Console.WriteLine($"Request already exists between student {studentId} and teacher {teacherProfileId}");
                return false;
            }

            var request = new TeacherStudent 
            {
                StudentId = studentId,
                TeacherProfileId = teacherProfileId,
                Status = RequestStatus.Pending
            };

            await _unitOfWork.TeacherStudents.AddAsync(request);
            await _unitOfWork.SaveChangesAsync();
            Console.WriteLine($"Successfully created request between student {studentId} and teacher {teacherProfileId}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating request: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return false;
        }
    }

    public async Task<bool> UpdateStudentRequest(int requestId, RequestStatus status)
    {
        var request = await _unitOfWork.TeacherStudents.GetByIdAsync(requestId);
        if (request == null) return false;

        request.Status = status;
        await _unitOfWork.TeacherStudents.UpdateAsync(request);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
    
    public async Task<bool> HasStudentRequestAsync(int studentId, int teacherProfileId)
    {
        var requests = await _unitOfWork.TeacherStudents.GetAllAsync();
        return requests.Any(r => r.StudentId == studentId && 
                               r.TeacherProfileId == teacherProfileId && 
                               r.Status == RequestStatus.Pending);
    }

    public async Task<RequestStatus?> GetRequestStatusAsync(int studentId, int teacherProfileId)
    {
        var request = (await _unitOfWork.TeacherStudents.GetAllAsync())
            .FirstOrDefault(r => r.StudentId == studentId && r.TeacherProfileId == teacherProfileId);
        return request?.Status;
    }
    
    public async Task<List<TeacherProfile>> GetAcceptedTeachersAsync(int studentId)
    {
        var requests = await _unitOfWork.TeacherStudents.GetAllWithIncludesAsync(
            r => r.TeacherProfile.User,
            r => r.TeacherProfile.TeacherSubjects
        );
    
        return requests
            .Where(r => r.StudentId == studentId && r.Status == RequestStatus.Accepted)
            .Select(r => r.TeacherProfile)
            .Distinct()
            .ToList();
    }
    
    // Метод для получения уроков с актуальным статусом
    public async Task<List<Lesson>> GetTeacherLessonsAsync(int teacherProfileId)
    {
        var lessons = await _unitOfWork.Lessons.GetAllWithIncludesAsync(
            l => l.Subject,
            l => l.Student
        );
        
        return lessons
            .Where(l => l.TeacherProfileId == teacherProfileId)
            .OrderBy(l => l.ScheduledStart)
            .ToList();
    }

    // Метод отмены урока
    public async Task<bool> CancelLessonAsync(int lessonId)
    {
        var lesson = await _unitOfWork.Lessons.GetByIdAsync(lessonId);
        if (lesson == null) return false;
    
        lesson.IsCancelled = true;
        await _unitOfWork.Lessons.UpdateAsync(lesson);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    // Метод создания урока
    public async Task<bool> CreateLessonAsync(int teacherProfileId, CreateLessonDto input)
    {
        try
        {
            // Конвертация времени в UTC если еще не сделано
            var startTimeUtc = DateTime.SpecifyKind(input.StartTime, DateTimeKind.Utc);
            var endTimeUtc = DateTime.SpecifyKind(input.EndTime, DateTimeKind.Utc);
            
            // Проверяем существование предмета
            var subject = await _unitOfWork.Subjects.GetByIdAsync(input.SubjectId);
            if (subject == null)
            {
                return false;
            }

            // Проверка времени
            if (startTimeUtc >= endTimeUtc)
            {
                return false;
            }

            int? studentId = null; // Изменяем тип на nullable int
            
            // Если StudentId указан явно, проверяем наличие соединения
            if (input.StudentId.HasValue)
            {
                // Проверяем, что у этого учителя есть соединение с указанным студентом
                var hasConnection = (await _unitOfWork.TeacherStudents
                    .FindAsync(ts => ts.TeacherProfileId == teacherProfileId && 
                                   ts.StudentId == input.StudentId.Value && 
                                   ts.Status == RequestStatus.Accepted))
                    .Any();
                    
                if (!hasConnection)
                {
                    // Если нет активного соединения с указанным студентом, возвращаем ошибку
                    return false;
                }
                
                studentId = input.StudentId.Value;
            }
            // Удаляем блок else, чтобы studentId оставался null, если студент не выбран
            
            // Создаем урок с DateTime значениями в UTC
            var lesson = new Lesson
            {
                TeacherProfileId = teacherProfileId,
                StudentId = studentId, // Теперь поле может быть null
                SubjectId = input.SubjectId,
                ScheduledStart = startTimeUtc,
                ScheduledEnd = endTimeUtc,
                VideoCallUrl = input.MeetingLink,
                WhiteboardUrl = input.WhiteboardLink,
                IsCancelled = false,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Lessons.AddAsync(lesson);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
} 