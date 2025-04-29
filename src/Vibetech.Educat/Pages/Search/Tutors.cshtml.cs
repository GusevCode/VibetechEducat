using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Vibetech.Educat.Common.Interfaces;
using Vibetech.Educat.Common.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Vibetech.Educat.Services.Services.TeacherService;
using System.ComponentModel.DataAnnotations;

namespace Vibetech.Educat.Pages.Search;

public sealed class TutorsModel : PageModel
{
    private readonly ITeacherService _teacherService;
    private readonly UserManager<User> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private const int PageSize = 10;

    [BindProperty(SupportsGet = true)]
    public required SearchInputModel Input { get; set; }

    [BindProperty(SupportsGet = true)]
    public string SortBy { get; set; } = "rating";

    [BindProperty(SupportsGet = true)]
    public new int Page { get; set; } = 1;

    public required List<TeacherViewModel> Teachers { get; set; }
    public int TotalPages { get; set; }
    public IEnumerable<Subject> AvailableSubjects { get; set; } = new List<Subject>();

    public TutorsModel(
        ITeacherService teacherService, 
        UserManager<User> userManager,
        IUnitOfWork unitOfWork)
    {
        _teacherService = teacherService ?? throw new ArgumentNullException(nameof(teacherService));
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        Teachers = new List<TeacherViewModel>();
    }

    public class SearchInputModel
    {
        public string? Subject { get; set; }
        
        [Range(0, double.MaxValue, ErrorMessage = "Цена не может быть отрицательной")]
        public decimal? MinPrice { get; set; }
        
        [Range(0, double.MaxValue, ErrorMessage = "Цена не может быть отрицательной")]
        public decimal? MaxPrice { get; set; }
        
        [Range(0, int.MaxValue, ErrorMessage = "Опыт не может быть отрицательным")]
        public int? MinExperience { get; set; }
    }

    public class TeacherViewModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string MiddleName { get; set; } = string.Empty;
        public string? PhotoBase64 { get; set; }
        public string ContactInformation { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string[] PreparationPrograms { get; set; } = Array.Empty<string>();
        public decimal HourlyRate { get; set; }
        public int ExperienceYears { get; set; }
        public string Education { get; set; } = string.Empty;
        public double Rating { get; set; }
        public int ReviewsCount { get; set; }
        public List<Subject> AvailableSubjects { get; set; } = new();
        public bool HasPendingRequest { get; set; }

        public RequestStatus? CurrentRequestStatus { get; set; }
    }

    public async Task OnGetAsync()
    {
        // Инициализируем Input, если он не был установлен через query string
        Input ??= new SearchInputModel();

        // Валидация цен: если минимальная цена больше максимальной, сбрасываем фильтры цены
        if (Input.MinPrice.HasValue && Input.MaxPrice.HasValue && Input.MinPrice > Input.MaxPrice)
        {
            ModelState.AddModelError("Input.MinPrice", "Минимальная цена должна быть меньше максимальной");
            ModelState.AddModelError("Input.MaxPrice", "Максимальная цена должна быть больше минимальной");
            // Сбрасываем некорректные значения
            var tempInput = new SearchInputModel
            {
                Subject = Input.Subject,
                MinExperience = Input.MinExperience
            };
            Input = tempInput;
        }

        // Получаем доступные предметы
        AvailableSubjects = await _teacherService.GetAvailableSubjectsAsync();

        // Получаем всех учителей с их предметами
        var teachersWithSubjects = await _unitOfWork.TeacherProfiles
            .GetAllWithIncludesAsync(
                t => t.TeacherSubjects
            );

        // Загружаем связанные предметы для каждого учителя
        foreach (var teacher in teachersWithSubjects)
        {
            var teacherSubjects = await _unitOfWork.TeacherSubjects
                .GetAllWithIncludesAsync(
                    ts => ts.Subject
                );
            teacher.TeacherSubjects = teacherSubjects.Where(ts => ts.TeacherProfileId == teacher.Id).ToList();
        }

        // Применяем фильтры
        var filteredTeachers = teachersWithSubjects.AsQueryable();

        // Фильтр по предмету
        if (!string.IsNullOrEmpty(Input.Subject))
        {
            filteredTeachers = filteredTeachers
                .Where(t => t.TeacherSubjects.Any(ts => ts.Subject.Name == Input.Subject));
        }

        // Фильтр по цене
        if (Input.MinPrice.HasValue)
        {
            filteredTeachers = filteredTeachers.Where(t => t.HourlyRate >= Input.MinPrice.Value);
        }
        if (Input.MaxPrice.HasValue)
        {
            filteredTeachers = filteredTeachers.Where(t => t.HourlyRate <= Input.MaxPrice.Value);
        }

        // Фильтр по опыту
        if (Input.MinExperience.HasValue)
        {
            filteredTeachers = filteredTeachers.Where(t => t.ExperienceYears >= Input.MinExperience.Value);
        }

        // Применяем сортировку
        filteredTeachers = SortBy switch
        {
            "price" => filteredTeachers.OrderBy(t => t.HourlyRate),
            "experience" => filteredTeachers.OrderByDescending(t => t.ExperienceYears),
            _ => filteredTeachers.OrderByDescending(t => t.Rating)
        };

        // Преобразуем в ViewModel
        var teacherViewModels = new List<TeacherViewModel>();
        foreach (var teacher in filteredTeachers)
        {
            teacherViewModels.Add(await MapToViewModel(teacher));
        }
        Teachers = teacherViewModels;

        // Проверяем наличие заявок для аутентифицированного пользователя
        if (User.Identity?.IsAuthenticated == true)
        {
            var studentId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? throw new InvalidOperationException("User ID not found"));
            
            foreach (var teacher in Teachers)
            {
                teacher.HasPendingRequest = await _teacherService.HasStudentRequestAsync(studentId, teacher.Id);
            }
        }

        // Рассчитываем общее количество страниц
        var totalTeachers = Teachers.Count;
        TotalPages = (int)Math.Ceiling(totalTeachers / (double)PageSize);

        // Применяем пагинацию
        Teachers = Teachers.Skip((Page - 1) * PageSize).Take(PageSize).ToList();
    }

    private async Task<TeacherViewModel> MapToViewModel(TeacherProfile teacher)
    {
        var user = await _userManager.FindByIdAsync(teacher.UserId.ToString());
        var hasPendingRequest = false;

        if (User.Identity?.IsAuthenticated == true)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null)
            {
                hasPendingRequest = await _teacherService.HasStudentRequestAsync(currentUser.Id, teacher.Id);
            }
        }

        // Получаем первый предмет учителя
        var firstSubject = teacher.TeacherSubjects.FirstOrDefault()?.Subject;
        var subjectName = firstSubject?.Name ?? string.Empty;

        RequestStatus? currentStatus = null;
        if (User.Identity?.IsAuthenticated == true)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null)
            {
                currentStatus = await _teacherService.GetRequestStatusAsync(currentUser.Id, teacher.Id);
            }
        }
        
        return new TeacherViewModel
        {
            Id = teacher.Id,
            FirstName = user?.FirstName ?? "Неизвестно",
            LastName = user?.LastName ?? "Неизвестно",
            MiddleName = user?.MiddleName ?? string.Empty,
            PhotoBase64 = user?.PhotoBase64,
            ContactInformation = user?.ContactInformation,
            Subject = subjectName,
            PreparationPrograms = teacher.PreparationPrograms,
            HourlyRate = teacher.HourlyRate,
            ExperienceYears = teacher.ExperienceYears,
            Education = teacher.Education,
            Rating = teacher.Rating,
            ReviewsCount = teacher.ReviewsCount,
            AvailableSubjects = teacher.TeacherSubjects.Select(ts => ts.Subject).ToList(),
            HasPendingRequest = hasPendingRequest,
            CurrentRequestStatus = currentStatus
        };
    }

    public async Task<IActionResult> OnPostRequest(int teacherProfileId)
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return RedirectToPage("/Account/Login");
        }

        try
        {
            var studentId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? throw new InvalidOperationException("User ID not found"));
            
            // Проверяем, не отправлена ли уже заявка
            if (await _teacherService.HasStudentRequestAsync(studentId, teacherProfileId))
            {
                TempData["ErrorMessage"] = "Заявка уже отправлена";
                return RedirectToPage();
            }

            // Создаем новую заявку
            var result = await _teacherService.CreateStudentRequestAsync(studentId, teacherProfileId);
            
            if (result)
            {
                TempData["SuccessMessage"] = "Заявка успешно отправлена";
            }
            else
            {
                
                TempData["ErrorMessage"] = "Не удалось создать заявку";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in Request handler: {ex.Message}");
            TempData["ErrorMessage"] = $"Ошибка при отправке заявки: {ex.Message}";
        }

        return RedirectToPage();
    }
} 