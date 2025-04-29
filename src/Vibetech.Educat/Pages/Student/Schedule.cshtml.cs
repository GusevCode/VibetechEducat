using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using Vibetech.Educat.Common.Interfaces;
using Vibetech.Educat.Common.Models;

namespace Vibetech.Educat.Pages.Student;

[Authorize(Roles = "Student")]
[ValidateAntiForgeryToken]
public class ScheduleModel : PageModel
{
    private readonly IUnitOfWork _unitOfWork;

    public string? ErrorMessage { get; set; }

    public ScheduleModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            // Проверяем доступ студента
            int studentId = await GetStudentIdAsync();
            if (studentId <= 0)
            {
                // Сообщение об ошибке уже установлено в GetStudentIdAsync
                return Page();
            }

            return Page();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Произошла ошибка при загрузке расписания: {ex.Message}";
            return Page();
        }
    }

    public async Task<IActionResult> OnGetEventsAsync(DateTime? start = null, DateTime? end = null)
    {
        try
        {
            // Проверяем доступ студента
            int studentId = await GetStudentIdAsync();
            if (studentId <= 0)
            {
                return new JsonResult(new { error = "Доступ запрещен" });
            }
            
            // Установим значения по умолчанию, если параметры не переданы
            var startDate = start ?? DateTime.UtcNow.AddMonths(-1);
            var endDate = end ?? DateTime.UtcNow.AddMonths(3);

            // Получаем уроки студента за указанный период
            var lessons = await _unitOfWork.Lessons.FindAsync(l => 
                l.StudentId == studentId && 
                l.ScheduledStart < endDate && 
                l.ScheduledEnd > startDate);

            // Подгружаем связанные данные
            var lessonsWithIncludes = await _unitOfWork.Lessons.GetAllWithIncludesAsync(
                l => l.Subject,
                l => l.TeacherProfile,
                l => l.Attachments
            );

            // Фильтруем только нужные уроки
            lessonsWithIncludes = lessonsWithIncludes
                .Where(l => l.StudentId == studentId && 
                       l.ScheduledStart < endDate && 
                       l.ScheduledEnd > startDate)
                .ToList();

            // Преобразуем уроки в формат для календаря
            var events = lessonsWithIncludes.Select(l => new
            {
                id = l.Id,
                title = l.Subject?.Name ?? "Предмет не указан",
                start = l.ScheduledStart,
                end = l.ScheduledEnd,
                status = l.Status.ToString(),
                teacherName = l.TeacherProfile?.User != null 
                    ? $"{l.TeacherProfile.User.LastName} {l.TeacherProfile.User.FirstName}" 
                    : null,
                attachmentsCount = l.Attachments?.Count ?? 0
            });

            return new JsonResult(events);
        }
        catch (Exception ex)
        {
            return new JsonResult(new { error = ex.Message });
        }
    }

    public async Task<IActionResult> OnGetLessonSummaryAsync(int id)
    {
        try
        {
            // Проверяем доступ студента
            int studentId = await GetStudentIdAsync();
            if (studentId <= 0)
            {
                return new JsonResult(new { error = "Доступ запрещен" });
            }

            // Получаем урок с включениями
            var lessons = await _unitOfWork.Lessons.GetAllWithIncludesAsync(
                l => l.Subject,
                l => l.TeacherProfile,
                l => l.TeacherProfile.User,
                l => l.Attachments
            );
            var lesson = lessons.FirstOrDefault(l => l.Id == id && l.StudentId == studentId);

            if (lesson == null)
            {
                return new JsonResult(new { error = "Урок не найден или у вас нет прав для его просмотра" });
            }

            // Возвращаем краткую информацию об уроке
            var summary = new
            {
                id = lesson.Id,
                subjectName = lesson.Subject?.Name ?? "Предмет не указан",
                scheduledStart = lesson.ScheduledStart,
                scheduledEnd = lesson.ScheduledEnd,
                status = lesson.Status.ToString(),
                teacherName = lesson.TeacherProfile?.User != null 
                    ? $"{lesson.TeacherProfile.User.LastName} {lesson.TeacherProfile.User.FirstName}" 
                    : null,
                attachmentsCount = lesson.Attachments?.Count ?? 0
            };

            return new JsonResult(summary);
        }
        catch (Exception ex)
        {
            return new JsonResult(new { error = ex.Message });
        }
    }

    private async Task<int> GetStudentIdAsync()
    {
        // Получение ID студента
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId) || userId == 0)
        {
            ErrorMessage = "Не удалось определить пользователя. Пожалуйста, войдите в систему заново.";
            return 0;
        }
        
        return userId;
    }
} 