using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using Vibetech.Educat.Common.Interfaces;
using Vibetech.Educat.Common.Models;
using Vibetech.Educat.Services.Services.TeacherService;

[Authorize(Roles = "Teacher")]
public class LessonsModel : PageModel
{
    private readonly ITeacherService _teacherService;
    private readonly IUnitOfWork _unitOfWork;

    public List<Lesson> Lessons { get; set; } = new();
    public string? ErrorMessage { get; set; } = null;
    
    // Pagination properties
    [BindProperty(SupportsGet = true)]
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 9; // Show 9 lessons per page (3 rows of 3)
    public int TotalPages { get; set; }
    public int TotalLessons { get; set; }
    
    // Sorting properties
    [BindProperty(SupportsGet = true)]
    public string SortField { get; set; } = "ScheduledStart";
    
    [BindProperty(SupportsGet = true)]
    public string SortOrder { get; set; } = "desc";
    
    // Filtering properties
    [BindProperty(SupportsGet = true)]
    public string? StatusFilter { get; set; }
    
    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    public LessonsModel(ITeacherService teacherService, IUnitOfWork unitOfWork)
    {
        _teacherService = teacherService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            int teacherProfileId = await GetTeacherProfileIdAsync();
            if (teacherProfileId <= 0)
            {
                // Error message is already set in GetTeacherProfileIdAsync
                return Page();
            }
            
            // Get all lessons for the teacher
            var allLessons = await _teacherService.GetTeacherLessonsAsync(teacherProfileId);
            
            // Apply filtering
            var filteredLessons = allLessons;
            
            // Filter by status if specified
            if (!string.IsNullOrEmpty(StatusFilter))
            {
                if (Enum.TryParse<LessonStatus>(StatusFilter, out var status))
                {
                    filteredLessons = filteredLessons.Where(l => l.Status == status).ToList();
                }
            }
            
            // Filter by search term if specified
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                string searchLower = SearchTerm.ToLower();
                filteredLessons = filteredLessons.Where(l => 
                    (l.Subject?.Name?.ToLower().Contains(searchLower) ?? false) ||
                    (l.Student != null && $"{l.Student.LastName} {l.Student.FirstName}".ToLower().Contains(searchLower))
                ).ToList();
            }
            
            // Apply sorting
            filteredLessons = ApplySorting(filteredLessons);
            
            // Calculate pagination
            TotalLessons = filteredLessons.Count;
            TotalPages = (int)Math.Ceiling(TotalLessons / (double)PageSize);
            
            // Ensure CurrentPage is valid
            if (CurrentPage < 1)
                CurrentPage = 1;
            else if (CurrentPage > TotalPages && TotalPages > 0)
                CurrentPage = TotalPages;
                
            // Apply pagination
            Lessons = filteredLessons
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
                
            return Page();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Произошла ошибка при загрузке уроков: {ex.Message}";
            return Page();
        }
    }
    
    private List<Lesson> ApplySorting(List<Lesson> lessons)
    {
        return SortField switch
        {
            "Subject" => SortOrder == "asc" 
                ? lessons.OrderBy(l => l.Subject?.Name).ToList()
                : lessons.OrderByDescending(l => l.Subject?.Name).ToList(),
                
            "Status" => SortOrder == "asc"
                ? lessons.OrderBy(l => l.Status).ToList()
                : lessons.OrderByDescending(l => l.Status).ToList(),
                
            "Student" => SortOrder == "asc"
                ? lessons.OrderBy(l => l.Student != null ? $"{l.Student.LastName} {l.Student.FirstName}" : "").ToList()
                : lessons.OrderByDescending(l => l.Student != null ? $"{l.Student.LastName} {l.Student.FirstName}" : "").ToList(),
                
            "ScheduledStart" or _ => SortOrder == "asc"
                ? lessons.OrderBy(l => l.ScheduledStart).ToList()
                : lessons.OrderByDescending(l => l.ScheduledStart).ToList()
        };
    }
    
    // Helper method to get teacher profile ID
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