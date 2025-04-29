using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Vibetech.Educat.Domain.Services;
using Vibetech.Educat.Common.Models;
using Microsoft.Extensions.Logging;
using Vibetech.Educat.Common.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Vibetech.Educat.Services.Services.TeacherService;

namespace Vibetech.Educat.Pages.Account;

public sealed class RegisterModel : PageModel
{
    private readonly IAuthService _authService;
    private readonly IFileStorageService _storageService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RegisterModel> _logger;
    private readonly ITeacherService _teacherService;

    public RegisterModel(
        IAuthService authService, 
        IFileStorageService storageService, 
        IUnitOfWork unitOfWork,
        ILogger<RegisterModel> logger,
        ITeacherService teacherService)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _teacherService = teacherService ?? throw new ArgumentNullException(nameof(teacherService));
        Input = new InputModel();
        ErrorMessage = string.Empty;
    }

    [BindProperty]
    public InputModel Input { get; set; }

    public string ErrorMessage { get; set; }

    public List<SelectListItem> AvailableSubjects { get; set; } = new();
    public List<SelectListItem> AvailablePrograms { get; set; } = new()
    {
        new SelectListItem { Value = "ОГЭ", Text = "ОГЭ" },
        new SelectListItem { Value = "ЕГЭ", Text = "ЕГЭ" },
        new SelectListItem { Value = "ВПР", Text = "ВПР" },
        new SelectListItem { Value = "Олимпиады", Text = "Олимпиады" },
        new SelectListItem { Value = "Школьная программа", Text = "Школьная программа" },
        new SelectListItem { Value = "Подготовка к поступлению", Text = "Подготовка к поступлению" },
        new SelectListItem { Value = "Повышение успеваемости", Text = "Повышение успеваемости" }
    };

    public class InputModel
    {
        [Required(ErrorMessage = "Выберите роль")]
        public string Role { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите логин")]
        [StringLength(512, ErrorMessage = "Максимальная длина — 512 символов")]
        [RegularExpression("^[a-zA-Z0-9]+$", ErrorMessage = "Логин должен содержать только латинские буквы и цифры")]
        public string Login { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите пароль")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Пароль должен быть не менее 8 символов")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Подтвердите пароль")]
        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите имя")]
        [StringLength(512, ErrorMessage = "Максимальная длина — 512 символов")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите фамилию")]
        [StringLength(512, ErrorMessage = "Максимальная длина — 512 символов")]
        public string LastName { get; set; } = string.Empty;

        [StringLength(512, ErrorMessage = "Максимальная длина — 512 символов")]
        public string? MiddleName { get; set; }

        [Required(ErrorMessage = "Введите дату рождения")]
        [DataType(DataType.Date, ErrorMessage = "Некорректный формат даты")]
        public DateTime BirthDate { get; set; }

        [Required(ErrorMessage = "Выберите пол")]
        public string Gender { get; set; } = string.Empty;

        public IFormFile? Photo { get; set; }

        // Teacher-specific fields
        public string? Education { get; set; }
        public List<string> SelectedPrograms { get; set; } = new();
        public decimal? HourlyRate { get; set; }
        public int? ExperienceYears { get; set; }
        public List<int> SelectedSubjects { get; set; } = new();
        public Dictionary<int, decimal> SubjectPrices { get; set; } = new();
    }

    public async Task OnGetAsync()
    {
        // Очистка ошибок при открытии страницы
        ErrorMessage = string.Empty;

        // Загрузка списка доступных предметов
        var subjects = await _teacherService.GetAvailableSubjectsAsync();
        AvailableSubjects = subjects.Select(s => new SelectListItem
        {
            Value = s.Id.ToString(),
            Text = s.Name
        }).ToList();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        _logger.LogInformation("Начало регистрации нового пользователя");
        
        // Валидация даты рождения
        if (Input.BirthDate == default)
        {
            ModelState.AddModelError("Input.BirthDate", "Пожалуйста, выберите дату рождения");
            await OnGetAsync(); // Перезагружаем список предметов
            return Page();
        }
        
        // Проверяем базовую валидацию модели
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Модель не валидна: {Errors}", 
                string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            await OnGetAsync(); // Перезагружаем список предметов
            return Page();
        }

        // Проверяем валидацию только для полей репетитора, если выбрана роль репетитора
        if (Input.Role == "Tutor")
        {
            var teacherErrors = new List<string>();

            if (string.IsNullOrEmpty(Input.Education))
            {
                teacherErrors.Add("Введите образование");
            }
            if (!Input.HourlyRate.HasValue)
            {
                teacherErrors.Add("Введите стоимость часа");
            }
            if (!Input.ExperienceYears.HasValue)
            {
                teacherErrors.Add("Введите опыт работы");
            }
            if (!Input.SelectedSubjects.Any())
            {
                teacherErrors.Add("Выберите хотя бы один предмет");
            }
            if (!Input.SelectedPrograms.Any())
            {
                teacherErrors.Add("Выберите хотя бы одну программу подготовки");
            }

            if (teacherErrors.Any())
            {
                foreach (var error in teacherErrors)
                {
                    ModelState.AddModelError("", error);
                }
                _logger.LogWarning("Модель не валидна для репетитора: {Errors}", string.Join(", ", teacherErrors));
                await OnGetAsync(); // Перезагружаем список предметов
                return Page();
            }
        }

        try
        {
            
            var user = new User
            {
                Login = Input.Login,
                UserName = Input.Login,
                FirstName = Input.FirstName,
                LastName = Input.LastName,
                MiddleName = Input.MiddleName,
                Role = Input.Role == "Tutor" ? "Teacher" : "Student",
                PasswordHash = string.Empty, // Initialize with empty string, will be set by AuthService
                CreatedAt = DateTime.UtcNow,
                BirthDate = Input.BirthDate,
            };
            
            if (Input.BirthDate.Kind != DateTimeKind.Utc)
            {
                user.BirthDate = DateTime.SpecifyKind(Input.BirthDate.Date, DateTimeKind.Utc);
            }
            else
            {
                user.BirthDate = Input.BirthDate;
            }

            _logger.LogInformation("Создан объект пользователя: {User}", user);

            // Сохраняем фото, если оно есть
            if (Input.Photo != null)
            {
                using var stream = Input.Photo.OpenReadStream();
                user.PhotoBase64 = await _storageService.SavePhotoAsync(stream, Input.Photo.FileName);
                _logger.LogInformation("Добавлено фото для пользователя {Login}", user.Login);
            }

            // Регистрируем пользователя
            _logger.LogInformation("Вызов AuthService.RegisterAsync для пользователя {Login}", user.Login);
            var registeredUser = await _authService.RegisterAsync(user, Input.Password);
            if (registeredUser == null)
            {
                _logger.LogWarning("Не удалось зарегистрировать пользователя {Login}", user.Login);
                ErrorMessage = "Пользователь с таким логином уже существует";
                await OnGetAsync(); // Перезагружаем список предметов
                return Page();
            }

            _logger.LogInformation("Пользователь {Login} успешно зарегистрирован с ID {Id}", 
                registeredUser.Login, registeredUser.Id);

            // Если это репетитор, создаем профиль
            if (Input.Role == "Tutor" && registeredUser != null)
            {
                _logger.LogInformation("Создание профиля репетитора для пользователя {Id}", registeredUser.Id);
                var teacherProfile = new TeacherProfile
                {
                    UserId = registeredUser.Id,
                    Education = Input.Education ?? string.Empty,
                    PreparationPrograms = Input.SelectedPrograms.ToArray(),
                    HourlyRate = Input.HourlyRate ?? 0,
                    ExperienceYears = Input.ExperienceYears ?? 0,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.TeacherProfiles.AddAsync(teacherProfile);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Профиль репетитора успешно создан");

                // Добавляем связи с предметами
                foreach (var subjectId in Input.SelectedSubjects)
                {
                    // Проверяем существование предмета
                    var subject = await _unitOfWork.Subjects.GetByIdAsync(subjectId);
                    if (subject == null)
                    {
                        _logger.LogWarning("Предмет с ID {SubjectId} не найден", subjectId);
                        continue;
                    }

                    var teacherSubject = new TeacherSubject
                    {
                        TeacherProfileId = teacherProfile.Id,
                        SubjectId = subjectId,
                        PricePerHour = Input.SubjectPrices.GetValueOrDefault(subjectId, Input.HourlyRate ?? 0)
                    };
                    await _unitOfWork.TeacherSubjects.AddAsync(teacherSubject);
                    _logger.LogInformation("Добавлен предмет {SubjectId} для репетитора {TeacherId}", subjectId, teacherProfile.Id);
                }
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Добавлены предметы для репетитора {TeacherId}", teacherProfile.Id);
            }

            return RedirectToPage("/Account/Login");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при регистрации пользователя");
            ErrorMessage = "Ошибка при регистрации: " + ex.Message;
            await OnGetAsync(); // Перезагружаем список предметов
            return Page();
        }
    }
} 