using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;

public class SuccessModel : PageModel
{
    public int TutorId { get; set; }
    public required string TutorName { get; set; }
    public DateTime BookingDate { get; set; }
    public required string BookingTime { get; set; }
    public int Duration { get; set; }
    public required string Format { get; set; }
    public decimal Price { get; set; }

    public void OnGet(int tutorId)
    {
        // Здесь должна быть логика получения данных о созданной записи
        // Пока что просто заполняем тестовыми данными
        TutorId = tutorId;
        TutorName = "Иванов Иван Иванович";
        BookingDate = DateTime.Now.AddDays(1);
        BookingTime = "15:00";
        Duration = 60;
        Format = "Онлайн";
        Price = 1500;
    }
} 