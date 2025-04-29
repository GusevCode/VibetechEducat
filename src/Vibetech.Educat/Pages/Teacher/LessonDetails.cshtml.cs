using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.IO;
using Vibetech.Educat.Common.Interfaces;
using Vibetech.Educat.Common.Models;
using Vibetech.Educat.Domain.Dto;
using Vibetech.Educat.Services.Services.TeacherService;

namespace Vibetech.Educat.Pages.Teacher;

[Authorize(Roles = "Teacher")]
public class LessonDetailsModel : PageModel
{
    private readonly ITeacherService _teacherService;
    private readonly IUnitOfWork _unitOfWork;

    public Lesson? Lesson { get; private set; }
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }
    
    // Properties for the edit form
    public List<Subject> Subjects { get; set; } = new();
    public List<User> Students { get; set; } = new();
    
    // Property for attachments
    public List<Attachment> Attachments { get; set; } = new();
    
    [BindProperty]
    public IFormFile? UploadedFile { get; set; }
    
    public LessonDetailsModel(ITeacherService teacherService, IUnitOfWork unitOfWork)
    {
        _teacherService = teacherService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        try
        {
            // Verify the teacher has access to this lesson
            int teacherProfileId = await GetTeacherProfileIdAsync();
            if (teacherProfileId <= 0)
            {
                // Error message is already set in GetTeacherProfileIdAsync
                return Page();
            }

            // Get the lesson with related entities
            var lessons = await _unitOfWork.Lessons.GetAllWithIncludesAsync(
                l => l.Subject,
                l => l.Student,
                l => l.TeacherProfile,
                l => l.Attachments
            );

            // Find the specific lesson
            Lesson = lessons.FirstOrDefault(l => l.Id == id && l.TeacherProfileId == teacherProfileId);

            if (Lesson == null)
            {
                ErrorMessage = "Урок не найден или у вас нет прав для его просмотра.";
                return Page();
            }
            
            // Load subjects and students for edit form
            await LoadSubjectsAndStudents(teacherProfileId);
            
            // Load attachments for this lesson
            Attachments = Lesson.Attachments.ToList();

            return Page();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Произошла ошибка при загрузке урока: {ex.Message}";
            return Page();
        }
    }
    
    public async Task<IActionResult> OnPostCancelLessonAsync(int lessonId, string cancellationReason)
    {
        try
        {
            int teacherProfileId = await GetTeacherProfileIdAsync();
            if (teacherProfileId <= 0)
            {
                return RedirectToPage(new { id = lessonId });
            }
            
            // Verify the lesson exists and belongs to this teacher
            var lessons = await _unitOfWork.Lessons.GetAllWithIncludesAsync(
                l => l.Subject,
                l => l.Student
            );
            
            var lesson = lessons.FirstOrDefault(l => l.Id == lessonId && l.TeacherProfileId == teacherProfileId);
            
            if (lesson == null)
            {
                ErrorMessage = "Урок не найден или у вас нет прав для его отмены.";
                return RedirectToPage(new { id = lessonId });
            }
            
            // Cancel the lesson
            bool success = await _teacherService.CancelLessonAsync(lessonId);
            
            if (success)
            {
                // Here you could also add logic to notify the student, save the cancellation reason, etc.
                SuccessMessage = "Урок успешно отменен.";
            }
            else
            {
                ErrorMessage = "Не удалось отменить урок. Пожалуйста, попробуйте еще раз.";
            }
            
            return RedirectToPage(new { id = lessonId });
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Произошла ошибка при отмене урока: {ex.Message}";
            return RedirectToPage(new { id = lessonId });
        }
    }
    
    public async Task<IActionResult> OnPostEditLessonAsync(
        int lessonId,
        int subjectId,
        int? studentId, 
        DateTime startTime, 
        DateTime endTime, 
        string? meetingLink, 
        string? whiteboardLink)
    {
        try
        {
            int teacherProfileId = await GetTeacherProfileIdAsync();
            if (teacherProfileId <= 0)
            {
                return RedirectToPage(new { id = lessonId });
            }
            
            // Verify the lesson exists and belongs to this teacher
            var lessons = await _unitOfWork.Lessons.GetAllWithIncludesAsync(
                l => l.Subject,
                l => l.Student
            );
            
            var lesson = lessons.FirstOrDefault(l => l.Id == lessonId && l.TeacherProfileId == teacherProfileId);
            
            if (lesson == null)
            {
                ErrorMessage = "Урок не найден или у вас нет прав для его редактирования.";
                return RedirectToPage(new { id = lessonId });
            }
            
            // Verify the subject exists
            var subject = await _unitOfWork.Subjects.GetByIdAsync(subjectId);
            if (subject == null)
            {
                ErrorMessage = "Выбранный предмет не найден.";
                return RedirectToPage(new { id = lessonId });
            }
            
            // Verify the student exists and is connected to this teacher (if provided)
            if (studentId.HasValue)
            {
                var student = await _unitOfWork.Users.GetByIdAsync(studentId.Value);
                if (student == null)
                {
                    ErrorMessage = "Выбранный студент не найден.";
                    return RedirectToPage(new { id = lessonId });
                }
                
                // Check if there's a connection between this teacher and the student
                var hasConnection = (await _unitOfWork.TeacherStudents
                    .FindAsync(ts => ts.TeacherProfileId == teacherProfileId && 
                               ts.StudentId == studentId.Value && 
                               ts.Status == RequestStatus.Accepted))
                    .Any();
                    
                if (!hasConnection)
                {
                    ErrorMessage = "У вас нет активного соединения с выбранным студентом.";
                    return RedirectToPage(new { id = lessonId });
                }
            }
            
            // Basic validation
            if (startTime >= endTime)
            {
                ErrorMessage = "Время окончания урока должно быть позже времени начала.";
                return RedirectToPage(new { id = lessonId });
            }
            
            // Convert local time to UTC
            // The form provides local time, but we need to store UTC
            if (startTime.Kind != DateTimeKind.Utc)
            {
                startTime = DateTime.SpecifyKind(startTime, DateTimeKind.Local).ToUniversalTime();
            }
            
            if (endTime.Kind != DateTimeKind.Utc)
            {
                endTime = DateTime.SpecifyKind(endTime, DateTimeKind.Local).ToUniversalTime();
            }
            
            // Update the lesson
            lesson.SubjectId = subjectId;
            lesson.StudentId = studentId ?? lesson.StudentId; // Keep existing student if not provided
            lesson.ScheduledStart = startTime;
            lesson.ScheduledEnd = endTime;
            lesson.VideoCallUrl = meetingLink;
            lesson.WhiteboardUrl = whiteboardLink;
            
            await _unitOfWork.Lessons.UpdateAsync(lesson);
            await _unitOfWork.SaveChangesAsync();
            
            SuccessMessage = "Урок успешно обновлен.";
            return RedirectToPage(new { id = lessonId });
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Произошла ошибка при обновлении урока: {ex.Message}";
            return RedirectToPage(new { id = lessonId });
        }
    }
    
    public async Task<IActionResult> OnPostUploadAttachmentAsync(int lessonId)
    {
        try
        {
            int teacherProfileId = await GetTeacherProfileIdAsync();
            if (teacherProfileId <= 0 || UploadedFile == null)
            {
                ErrorMessage = "Необходимо выбрать файл для загрузки.";
                return RedirectToPage(new { id = lessonId });
            }
            
            // Verify the lesson exists and belongs to this teacher
            var lesson = await _unitOfWork.Lessons.GetByIdAsync(lessonId);
            if (lesson == null || lesson.TeacherProfileId != teacherProfileId)
            {
                ErrorMessage = "Урок не найден или у вас нет прав для его редактирования.";
                return RedirectToPage(new { id = lessonId });
            }
            
            // Verify file size (max 50MB)
            if (UploadedFile.Length > 50 * 1024 * 1024)
            {
                ErrorMessage = "Размер файла превышает допустимый лимит в 50 МБ.";
                return RedirectToPage(new { id = lessonId });
            }
            
            // Get user ID
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (userId == 0)
            {
                ErrorMessage = "Не удалось определить пользователя.";
                return RedirectToPage(new { id = lessonId });
            }
            
            // Convert file to base64
            using var memoryStream = new MemoryStream();
            await UploadedFile.CopyToAsync(memoryStream);
            var fileBytes = memoryStream.ToArray();
            var base64String = Convert.ToBase64String(fileBytes);
            
            // Create attachment
            var attachment = new Attachment
            {
                LessonId = lessonId,
                UploadedById = userId,
                FileName = UploadedFile.FileName,
                ContentType = UploadedFile.ContentType,
                FileBase64 = base64String,
                CreatedAt = DateTime.UtcNow
            };
            
            await _unitOfWork.Attachments.AddAsync(attachment);
            await _unitOfWork.SaveChangesAsync();
            
            SuccessMessage = "Файл успешно загружен.";
            return RedirectToPage(new { id = lessonId });
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Произошла ошибка при загрузке файла: {ex.Message}";
            return RedirectToPage(new { id = lessonId });
        }
    }
    
    public async Task<IActionResult> OnGetDownloadAsync(int id, int attachmentId)
    {
        try
        {
            int teacherProfileId = await GetTeacherProfileIdAsync();
            if (teacherProfileId <= 0)
            {
                ErrorMessage = "Доступ запрещен.";
                return RedirectToPage(new { id });
            }
            
            // Verify the lesson exists and belongs to this teacher
            var lesson = await _unitOfWork.Lessons.GetByIdAsync(id);
            if (lesson == null || lesson.TeacherProfileId != teacherProfileId)
            {
                ErrorMessage = "Урок не найден или у вас нет прав для доступа к нему.";
                return RedirectToPage(new { id });
            }
            
            // Get the attachment
            var attachment = await _unitOfWork.Attachments.GetByIdAsync(attachmentId);
            if (attachment == null || attachment.LessonId != id)
            {
                ErrorMessage = "Файл не найден или не относится к этому уроку.";
                return RedirectToPage(new { id });
            }
            
            // Convert base64 to file
            var fileBytes = Convert.FromBase64String(attachment.FileBase64);
            
            // Return the file
            return File(fileBytes, attachment.ContentType, attachment.FileName);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Произошла ошибка при загрузке файла: {ex.Message}";
            return RedirectToPage(new { id });
        }
    }
    
    public async Task<IActionResult> OnPostDeleteAttachmentAsync(int lessonId, int attachmentId)
    {
        try
        {
            int teacherProfileId = await GetTeacherProfileIdAsync();
            if (teacherProfileId <= 0)
            {
                ErrorMessage = "Доступ запрещен.";
                return RedirectToPage(new { id = lessonId });
            }
            
            // Verify the lesson exists and belongs to this teacher
            var lesson = await _unitOfWork.Lessons.GetByIdAsync(lessonId);
            if (lesson == null || lesson.TeacherProfileId != teacherProfileId)
            {
                ErrorMessage = "Урок не найден или у вас нет прав для доступа к нему.";
                return RedirectToPage(new { id = lessonId });
            }
            
            // Get the attachment
            var attachment = await _unitOfWork.Attachments.GetByIdAsync(attachmentId);
            if (attachment == null || attachment.LessonId != lessonId)
            {
                ErrorMessage = "Файл не найден или не относится к этому уроку.";
                return RedirectToPage(new { id = lessonId });
            }
            
            // Delete the attachment
            await _unitOfWork.Attachments.DeleteAsync(attachmentId);
            await _unitOfWork.SaveChangesAsync();
            
            SuccessMessage = "Файл успешно удален.";
            return RedirectToPage(new { id = lessonId });
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Произошла ошибка при удалении файла: {ex.Message}";
            return RedirectToPage(new { id = lessonId });
        }
    }
    
    // Helper method to load subjects and students
    private async Task LoadSubjectsAndStudents(int teacherProfileId)
    {
        try
        {
            // Load subjects
            Subjects = await _teacherService.GetAvailableSubjectsAsync();
            
            // Load students connected to this teacher
            var teacherStudents = await _unitOfWork.TeacherStudents
                .FindAsync(ts => ts.TeacherProfileId == teacherProfileId && ts.Status == RequestStatus.Accepted);
                
            if (teacherStudents != null && teacherStudents.Any())
            {
                var studentIds = teacherStudents.Select(ts => ts.StudentId).ToList();
                var students = await _unitOfWork.Users.FindAsync(u => studentIds.Contains(u.Id));
                Students = students.ToList();
            }
        }
        catch
        {
            // Fallback to empty lists if loading fails
            Subjects = new List<Subject>();
            Students = new List<User>();
        }
    }

    // Same helper method for getting teacher profile ID
    private async Task<int> GetTeacherProfileIdAsync()
    {
        // Получение ID профиля учителя
        var teacherProfileIdClaim = User.FindFirst("TeacherProfileId");
        if (teacherProfileIdClaim != null && int.TryParse(teacherProfileIdClaim.Value, out var id) && id > 0)
        {
            return id;
        }
        
        // Пытаемся найти профиль учителя по ID пользователя
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId) || userId == 0)
        {
            ErrorMessage = "Не удалось определить пользователя. Пожалуйста, войдите в систему заново.";
            return 0;
        }
        
        var teacherProfiles = await _unitOfWork.TeacherProfiles.FindAsync(tp => tp.UserId == userId);
        var teacherProfile = teacherProfiles.FirstOrDefault();
        
        if (teacherProfile == null)
        {
            ErrorMessage = "У вас еще нет профиля учителя. Необходимо создать профиль учителя.";
            return 0;
        }
        
        return teacherProfile.Id;
    }
} 