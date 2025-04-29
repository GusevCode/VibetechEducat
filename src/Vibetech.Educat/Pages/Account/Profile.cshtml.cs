using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

public class ProfileModel : PageModel
{
    [BindProperty]
    public required InputModel Input { get; set; }

    public required string PhotoUrl { get; set; }
    public required string FullName { get; set; }
    public bool IsTutor { get; set; }
    public required string[] PreparationPrograms { get; set; }

    public class InputModel
    {
        public required string Login { get; set; }
        public required string FullName { get; set; }
        public DateTime BirthDate { get; set; }
        public required string Gender { get; set; }
        public required string Subject { get; set; }
        public required string[] PreparationPrograms { get; set; }
        public decimal? Price { get; set; }
        public int? Experience { get; set; }
        public required string Education { get; set; }
    }

    public void OnGet()
    {
        // Здесь должна быть логика загрузки данных пользователя
        // Пока что просто заполняем тестовыми данными
        Input = new InputModel
        {
            Login = "testuser",
            FullName = "Иванов Иван Иванович",
            BirthDate = new DateTime(2000, 1, 1),
            Gender = "Male",
            Subject = "Math",
            PreparationPrograms = new[] { "OGE", "EGE" },
            Price = 1000,
            Experience = 5,
            Education = "МГУ, факультет математики"
        };

        PhotoUrl = "/images/default-avatar.jpg";
        FullName = Input.FullName;
        IsTutor = true;
        PreparationPrograms = Input.PreparationPrograms;
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Здесь должна быть логика сохранения изменений
        // Пока что просто перезагружаем страницу
        return RedirectToPage();
    }

    public IActionResult OnPostPhoto(IFormFile photo)
    {
        if (photo == null || photo.Length == 0)
        {
            return BadRequest("Файл не выбран");
        }

        // Здесь должна быть логика сохранения фото
        // Пока что просто перезагружаем страницу
        return RedirectToPage();
    }
} 