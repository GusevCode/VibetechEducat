using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Vibetech.Educat.Common.Interfaces;
using Vibetech.Educat.Common.Models;
using System.Security.Claims;

namespace Vibetech.Educat.Pages.Teacher;

public class DashboardModel : PageModel
{
    private readonly IUnitOfWork _unitOfWork;
    public List<Lesson> UpcomingLessons { get; set; } = new();
    public List<TeacherStudent> PendingRequests { get; set; } = new();
    public List<TeacherStudent> AcceptedStudents { get; set; } = new();
    public string ActiveTab { get; set; } = "requests";

    public DashboardModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task OnGetAsync(string? tab = "requests")
    {
        ActiveTab = tab ?? "requests";

        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return;
        }

        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? throw new InvalidOperationException("User ID not found"));

        var teacherProfile = await _unitOfWork.TeacherProfiles
            .GetAllWithIncludesAsync(t => t.User)
            .ContinueWith(t => t.Result.FirstOrDefault(tp => tp.UserId == userId));

        if (teacherProfile == null)
        {
            return;
        }

        if (ActiveTab == "requests")
        {
            PendingRequests = (await _unitOfWork.TeacherStudents
                .GetAllWithIncludesAsync(
                    r => r.Student,
                    r => r.TeacherProfile
                ))
                .Where(r => r.TeacherProfileId == teacherProfile.Id && r.Status == RequestStatus.Pending)
                .ToList();
        }
        else if (ActiveTab == "students")
        {
            AcceptedStudents = (await _unitOfWork.TeacherStudents
                .GetAllWithIncludesAsync(
                    r => r.Student,
                    r => r.TeacherProfile
                ))
                .Where(r => r.TeacherProfileId == teacherProfile.Id && r.Status == RequestStatus.Accepted)
                .OrderByDescending(r => r.UpdatedAt)
                .ToList();
        }
    }

    public async Task<IActionResult> OnPostAcceptRequestAsync(int requestId)
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return new JsonResult(new { success = false, message = "Пользователь не авторизован" });
        }

        try
        {
            var request = await _unitOfWork.TeacherStudents.GetByIdAsync(requestId);
            if (request == null)
            {
                return new JsonResult(new { success = false, message = "Заявка не найдена" });
            }

            request.Status = RequestStatus.Accepted;
            request.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.TeacherStudents.UpdateAsync(request);
            await _unitOfWork.SaveChangesAsync();

            return new JsonResult(new { success = true });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, message = $"Ошибка при принятии заявки: {ex.Message}" });
        }
    }

    public async Task<IActionResult> OnPostRejectRequestAsync(int requestId)
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return new JsonResult(new { success = false, message = "Пользователь не авторизован" });
        }

        try
        {
            var request = await _unitOfWork.TeacherStudents.GetByIdAsync(requestId);
            if (request == null)
            {
                return new JsonResult(new { success = false, message = "Заявка не найдена" });
            }

            request.Status = RequestStatus.Rejected;
            request.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.TeacherStudents.UpdateAsync(request);
            await _unitOfWork.SaveChangesAsync();

            return new JsonResult(new { success = true });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, message = $"Ошибка при отклонении заявки: {ex.Message}" });
        }
    }
    
    public async Task<IActionResult> OnPostRemoveStudentAsync(int studentRelationId)
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return new JsonResult(new { success = false, message = "Пользователь не авторизован" });
        }

        try
        {
            // Проверяем, что текущий пользователь является владельцем этой связи
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? throw new InvalidOperationException("User ID not found"));
            
            var teacherProfile = await _unitOfWork.TeacherProfiles.FindAsync(tp => tp.UserId == userId);
            if (!teacherProfile.Any())
            {
                return new JsonResult(new { success = false, message = "Профиль репетитора не найден" });
            }
            
            var relation = await _unitOfWork.TeacherStudents.GetByIdAsync(studentRelationId);
            if (relation == null)
            {
                return new JsonResult(new { success = false, message = "Связь с учеником не найдена" });
            }
            
            if (relation.TeacherProfileId != teacherProfile.First().Id)
            {
                return new JsonResult(new { success = false, message = "У вас нет прав для удаления этого ученика" });
            }
            
            // Вместо удаления меняем статус на отклонено и обновляем дату
            relation.Status = RequestStatus.Rejected;
            relation.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.TeacherStudents.UpdateAsync(relation);
            await _unitOfWork.SaveChangesAsync();
            
            return new JsonResult(new { success = true });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, message = $"Ошибка при удалении ученика: {ex.Message}" });
        }
    }
} 