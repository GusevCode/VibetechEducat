using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using Vibetech.Educat.Common.Interfaces;
using Vibetech.Educat.Common.Models;
using Vibetech.Educat.Domain.Dto;
using Vibetech.Educat.Services.Services.TeacherService;

namespace Vibetech.Educat.Pages.Teacher;

[Authorize(Roles = "Teacher")]
public class CreateLessonModel : PageModel
{
    private readonly ITeacherService _teacherService;
    private readonly IUnitOfWork _unitOfWork;

    [BindProperty]
    public CreateLessonDto Input { get; set; } = new();

    public SelectList Subjects { get; set; } = new SelectList(new List<SelectListItem>());
    public List<SelectListItem> StudentItems { get; set; } = new List<SelectListItem>();
    
    public string? ErrorMessage { get; set; } = null;

    public CreateLessonModel(ITeacherService teacherService, IUnitOfWork unitOfWork)
    {
        _teacherService = teacherService;
        _unitOfWork = unitOfWork;
        
        // Initialize with default values in local time format
        var now = DateTime.Now;
        Input.StartTime = new DateTime(now.Year, now.Month, now.Day, now.Hour + 1, 0, 0, DateTimeKind.Local);
        Input.EndTime = Input.StartTime.AddHours(1);
    }

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            // Получение предметов
            var subjects = await _teacherService.GetAvailableSubjectsAsync();
            if (subjects != null && subjects.Any())
            {
                Subjects = new SelectList(subjects, "Id", "Name");
            }
            else
            {
                Subjects = new SelectList(new List<object>());
            }

            // Получение ID профиля учителя
            int teacherProfileId = await GetTeacherProfileIdAsync();
            if (teacherProfileId <= 0)
            {
                // Если не удалось получить teacherProfileId, ошибка уже записана в ErrorMessage
                return Page();
            }
            
            // Получение студентов, которые были приняты этим учителем
            await LoadStudentsAsync(teacherProfileId);
            
            return Page();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Произошла ошибка: {ex.Message}";
            return Page();
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) 
        {
            // Re-populate the select lists if validation fails
            try
            {
                var subjects = await _teacherService.GetAvailableSubjectsAsync();
                if (subjects != null && subjects.Any())
                {
                    Subjects = new SelectList(subjects, "Id", "Name");
                }
                else
                {
                    Subjects = new SelectList(new List<object>());
                }
                
                // Try to reload students if possible
                int teacherProfileId = await GetTeacherProfileIdAsync(false);
                if (teacherProfileId > 0)
                {
                    await LoadStudentsAsync(teacherProfileId);
                }
            }
            catch
            {
                // Fall back to empty lists if we can't load the data
                Subjects = new SelectList(new List<object>());
                StudentItems = new List<SelectListItem>();
            }
            
            return Page();
        }

        try
        {
            // Получение ID профиля учителя
            int teacherProfileId = await GetTeacherProfileIdAsync();
            if (teacherProfileId <= 0)
            {
                // Re-initialize select lists
                Subjects = new SelectList(new List<object>());
                StudentItems = new List<SelectListItem>();
                
                return Page();
            }
            
            // Directly convert DateTime values to UTC
            if (Input.StartTime.Kind != DateTimeKind.Utc)
            {
                Input.StartTime = DateTime.SpecifyKind(Input.StartTime, DateTimeKind.Local).ToUniversalTime();
            }

            if (Input.EndTime.Kind != DateTimeKind.Utc)
            {
                Input.EndTime = DateTime.SpecifyKind(Input.EndTime, DateTimeKind.Local).ToUniversalTime();
            }
            
            await _teacherService.CreateLessonAsync(teacherProfileId, Input);
            return RedirectToPage("/Lessons");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            
            // Re-initialize select lists in case of error
            try
            {
                var subjects = await _teacherService.GetAvailableSubjectsAsync();
                if (subjects != null && subjects.Any())
                {
                    Subjects = new SelectList(subjects, "Id", "Name");
                }
                else
                {
                    Subjects = new SelectList(new List<object>());
                }
                
                // Try to reload students if possible
                int teacherProfileId = await GetTeacherProfileIdAsync(false);
                if (teacherProfileId > 0)
                {
                    await LoadStudentsAsync(teacherProfileId);
                }
            }
            catch
            {
                // Fall back to empty lists if we can't load the data
                Subjects = new SelectList(new List<object>());
                StudentItems = new List<SelectListItem>();
            }
            
            return Page();
        }
    }
    
    // Helper method to get teacher profile ID
    private async Task<int> GetTeacherProfileIdAsync(bool setErrorMessage = true)
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
            if (setErrorMessage)
            {
                ErrorMessage = "Не удалось определить пользователя. Пожалуйста, войдите в систему заново.";
            }
            return 0;
        }
        
        var teacherProfiles = await _unitOfWork.TeacherProfiles.FindAsync(tp => tp.UserId == userId);
        var teacherProfile = teacherProfiles.FirstOrDefault();
        
        if (teacherProfile == null)
        {
            if (setErrorMessage)
            {
                ErrorMessage = "У вас еще нет профиля учителя. Необходимо создать профиль учителя.";
            }
            return 0;
        }
        
        return teacherProfile.Id;
    }
    
    // Helper method to load students
    private async Task LoadStudentsAsync(int teacherProfileId)
    {
        try
        {
            // Clear existing items
            StudentItems.Clear();
            
            // Get all accepted connections between teacher and students
            var teacherStudents = await _unitOfWork.TeacherStudents
                .FindAsync(ts => ts.TeacherProfileId == teacherProfileId && ts.Status == RequestStatus.Accepted);
                
            if (teacherStudents != null && teacherStudents.Any())
            {   
                // Get student details and add them to the items list
                foreach (var ts in teacherStudents)
                {
                    var student = await _unitOfWork.Users.GetByIdAsync(ts.StudentId);
                    if (student != null)
                    {
                        StudentItems.Add(new SelectListItem 
                        { 
                            Value = student.Id.ToString(), 
                            Text = $"{student.LastName} {student.FirstName}" 
                        });
                    }
                }
            }
            else
            {
                // No students, add just a placeholder
                StudentItems.Add(new SelectListItem 
                { 
                    Value = "", 
                    Text = "У вас пока нет принятых студентов" 
                });
            }
        }
        catch
        {
            // If there's any error, make sure we have at least an empty list with a placeholder
            StudentItems.Clear();
            StudentItems.Add(new SelectListItem { Value = "", Text = "Невозможно загрузить список студентов" });
        }
    }
}