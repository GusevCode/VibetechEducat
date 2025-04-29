using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Vibetech.Educat.Common.Interfaces;
using Vibetech.Educat.Common.Models;

namespace Vibetech.Educat.Pages.Account;

[Authorize(Roles = "Student")]
public class StudentProfileModel : PageModel
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<StudentProfileModel> _logger;

    public StudentProfileModel(IUnitOfWork unitOfWork, ILogger<StudentProfileModel> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string PhotoUrl { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public int TotalLessonsCount { get; set; }
    public int CompletedLessonsCount { get; set; }
    public int UpcomingLessonsCount { get; set; }
    public int TeachersCount { get; set; }

    public List<TeacherProfile> Teachers { get; set; } = new();

    public class InputModel
    {
        [Required(ErrorMessage = "Поле Имя обязательно для заполнения")]
        [Display(Name = "Имя")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Поле Фамилия обязательно для заполнения")]
        [Display(Name = "Фамилия")]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Отчество")]
        public string? MiddleName { get; set; }

        [Required(ErrorMessage = "Поле Дата рождения обязательно для заполнения")]
        [Display(Name = "Дата рождения")]
        [DataType(DataType.Date)]
        public DateTime BirthDate { get; set; }

        [Required(ErrorMessage = "Поле Пол обязательно для заполнения")]
        [Display(Name = "Пол")]
        public string Gender { get; set; } = string.Empty;

        [Display(Name = "Телефон")]
        public string Phone { get; set; } = string.Empty;
        
        [Display(Name = "Контактная информация")]
        [StringLength(1000, ErrorMessage = "Максимальная длина — 1000 символов")]
        public string? ContactInformation { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            // Get current user
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId))
            {
                return RedirectToPage("/Account/Login");
            }

            // Get user data
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                return RedirectToPage("/Account/Login");
            }

            // Check for recently removed teacher relationships (last 24 hours)
            var recentRemovals = await CheckForRemovedTeacherRelationships(userId);
            if (recentRemovals.Any())
            {
                foreach (var teacherName in recentRemovals)
                {
                    TempData["InfoMessage"] = $"Репетитор {teacherName} удалил вас из списка своих учеников. Чтобы возобновить занятия, вам необходимо отправить новую заявку.";
                }
            }

            // Populate input model
            Input.FirstName = user.FirstName;
            Input.LastName = user.LastName;
            Input.MiddleName = user.MiddleName;
            Input.BirthDate = user.BirthDate;
            Input.Gender = user.Gender != null ? user.Gender : string.Empty;
            Input.Phone = user.PhoneNumber;
            Input.ContactInformation = user.ContactInformation;

            // Set view data
            PhotoUrl = !string.IsNullOrEmpty(user.PhotoBase64) 
                ? $"data:image/jpeg;base64,{user.PhotoBase64}" 
                : "/images/default-avatar.jpg";
                
            FullName = $"{user.LastName} {user.FirstName} {user.MiddleName}".Trim();
            
            // Get lessons
            var allLessons = await _unitOfWork.Lessons.GetAllWithIncludesAsync(
                l => l.Subject,
                l => l.TeacherProfile,
                l => l.TeacherProfile.User
            );
            
            var userLessons = allLessons.Where(l => l.StudentId == userId).ToList();
            TotalLessonsCount = userLessons.Count;
            CompletedLessonsCount = userLessons.Count(l => l.Status == LessonStatus.Completed);
            
            // Get upcoming lessons (scheduled and in progress)
            var upcomingLessons = userLessons
                .Where(l => l.Status == LessonStatus.Scheduled || l.Status == LessonStatus.InProgress)
                .OrderBy(l => l.ScheduledStart)
                .Take(5)
                .ToList();
                
            UpcomingLessonsCount = upcomingLessons.Count;
            
            // Get teachers
            var teacherStudents = await _unitOfWork.TeacherStudents.FindAsync(ts => 
                ts.StudentId == userId && 
                ts.Status == RequestStatus.Accepted);
                
            var teacherIds = teacherStudents.Select(ts => ts.TeacherProfileId).ToList();
            TeachersCount = teacherIds.Count;
            
            if (teacherIds.Any())
            {
                // Исправлено: используем правильный подход к загрузке вложенных коллекций
                // Сначала получаем все профили преподавателей с базовыми данными и пользователями
                var teacherProfiles = await _unitOfWork.TeacherProfiles.FindAsync(tp => teacherIds.Contains(tp.Id));
                var teacherProfilesList = teacherProfiles.ToList();
                
                // Если нужны предметы и другие данные преподавателей, их нужно загружать отдельно
                if (teacherProfilesList.Any())
                {
                    // Загружаем пользователей для профилей преподавателей
                    foreach (var profile in teacherProfilesList)
                    {
                        // Загружаем данные пользователя
                        var teacherUser = await _unitOfWork.Users.GetByIdAsync(profile.UserId);
                        if (teacherUser != null)
                        {
                            profile.User = teacherUser;
                        }
                        
                        // Загружаем предметы преподавателя
                        var teacherSubjects = await _unitOfWork.TeacherSubjects.FindAsync(ts => ts.TeacherProfileId == profile.Id);
                        var subjectIds = teacherSubjects.Select(ts => ts.SubjectId).ToList();
                        var subjects = await _unitOfWork.Subjects.FindAsync(s => subjectIds.Contains(s.Id));
                        
                        // Связываем субъекты с TeacherSubject объектами
                        foreach (var ts in teacherSubjects)
                        {
                            ts.Subject = subjects.FirstOrDefault(s => s.Id == ts.SubjectId);
                        }
                        
                        profile.TeacherSubjects = teacherSubjects.ToList();
                    }
                }
                
                Teachers = teacherProfilesList;
            }

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading student profile");
            ModelState.AddModelError(string.Empty, "Произошла ошибка при загрузке профиля. Пожалуйста, попробуйте еще раз.");
            return Page();
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Get current user
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId))
            {
                return RedirectToPage("/Account/Login");
            }

            // Update user data
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                return RedirectToPage("/Account/Login");
            }

            user.FirstName = Input.FirstName;
            user.LastName = Input.LastName;
            user.MiddleName = Input.MiddleName;
            
            // Ensure BirthDate is in UTC format for PostgreSQL
            if (Input.BirthDate.Kind != DateTimeKind.Utc)
            {
                user.BirthDate = DateTime.SpecifyKind(Input.BirthDate.Date, DateTimeKind.Utc);
            }
            else
            {
                user.BirthDate = Input.BirthDate;
            }
            
            user.Gender = Input.Gender;
            user.ContactInformation = Input.ContactInformation;
            user.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            TempData["SuccessMessage"] = "Личная информация успешно обновлена.";
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating student info");
            ModelState.AddModelError(string.Empty, "Произошла ошибка при обновлении информации. Пожалуйста, попробуйте еще раз.");
            return Page();
        }
    }

    public async Task<IActionResult> OnPostUpdatePhotoAsync(IFormFile photo)
    {
        try
        {
            if (photo == null || photo.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "Пожалуйста, выберите файл.");
                return Page();
            }

            if (photo.Length > 2 * 1024 * 1024) // 2MB limit
            {
                ModelState.AddModelError(string.Empty, "Размер файла не должен превышать 2MB.");
                return Page();
            }

            // Get current user
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId))
            {
                return RedirectToPage("/Account/Login");
            }

            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                return RedirectToPage("/Account/Login");
            }

            // Convert to Base64
            using var memoryStream = new MemoryStream();
            await photo.CopyToAsync(memoryStream);
            user.PhotoBase64 = Convert.ToBase64String(memoryStream.ToArray());
            user.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            TempData["SuccessMessage"] = "Фото профиля успешно обновлено.";
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating photo");
            ModelState.AddModelError(string.Empty, "Произошла ошибка при обновлении фото. Пожалуйста, попробуйте еще раз.");
            return Page();
        }
    }

    public async Task<IActionResult> OnPostRemoveTeacherAsync(int teacherId)
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return new JsonResult(new { success = false, message = "Пользователь не авторизован" });
        }

        try
        {
            // Проверяем, что текущий пользователь является учеником
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? throw new InvalidOperationException("User ID not found"));
            
            // Находим связь между учеником и репетитором
            var relations = await _unitOfWork.TeacherStudents.FindAsync(
                r => r.StudentId == userId && r.TeacherProfileId == teacherId);
                
            var relation = relations.FirstOrDefault();
            if (relation == null)
            {
                return new JsonResult(new { success = false, message = "Связь с репетитором не найдена" });
            }
            
            // Удаляем связь
            await _unitOfWork.TeacherStudents.DeleteAsync(relation.Id);
            await _unitOfWork.SaveChangesAsync();
            
            return new JsonResult(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении репетитора");
            return new JsonResult(new { success = false, message = $"Ошибка при удалении репетитора: {ex.Message}" });
        }
    }

    // Helper method to check for recently removed teacher relationships
    private async Task<List<string>> CheckForRemovedTeacherRelationships(int studentId)
    {
        var removedTeachers = new List<string>();
        
        try
        {
            // Get teacher-student relationships that were updated in the last 24 hours and are now in Rejected status
            var yesterday = DateTime.UtcNow.AddDays(-1);
            var recentRemovals = await _unitOfWork.TeacherStudents.FindAsync(
                ts => ts.StudentId == studentId && 
                      ts.Status == RequestStatus.Rejected &&
                      ts.UpdatedAt.HasValue && 
                      ts.UpdatedAt > yesterday);
                      
            // Get teacher names for display
            foreach (var removal in recentRemovals)
            {
                // Load teacher profile and user info
                var teacherProfile = await _unitOfWork.TeacherProfiles.GetByIdAsync(removal.TeacherProfileId);
                if (teacherProfile != null)
                {
                    var teacher = await _unitOfWork.Users.GetByIdAsync(teacherProfile.UserId);
                    if (teacher != null)
                    {
                        removedTeachers.Add($"{teacher.LastName} {teacher.FirstName}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking for removed teacher relationships");
        }
        
        return removedTeachers;
    }
} 