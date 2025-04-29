using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.Json;
using Vibetech.Educat.Common.Interfaces;
using Vibetech.Educat.Common.Models;

namespace Vibetech.Educat.Pages.Account;

[Authorize(Roles = "Teacher")]
public class TeacherProfileModel : PageModel
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TeacherProfileModel> _logger;

    public TeacherProfileModel(IUnitOfWork unitOfWork, ILogger<TeacherProfileModel> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string PhotoUrl { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public double Rating { get; set; } = 0;
    public int ReviewsCount { get; set; } = 0;
    public int StudentsCount { get; set; } = 0;
    public int CompletedLessonsCount { get; set; } = 0;
    public List<Subject> Subjects { get; set; } = new();
    public List<Subject> AvailableSubjects { get; set; } = new();

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

        [Required(ErrorMessage = "Поле Образование обязательно для заполнения")]
        [Display(Name = "Образование")]
        public string Education { get; set; } = string.Empty;

        [Required(ErrorMessage = "Поле Опыт преподавания обязательно для заполнения")]
        [Display(Name = "Опыт преподавания (лет)")]
        [Range(0, 50, ErrorMessage = "Опыт преподавания должен быть от 0 до 50 лет")]
        public int ExperienceYears { get; set; }

        [Required(ErrorMessage = "Поле Стоимость занятия обязательно для заполнения")]
        [Display(Name = "Стоимость занятия (руб/час)")]
        [Range(0, 10000, ErrorMessage = "Стоимость должна быть от 0 до 10000 рублей")]
        public decimal HourlyRate { get; set; }

        [Display(Name = "Программы подготовки")]
        public string[] PreparationPrograms { get; set; } = Array.Empty<string>();
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

            // Get teacher profile
            var teacherProfiles = await _unitOfWork.TeacherProfiles.FindAsync(tp => tp.UserId == userId);
            var teacherProfile = teacherProfiles.FirstOrDefault();
            
            if (teacherProfile == null)
            {
                // Create a new teacher profile if one doesn't exist
                teacherProfile = new TeacherProfile
                {
                    UserId = userId,
                    Education = "Укажите ваше образование",
                    ExperienceYears = 0,
                    HourlyRate = 0,
                    IsModerated = false,
                    PreparationPrograms = Array.Empty<string>()
                };
                
                await _unitOfWork.TeacherProfiles.AddAsync(teacherProfile);
                await _unitOfWork.SaveChangesAsync();
            }

            // Populate input model
            Input = new InputModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                MiddleName = user.MiddleName,
                BirthDate = user.BirthDate,
                Gender = user.Gender,
                Education = teacherProfile.Education,
                ExperienceYears = teacherProfile.ExperienceYears,
                HourlyRate = teacherProfile.HourlyRate,
                PreparationPrograms = teacherProfile.PreparationPrograms
            };

            // Set view data
            PhotoUrl = !string.IsNullOrEmpty(user.PhotoBase64) 
                ? $"data:image/jpeg;base64,{user.PhotoBase64}" 
                : "/images/default-avatar.jpg";
                
            FullName = $"{user.LastName} {user.FirstName} {user.MiddleName}".Trim();
            Rating = teacherProfile.Rating;
            ReviewsCount = teacherProfile.ReviewsCount;
            
            // Get students count
            var teacherStudents = await _unitOfWork.TeacherStudents.FindAsync(ts => 
                ts.TeacherProfileId == teacherProfile.Id && 
                ts.Status == RequestStatus.Accepted);
            StudentsCount = teacherStudents.Count();
            
            // Get completed lessons count - Fixed to avoid EF translation issue
            var lessons = await _unitOfWork.Lessons.FindAsync(l => 
                l.TeacherProfileId == teacherProfile.Id);
            // Process the status filtering in memory to avoid the EF Core translation issue
            CompletedLessonsCount = lessons.Count(l => l.Status == LessonStatus.Completed);
            
            // Get teacher's subjects
            var teacherSubjects = await _unitOfWork.TeacherSubjects.GetAllWithIncludesAsync(
                ts => ts.Subject
            );
            Subjects = teacherSubjects
                .Where(ts => ts.TeacherProfileId == teacherProfile.Id)
                .Select(ts => ts.Subject)
                .ToList();
            
            // Get all available subjects
            var allSubjects = await _unitOfWork.Subjects.GetAllAsync();
            AvailableSubjects = allSubjects.Except(Subjects).ToList();

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading teacher profile");
            ModelState.AddModelError(string.Empty, "Произошла ошибка при загрузке профиля. Пожалуйста, попробуйте еще раз.");
            return Page();
        }
    }

    public async Task<IActionResult> OnPostUpdateMainInfoAsync()
    {
        try
        {
            if (!ModelState.IsValid)
            {
                // Загружаем данные для отображения страницы
                await InitializePageDataAsync();
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
            user.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            TempData["SuccessMessage"] = "Основная информация успешно обновлена.";
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating main info");
            ModelState.AddModelError(string.Empty, "Произошла ошибка при обновлении основной информации. Пожалуйста, попробуйте еще раз.");
            // Загружаем данные для отображения страницы
            await InitializePageDataAsync();
            return Page();
        }
    }

    public async Task<IActionResult> OnPostUpdateProfInfoAsync()
    {
        try
        {
            if (!ModelState.IsValid)
            {
                // Загружаем данные для отображения страницы
                await InitializePageDataAsync();
                return Page();
            }

            // Get current user
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId))
            {
                return RedirectToPage("/Account/Login");
            }

            // Log the input values for debugging
            _logger.LogInformation("Teacher profile update - Input values:");
            _logger.LogInformation($"Education: {Input.Education}");
            _logger.LogInformation($"ExperienceYears: {Input.ExperienceYears}");
            _logger.LogInformation($"HourlyRate: {Input.HourlyRate}");
            
            // Log preparation programs
            if (Input.PreparationPrograms != null)
            {
                _logger.LogInformation($"PreparationPrograms count: {Input.PreparationPrograms.Length}");
                foreach (var program in Input.PreparationPrograms)
                {
                    _logger.LogInformation($"Program: {program}");
                }
            }
            else
            {
                _logger.LogInformation("PreparationPrograms is null");
            }

            // Get user data
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                return RedirectToPage("/Account/Login");
            }
            
            // Update user data (from Input)
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
            user.UpdatedAt = DateTime.UtcNow;
            
            await _unitOfWork.Users.UpdateAsync(user);
            
            // Get teacher profile
            var teacherProfiles = await _unitOfWork.TeacherProfiles.FindAsync(tp => tp.UserId == userId);
            var teacherProfile = teacherProfiles.FirstOrDefault();
            
            if (teacherProfile == null)
            {
                ModelState.AddModelError(string.Empty, "Профиль учителя не найден.");
                // Загружаем данные для отображения страницы
                await InitializePageDataAsync();
                return Page();
            }

            // Update teacher profile
            teacherProfile.Education = Input.Education;
            teacherProfile.ExperienceYears = Input.ExperienceYears;
            teacherProfile.HourlyRate = Input.HourlyRate;
            
            // Ensure preparation programs are properly handled
            teacherProfile.PreparationPrograms = Input.PreparationPrograms?.Where(p => !string.IsNullOrEmpty(p)).ToArray() ?? Array.Empty<string>();
            
            teacherProfile.IsModerated = false; // Require re-moderation after changes

            await _unitOfWork.TeacherProfiles.UpdateAsync(teacherProfile);
            
            // Save all changes in one transaction
            await _unitOfWork.SaveChangesAsync();

            // Log the saved values for debugging
            _logger.LogInformation("Teacher profile updated successfully:");
            _logger.LogInformation($"Saved PreparationPrograms count: {teacherProfile.PreparationPrograms.Length}");

            TempData["SuccessMessage"] = "Информация успешно обновлена.";
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile info");
            ModelState.AddModelError(string.Empty, "Произошла ошибка при обновлении информации. Пожалуйста, попробуйте еще раз.");
            // Загружаем данные для отображения страницы
            await InitializePageDataAsync();
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
                // Загружаем данные для отображения страницы
                await InitializePageDataAsync();
                return Page();
            }

            if (photo.Length > 2 * 1024 * 1024) // 2MB limit
            {
                ModelState.AddModelError(string.Empty, "Размер файла не должен превышать 2MB.");
                // Загружаем данные для отображения страницы
                await InitializePageDataAsync();
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
            // Загружаем данные для отображения страницы
            await InitializePageDataAsync();
            return Page();
        }
    }

    public async Task<IActionResult> OnPostAddSubjectAsync([FromBody] AddSubjectRequest request)
    {
        try
        {
            if (request?.SubjectId == null || request.SubjectId <= 0)
            {
                return new JsonResult(new { success = false, message = "Неверный идентификатор предмета." });
            }

            // Get current user
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId))
            {
                return new JsonResult(new { success = false, message = "Пользователь не найден." });
            }

            // Get teacher profile
            var teacherProfiles = await _unitOfWork.TeacherProfiles.FindAsync(tp => tp.UserId == userId);
            var teacherProfile = teacherProfiles.FirstOrDefault();
            
            if (teacherProfile == null)
            {
                return new JsonResult(new { success = false, message = "Профиль учителя не найден." });
            }

            // Check if subject exists
            var subject = await _unitOfWork.Subjects.GetByIdAsync(request.SubjectId);
            if (subject == null)
            {
                return new JsonResult(new { success = false, message = "Предмет не найден." });
            }

            // Check if teacher already has this subject
            var existingTeacherSubjects = await _unitOfWork.TeacherSubjects.FindAsync(ts => 
                ts.TeacherProfileId == teacherProfile.Id && 
                ts.SubjectId == request.SubjectId);
                
            if (existingTeacherSubjects.Any())
            {
                return new JsonResult(new { success = false, message = "Этот предмет уже добавлен." });
            }

            // Add subject to teacher
            var teacherSubject = new TeacherSubject
            {
                TeacherProfileId = teacherProfile.Id,
                SubjectId = request.SubjectId
            };

            await _unitOfWork.TeacherSubjects.AddAsync(teacherSubject);
            await _unitOfWork.SaveChangesAsync();

            return new JsonResult(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding subject");
            return new JsonResult(new { success = false, message = "Произошла ошибка при добавлении предмета." });
        }
    }

    public async Task<IActionResult> OnPostDeleteSubjectAsync([FromBody] DeleteSubjectRequest request)
    {
        try
        {
            if (request?.SubjectId == null || request.SubjectId <= 0)
            {
                return new JsonResult(new { success = false, message = "Неверный идентификатор предмета." });
            }

            // Get current user
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId))
            {
                return new JsonResult(new { success = false, message = "Пользователь не найден." });
            }

            // Get teacher profile
            var teacherProfiles = await _unitOfWork.TeacherProfiles.FindAsync(tp => tp.UserId == userId);
            var teacherProfile = teacherProfiles.FirstOrDefault();
            
            if (teacherProfile == null)
            {
                return new JsonResult(new { success = false, message = "Профиль учителя не найден." });
            }

            // Check if subject exists
            var subject = await _unitOfWork.Subjects.GetByIdAsync(request.SubjectId);
            if (subject == null)
            {
                return new JsonResult(new { success = false, message = "Предмет не найден." });
            }

            // Check if teacher has this subject
            var existingTeacherSubjects = await _unitOfWork.TeacherSubjects.FindAsync(ts => 
                ts.TeacherProfileId == teacherProfile.Id && 
                ts.SubjectId == request.SubjectId);
                
            if (!existingTeacherSubjects.Any())
            {
                return new JsonResult(new { success = false, message = "Этот предмет не добавлен к вашему профилю." });
            }

            // Delete subject from teacher profile
            foreach (var teacherSubject in existingTeacherSubjects)
            {
                _unitOfWork.TeacherSubjects.Remove(teacherSubject);
            }
            
            await _unitOfWork.SaveChangesAsync();
            
            return new JsonResult(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting subject");
            return new JsonResult(new { success = false, message = "Произошла ошибка при удалении предмета." });
        }
    }

    public class AddSubjectRequest
    {
        public int SubjectId { get; set; }
    }

    public class DeleteSubjectRequest
    {
        public int SubjectId { get; set; }
    }

    // Метод для инициализации данных страницы
    private async Task InitializePageDataAsync()
    {
        try
        {
            // Get current user
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId))
            {
                return;
            }

            // Get user data
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                return;
            }

            // Get teacher profile
            var teacherProfiles = await _unitOfWork.TeacherProfiles.FindAsync(tp => tp.UserId == userId);
            var teacherProfile = teacherProfiles.FirstOrDefault();
            
            if (teacherProfile == null)
            {
                return;
            }

            // Set view data
            PhotoUrl = !string.IsNullOrEmpty(user.PhotoBase64) 
                ? $"data:image/jpeg;base64,{user.PhotoBase64}" 
                : "/images/default-avatar.jpg";
                
            FullName = $"{user.LastName} {user.FirstName} {user.MiddleName}".Trim();
            Rating = teacherProfile.Rating;
            ReviewsCount = teacherProfile.ReviewsCount;
            
            // Get students count
            var teacherStudents = await _unitOfWork.TeacherStudents.FindAsync(ts => 
                ts.TeacherProfileId == teacherProfile.Id && 
                ts.Status == RequestStatus.Accepted);
            StudentsCount = teacherStudents.Count();
            
            // Get completed lessons count
            var lessons = await _unitOfWork.Lessons.FindAsync(l => 
                l.TeacherProfileId == teacherProfile.Id);
            CompletedLessonsCount = lessons.Count(l => l.Status == LessonStatus.Completed);
            
            // Get teacher's subjects
            var teacherSubjects = await _unitOfWork.TeacherSubjects.GetAllWithIncludesAsync(
                ts => ts.Subject
            );
            Subjects = teacherSubjects
                .Where(ts => ts.TeacherProfileId == teacherProfile.Id)
                .Select(ts => ts.Subject)
                .ToList();
            
            // Get all available subjects
            var allSubjects = await _unitOfWork.Subjects.GetAllAsync();
            AvailableSubjects = allSubjects.Except(Subjects).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing page data");
        }
    }
} 