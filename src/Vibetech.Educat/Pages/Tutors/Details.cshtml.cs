using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class DetailsModel : PageModel
{
    public required TutorViewModel Tutor { get; set; }
    public required List<string> AvailableTimes { get; set; }

    [BindProperty]
    public required BookingInputModel Input { get; set; }

    public class TutorViewModel
    {
        public int Id { get; set; }
        public required string PhotoUrl { get; set; }
        public required string FullName { get; set; }
        public required string Subject { get; set; }
        public required string Education { get; set; }
        public required string[] PreparationPrograms { get; set; }
        public int Experience { get; set; }
        public decimal Price { get; set; }
        public double Rating { get; set; }
        public int ReviewsCount { get; set; }
        public required string Location { get; set; }
        public required string About { get; set; }
        public required string Phone { get; set; }
        public required string Email { get; set; }
        public string? ContactInformation { get; set; }
        public required List<ReviewViewModel> Reviews { get; set; }
    }

    public class ReviewViewModel
    {
        public required string StudentName { get; set; }
        public double Rating { get; set; }
        public required string Text { get; set; }
        public DateTime Date { get; set; }
    }

    public class BookingInputModel
    {
        [Required(ErrorMessage = "Выберите дату")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Выберите время")]
        public required string Time { get; set; }

        [Required(ErrorMessage = "Выберите длительность")]
        public int Duration { get; set; }

        [Required(ErrorMessage = "Выберите формат")]
        public required string Format { get; set; }
    }

    public void OnGet(int id)
    {
        // Здесь должна быть логика получения данных репетитора по id
        // Пока что просто заполняем тестовыми данными
        Tutor = new TutorViewModel
        {
            Id = id,
            PhotoUrl = "/images/tutors/tutor1.jpg",
            FullName = "Иванов Иван Иванович",
            Subject = "Математика",
            Education = "МГУ, факультет математики",
            PreparationPrograms = new[] { "OGE", "EGE", "Olympiad" },
            Experience = 5,
            Price = 1500,
            Rating = 4.8,
            ReviewsCount = 12,
            Location = "Москва, м. Университет",
            About = "Опытный репетитор по математике. Готовлю к ОГЭ, ЕГЭ и олимпиадам. Индивидуальный подход к каждому ученику.",
            Phone = "+7 (999) 123-45-67",
            Email = "ivanov@example.com",
            ContactInformation = "Telegram: @ivanov_math, WhatsApp: +79991234567",
            Reviews = new List<ReviewViewModel>
            {
                new ReviewViewModel
                {
                    StudentName = "Анна Смирнова",
                    Rating = 5,
                    Text = "Отличный преподаватель! Объясняет очень понятно и доступно. Занимаюсь уже полгода, результаты стали намного лучше.",
                    Date = DateTime.Now.AddDays(-10)
                },
                new ReviewViewModel
                {
                    StudentName = "Петр Иванов",
                    Rating = 4,
                    Text = "Хороший репетитор, но иногда опаздывает на занятия.",
                    Date = DateTime.Now.AddDays(-20)
                }
            }
        };

        // Заполняем доступные временные слоты
        AvailableTimes = new List<string>
        {
            "09:00", "10:00", "11:00", "12:00", "13:00", "14:00", "15:00", "16:00", "17:00", "18:00", "19:00", "20:00"
        };
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Здесь должна быть логика создания записи на занятие
        // Пока что просто редиректим на страницу успешной записи
        return RedirectToPage("/Booking/Success", new { tutorId = Tutor.Id });
    }
} 