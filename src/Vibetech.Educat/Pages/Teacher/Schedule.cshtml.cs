using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Text.Json;
using Vibetech.Educat.Common.Interfaces;
using Vibetech.Educat.Common.Models;
using Vibetech.Educat.Services.Services.TeacherService;

namespace Vibetech.Educat.Pages.Teacher;

[Authorize(Roles = "Teacher")]
[ValidateAntiForgeryToken]
public class ScheduleModel : PageModel
{
    private readonly ITeacherService _teacherService;
    private readonly IUnitOfWork _unitOfWork;

    public string? ErrorMessage { get; set; }

    public ScheduleModel(ITeacherService teacherService, IUnitOfWork unitOfWork)
    {
        _teacherService = teacherService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            // Проверяем доступ учителя
            int teacherProfileId = await GetTeacherProfileIdAsync();
            if (teacherProfileId <= 0)
            {
                // Сообщение об ошибке уже установлено в GetTeacherProfileIdAsync
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
            // Проверяем доступ учителя
            int teacherProfileId = await GetTeacherProfileIdAsync();
            if (teacherProfileId <= 0)
            {
                return new JsonResult(new { error = "Доступ запрещен" });
            }
            
            // Установим значения по умолчанию, если параметры не переданы
            var startDate = start ?? DateTime.UtcNow.AddMonths(-1);
            var endDate = end ?? DateTime.UtcNow.AddMonths(3);

            // Получаем уроки учителя за указанный период
            var lessons = await _unitOfWork.Lessons.FindAsync(l => 
                l.TeacherProfileId == teacherProfileId && 
                l.ScheduledStart < endDate && 
                l.ScheduledEnd > startDate);

            // Подгружаем связанные данные
            var lessonsWithIncludes = await _unitOfWork.Lessons.GetAllWithIncludesAsync(
                l => l.Subject,
                l => l.Student,
                l => l.Attachments
            );

            // Фильтруем только нужные уроки
            lessonsWithIncludes = lessonsWithIncludes
                .Where(l => l.TeacherProfileId == teacherProfileId && 
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
                studentName = l.Student != null 
                    ? $"{l.Student.LastName} {l.Student.FirstName}" 
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
            // Проверяем доступ учителя
            int teacherProfileId = await GetTeacherProfileIdAsync();
            if (teacherProfileId <= 0)
            {
                return new JsonResult(new { error = "Доступ запрещен" });
            }

            // Получаем урок с включениями
            var lessons = await _unitOfWork.Lessons.GetAllWithIncludesAsync(
                l => l.Subject,
                l => l.Student,
                l => l.Attachments
            );
            var lesson = lessons.FirstOrDefault(l => l.Id == id);

            if (lesson == null || lesson.TeacherProfileId != teacherProfileId)
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
                studentName = lesson.Student != null 
                    ? $"{lesson.Student.LastName} {lesson.Student.FirstName}" 
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