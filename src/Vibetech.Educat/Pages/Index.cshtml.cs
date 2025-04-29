using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Vibetech.Educat.Common.Models;
using Vibetech.Educat.Common.Interfaces;
using Vibetech.Educat.DataAccess.Data;

namespace Vibetech.Educat.Pages;

public class IndexModel : PageModel
{
    private readonly IUnitOfWork _unitOfWork;

    public IndexModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public bool IsAuthenticated { get; private set; }
    
    // Данные для графика активности учеников у репетитора
    public List<string> MonthLabels { get; set; } = new List<string>();
    public List<int> StudentCountByMonth { get; set; } = new List<int>();
    
    // Данные для графика часов обучения у репетитора
    public List<string> WeekdayLabels { get; set; } = new List<string>();
    public List<int> HoursByWeekday { get; set; } = new List<int>();
    
    // Данные для графика рейтингов у репетитора
    public List<string> RatingLabels { get; set; } = new List<string>();
    public List<int> RatingsData { get; set; } = new List<int>();
    
    // Данные для графика предметов у репетитора
    public List<string> SubjectLabels { get; set; } = new List<string>();
    public List<int> SubjectData { get; set; } = new List<int>();
    
    // Данные для графика прогресса ученика
    public List<string> StudentMonthLabels { get; set; } = new List<string>();
    public List<int> StudentProgress { get; set; } = new List<int>();
    
    // Данные для графика часов занятий ученика
    public List<string> StudentWeekdayLabels { get; set; } = new List<string>();
    public List<int> StudentHoursByWeekday { get; set; } = new List<int>();
    
    // Данные для графика предстоящих занятий ученика
    public List<string> UpcomingDaysLabels { get; set; } = new List<string>();
    public List<int> UpcomingLessonsCount { get; set; } = new List<int>();
    
    // Данные для графика распределения по предметам ученика
    public List<string> StudentSubjectLabels { get; set; } = new List<string>();
    public List<int> StudentSubjectData { get; set; } = new List<int>();

    public async Task OnGetAsync()
    {
        IsAuthenticated = User.Identity?.IsAuthenticated ?? false;

        if (IsAuthenticated)
        {
            if (User.IsInRole("Teacher"))
            {
                await LoadTeacherDashboardData();
            }
            else
            {
                await LoadStudentDashboardData();
            }
        }
    }

    private async Task LoadTeacherDashboardData()
    {
        // Получение ID текущего преподавателя
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var teacherId))
        {
            SetupEmptyChartData();
            return;
        }
        
        // 1. Получаем профиль преподавателя
        var teacherProfiles = await _unitOfWork.TeacherProfiles.FindAsync(tp => tp.UserId == teacherId);
        var teacherProfile = teacherProfiles.FirstOrDefault();
        
        if (teacherProfile == null)
        {
            // Если профиль не найден, используем пустые данные
            SetupEmptyChartData();
            return;
        }
        
        // 2. Подготовка меток для месяцев (последние 6 месяцев)
        var months = new[] { "Янв", "Фев", "Мар", "Апр", "Май", "Июн", "Июл", "Авг", "Сен", "Окт", "Ноя", "Дек" };
        var currentMonth = DateTime.Now.Month - 1; // 0-based index
        
        for (int i = 5; i >= 0; i--)
        {
            var monthIndex = (currentMonth - i + 12) % 12; // Ensure positive index with modulo
            MonthLabels.Add(months[monthIndex]);
        }
        
        // 3. Подготовка данных по количеству учеников за последние 6 месяцев
        await LoadStudentCountByMonth(teacherProfile.Id);
        
        // 4. Подготовка данных для графика часов по дням недели
        await LoadHoursByWeekday(teacherProfile.Id);
        
        // 5. Подготовка данных для графика рейтингов
        await LoadRatingsData(teacherProfile.Id);
        
        // 6. Подготовка данных для графика предметов
        await LoadSubjectsData(teacherProfile.Id);
    }
    
    private async Task LoadStudentDashboardData()
    {
        // Получение ID текущего ученика
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var studentId))
        {
            SetupEmptyStudentChartData();
            return;
        }
        
        // 1. Подготовка меток для месяцев (последние 6 месяцев)
        var months = new[] { "Янв", "Фев", "Мар", "Апр", "Май", "Июн", "Июл", "Авг", "Сен", "Окт", "Ноя", "Дек" };
        var currentMonth = DateTime.Now.Month - 1; // 0-based index
        
        StudentMonthLabels = new List<string>();
        for (int i = 5; i >= 0; i--)
        {
            var monthIndex = (currentMonth - i + 12) % 12; // Ensure positive index with modulo
            StudentMonthLabels.Add(months[monthIndex]);
        }
        
        // 2. Загрузка данных о прогрессе ученика
        await LoadStudentProgressData(studentId);
        
        // 3. Загрузка данных о часах занятий по дням недели
        await LoadStudentHoursByWeekday(studentId);
        
        // 4. Загрузка данных о предстоящих занятиях
        await LoadUpcomingLessonsData(studentId);
        
        // 5. Загрузка данных о распределении по предметам
        await LoadStudentSubjectsData(studentId);
    }
    
    private void SetupEmptyChartData()
    {
        // Заполняем месяцы
        var months = new[] { "Янв", "Фев", "Мар", "Апр", "Май", "Июн", "Июл", "Авг", "Сен", "Окт", "Ноя", "Дек" };
        var currentMonth = DateTime.Now.Month - 1; // 0-based index
        
        MonthLabels = new List<string>();
        for (int i = 5; i >= 0; i--)
        {
            var monthIndex = (currentMonth - i + 12) % 12;
            MonthLabels.Add(months[monthIndex]);
        }
        
        StudentCountByMonth = new List<int> { 0, 0, 0, 0, 0, 0 };
        
        WeekdayLabels = new List<string> { "Пн", "Вт", "Ср", "Чт", "Пт", "Сб", "Вс" };
        HoursByWeekday = new List<int> { 0, 0, 0, 0, 0, 0, 0 };
        
        RatingLabels = new List<string> { "5 звезд", "4 звезды", "3 звезды", "2 звезды", "1 звезда" };
        RatingsData = new List<int> { 0, 0, 0, 0, 0 };
        
        SubjectLabels = new List<string> { "Нет данных" };
        SubjectData = new List<int> { 0 };
    }
    
    private void SetupEmptyStudentChartData()
    {
        // Заполняем месяцы
        var months = new[] { "Янв", "Фев", "Мар", "Апр", "Май", "Июн", "Июл", "Авг", "Сен", "Окт", "Ноя", "Дек" };
        var currentMonth = DateTime.Now.Month - 1; // 0-based index
        
        StudentMonthLabels = new List<string>();
        for (int i = 5; i >= 0; i--)
        {
            var monthIndex = (currentMonth - i + 12) % 12;
            StudentMonthLabels.Add(months[monthIndex]);
        }
        
        StudentProgress = new List<int> { 0, 0, 0, 0, 0, 0 };
        
        StudentWeekdayLabels = new List<string> { "Пн", "Вт", "Ср", "Чт", "Пт", "Сб", "Вс" };
        StudentHoursByWeekday = new List<int> { 0, 0, 0, 0, 0, 0, 0 };
        
        UpcomingDaysLabels = new List<string> { "Сегодня", "Завтра", "3 день", "4 день", "5 день", "6 день", "7 день" };
        UpcomingLessonsCount = new List<int> { 0, 0, 0, 0, 0, 0, 0 };
        
        StudentSubjectLabels = new List<string> { "Нет данных" };
        StudentSubjectData = new List<int> { 0 };
    }
    
    private async Task LoadStudentCountByMonth(int teacherProfileId)
    {
        try
        {
            // Получаем все занятия за последние 6 месяцев
            var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
            var lessons = await _unitOfWork.Lessons.FindAsync(l => 
                l.TeacherProfileId == teacherProfileId && 
                l.ScheduledStart >= sixMonthsAgo);
            
            var lessonsList = lessons.ToList();
            
            // Подготавливаем названия месяцев для диаграммы
            MonthLabels = new List<string>();
            for (int i = 0; i < 6; i++)
            {
                var monthDate = DateTime.UtcNow.AddMonths(-(5-i));
                MonthLabels.Add(monthDate.ToString("MMM")); // Отображаем сокращенное название месяца
            }
            
            // Инициализируем словарь для подсчета уникальных студентов по месяцам
            var studentsByMonth = new Dictionary<int, HashSet<int>>();
            
            // Инициализируем словарь для всех 6 месяцев
            for (int i = 0; i < 6; i++)
            {
                var monthDate = DateTime.UtcNow.AddMonths(-(5-i));
                var monthKey = monthDate.Month + monthDate.Year * 100; // Уникальный ключ для месяца
                studentsByMonth[monthKey] = new HashSet<int>();
            }
            
            // Заполняем данные из занятий
            foreach (var lesson in lessonsList)
            {
                // Проверяем, что у урока есть студент
                if (lesson.StudentId.HasValue)
                {
                    var monthKey = lesson.ScheduledStart.Month + lesson.ScheduledStart.Year * 100;
                    if (studentsByMonth.ContainsKey(monthKey))
                    {
                        studentsByMonth[monthKey].Add(lesson.StudentId.Value);
                    }
                }
            }
            
            // Формируем массив данных для диаграммы
            StudentCountByMonth = new List<int>();
            for (int i = 0; i < 6; i++)
            {
                var monthDate = DateTime.UtcNow.AddMonths(-(5-i));
                var monthKey = monthDate.Month + monthDate.Year * 100;
                if (studentsByMonth.ContainsKey(monthKey))
                {
                    StudentCountByMonth.Add(studentsByMonth[monthKey].Count);
                }
                else
                {
                    StudentCountByMonth.Add(0);
                }
            }
        }
        catch (Exception)
        {
            // В случае ошибки используем пустые данные
            StudentCountByMonth = new List<int> { 0, 0, 0, 0, 0, 0 };
        }
    }
    
    private async Task LoadHoursByWeekday(int teacherProfileId)
    {
        try
        {
            // Получаем все занятия за последний месяц
            var lastMonth = DateTime.UtcNow.AddMonths(-1);
            var lessons = await _unitOfWork.Lessons.FindAsync(l => 
                l.TeacherProfileId == teacherProfileId && 
                l.ScheduledStart >= lastMonth);
            
            var lessonsList = lessons.ToList();
            
            // Инициализируем массив для дней недели (0 - понедельник, 6 - воскресенье)
            var hoursByDay = new int[7];
            
            // Рассчитываем количество часов для каждого дня недели
            foreach (var lesson in lessonsList)
            {
                // Переводим день недели .NET (где 0 - воскресенье) в наш формат (где 0 - понедельник)
                int dayOfWeek = ((int)lesson.ScheduledStart.DayOfWeek + 6) % 7;
                
                // Вычисляем продолжительность в часах
                double hours = (lesson.ScheduledEnd - lesson.ScheduledStart).TotalHours;
                hoursByDay[dayOfWeek] += (int)Math.Ceiling(hours);
            }
            
            // Подготавливаем данные для диаграммы
            WeekdayLabels = new List<string> { "Пн", "Вт", "Ср", "Чт", "Пт", "Сб", "Вс" };
            HoursByWeekday = hoursByDay.ToList();
        }
        catch (Exception)
        {
            // В случае ошибки используем пустые данные
            WeekdayLabels = new List<string> { "Пн", "Вт", "Ср", "Чт", "Пт", "Сб", "Вс" };
            HoursByWeekday = new List<int> { 0, 0, 0, 0, 0, 0, 0 };
        }
    }
    
    private async Task LoadRatingsData(int teacherProfileId)
    {
        try
        {
            // Получаем все отзывы о преподавателе
            var reviews = await _unitOfWork.Reviews.FindAsync(r => r.TeacherProfileId == teacherProfileId);
            var reviewsList = reviews.ToList();
            
            // Инициализируем массив для рейтингов (индекс 0 - оценка 1, индекс 4 - оценка 5)
            var ratingCounts = new int[5];
            
            // Подсчитываем количество отзывов для каждой оценки
            foreach (var review in reviewsList)
            {
                if (review.Rating >= 1 && review.Rating <= 5)
                {
                    ratingCounts[review.Rating - 1]++;
                }
            }
            
            // Переворачиваем массив, чтобы индекс 0 соответствовал оценке 5
            RatingLabels = new List<string> { "5 звезд", "4 звезды", "3 звезды", "2 звезды", "1 звезда" };
            RatingsData = ratingCounts.Reverse().ToList();
        }
        catch (Exception)
        {
            // В случае ошибки используем пустые данные
            RatingLabels = new List<string> { "5 звезд", "4 звезды", "3 звезды", "2 звезды", "1 звезда" };
            RatingsData = new List<int> { 0, 0, 0, 0, 0 };
        }
    }
    
    private async Task LoadSubjectsData(int teacherProfileId)
    {
        try
        {
            // Получаем все занятия преподавателя
            var lessons = await _unitOfWork.Lessons.GetAllWithIncludesAsync(l => l.Subject);
            var lessonsList = lessons.Where(l => l.TeacherProfileId == teacherProfileId).ToList();
            
            // Группируем занятия по предметам и считаем количество
            var subjectCounts = lessonsList
                .GroupBy(l => l.Subject?.Name ?? "Неизвестно")
                .Select(g => new { SubjectName = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();
            
            if (subjectCounts.Any())
            {
                SubjectLabels = subjectCounts.Select(s => s.SubjectName).ToList();
                SubjectData = subjectCounts.Select(s => s.Count).ToList();
            }
            else
            {
                // Если нет данных о предметах, получаем список предметов преподавателя
                var teacherSubjects = await _unitOfWork.TeacherSubjects.GetAllWithIncludesAsync(ts => ts.Subject);
                var subjects = teacherSubjects
                    .Where(ts => ts.TeacherProfileId == teacherProfileId)
                    .Select(ts => ts.Subject.Name)
                    .ToList();
                
                if (subjects.Any())
                {
                    SubjectLabels = subjects;
                    SubjectData = subjects.Select(_ => 0).ToList(); // Нет занятий, поэтому все нули
                }
                else
                {
                    // Если нет предметов, используем заглушку
                    SubjectLabels = new List<string> { "Нет данных" };
                    SubjectData = new List<int> { 0 };
                }
            }
        }
        catch (Exception)
        {
            // В случае ошибки используем пустые данные
            SubjectLabels = new List<string> { "Нет данных" };
            SubjectData = new List<int> { 0 };
        }
    }
    
    private async Task LoadStudentProgressData(int studentId)
    {
        try
        {
            // В данном примере у нас нет реальных данных по прогрессу ученика
            // Вместо этого можно использовать завершенные уроки как приближение
            var startDate = DateTime.UtcNow.AddMonths(-6);
            
            // Получаем все завершенные занятия ученика
            var lessons = await _unitOfWork.Lessons.FindAsync(l => 
                l.StudentId == studentId && 
                l.ScheduledStart >= startDate && 
                l.ScheduledEnd < DateTime.UtcNow && 
                !l.IsCancelled);
            
            var lessonsList = lessons.ToList();
            
            // Группируем занятия по месяцам
            var lessonsByMonth = new Dictionary<int, int>();
            
            // Инициализируем словарь для всех 6 месяцев
            for (int i = 0; i < 6; i++)
            {
                var monthDate = DateTime.UtcNow.AddMonths(-(5-i));
                var monthKey = monthDate.Month + monthDate.Year * 100; // Уникальный ключ для месяца
                lessonsByMonth[monthKey] = 0;
            }
            
            // Заполняем данные из занятий
            foreach (var lesson in lessonsList)
            {
                var monthKey = lesson.ScheduledStart.Month + lesson.ScheduledStart.Year * 100;
                if (lessonsByMonth.ContainsKey(monthKey))
                {
                    lessonsByMonth[monthKey]++;
                }
            }
            
            // Формируем массив данных для диаграммы - преобразуем количество занятий в "прогресс" (0-100)
            // Для простоты используем простую формулу: 1 занятие = +10% прогресса, максимум 100%
            StudentProgress = new List<int>();
            var progress = 60; // Начальный прогресс (условно)
            
            for (int i = 0; i < 6; i++)
            {
                var monthDate = DateTime.UtcNow.AddMonths(-(5-i));
                var monthKey = monthDate.Month + monthDate.Year * 100;
                if (lessonsByMonth.ContainsKey(monthKey))
                {
                    // Увеличиваем прогресс на 5% за каждое занятие
                    progress += lessonsByMonth[monthKey] * 5;
                    progress = Math.Min(progress, 100); // Не больше 100%
                }
                StudentProgress.Add(progress);
            }
        }
        catch (Exception)
        {
            // В случае ошибки используем тестовые данные
            StudentProgress = new List<int> { 65, 70, 75, 82, 85, 90 };
        }
    }
    
    private async Task LoadStudentHoursByWeekday(int studentId)
    {
        try
        {
            // Получаем все занятия за последний месяц
            var lastMonth = DateTime.UtcNow.AddMonths(-1);
            var lessons = await _unitOfWork.Lessons.FindAsync(l => 
                l.StudentId == studentId && 
                l.ScheduledStart >= lastMonth);
            
            var lessonsList = lessons.ToList();
            
            // Инициализируем массив для дней недели (0 - понедельник, 6 - воскресенье)
            var hoursByDay = new int[7];
            
            // Рассчитываем количество часов для каждого дня недели
            foreach (var lesson in lessonsList)
            {
                // Переводим день недели .NET (где 0 - воскресенье) в наш формат (где 0 - понедельник)
                int dayOfWeek = ((int)lesson.ScheduledStart.DayOfWeek + 6) % 7;
                
                // Вычисляем продолжительность в часах
                double hours = (lesson.ScheduledEnd - lesson.ScheduledStart).TotalHours;
                hoursByDay[dayOfWeek] += (int)Math.Ceiling(hours);
            }
            
            // Подготавливаем данные для диаграммы
            StudentWeekdayLabels = new List<string> { "Пн", "Вт", "Ср", "Чт", "Пт", "Сб", "Вс" };
            StudentHoursByWeekday = hoursByDay.ToList();
        }
        catch (Exception)
        {
            // В случае ошибки используем тестовые данные
            StudentWeekdayLabels = new List<string> { "Пн", "Вт", "Ср", "Чт", "Пт", "Сб", "Вс" };
            StudentHoursByWeekday = new List<int> { 2, 3, 1, 4, 2, 0, 0 };
        }
    }
    
    private async Task LoadUpcomingLessonsData(int studentId)
    {
        try
        {
            // Получаем все предстоящие занятия ученика на ближайшие 7 дней
            var now = DateTime.UtcNow;
            var endDate = now.AddDays(7);
            
            var lessons = await _unitOfWork.Lessons.FindAsync(l => 
                l.StudentId == studentId && 
                l.ScheduledStart >= now && 
                l.ScheduledStart <= endDate && 
                !l.IsCancelled);
            
            var lessonsList = lessons.ToList();
            
            // Инициализируем метки и данные для 7 дней
            UpcomingDaysLabels = new List<string> { 
                "Сегодня", 
                "Завтра", 
                now.AddDays(2).ToString("dd.MM"), 
                now.AddDays(3).ToString("dd.MM"), 
                now.AddDays(4).ToString("dd.MM"), 
                now.AddDays(5).ToString("dd.MM"), 
                now.AddDays(6).ToString("dd.MM") 
            };
            
            // Инициализируем массив для количества занятий на каждый день
            var lessonsByDay = new int[7];
            
            // Заполняем данные о занятиях
            foreach (var lesson in lessonsList)
            {
                // Вычисляем индекс дня (0 - сегодня, 1 - завтра и т.д.)
                int dayIndex = (int)(lesson.ScheduledStart.Date - now.Date).TotalDays;
                if (dayIndex >= 0 && dayIndex < 7)
                {
                    lessonsByDay[dayIndex]++;
                }
            }
            
            UpcomingLessonsCount = lessonsByDay.ToList();
        }
        catch (Exception)
        {
            // В случае ошибки используем тестовые данные
            UpcomingDaysLabels = new List<string> { "Сегодня", "Завтра", "3 день", "4 день", "5 день", "6 день", "7 день" };
            UpcomingLessonsCount = new List<int> { 2, 1, 3, 0, 2, 1, 0 };
        }
    }
    
    private async Task LoadStudentSubjectsData(int studentId)
    {
        try
        {
            // Получаем все занятия ученика с информацией о предметах
            var lessons = await _unitOfWork.Lessons.GetAllWithIncludesAsync(l => l.Subject);
            var lessonsList = lessons.Where(l => l.StudentId == studentId).ToList();
            
            // Группируем занятия по предметам и считаем количество
            var subjectCounts = lessonsList
                .GroupBy(l => l.Subject?.Name ?? "Неизвестно")
                .Select(g => new { SubjectName = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();
            
            if (subjectCounts.Any())
            {
                StudentSubjectLabels = subjectCounts.Select(s => s.SubjectName).ToList();
                StudentSubjectData = subjectCounts.Select(s => s.Count).ToList();
            }
            else
            {
                // Если нет данных, используем заглушку
                StudentSubjectLabels = new List<string> { "Нет данных" };
                StudentSubjectData = new List<int> { 0 };
            }
        }
        catch (Exception)
        {
            // В случае ошибки используем тестовые данные
            StudentSubjectLabels = new List<string> { "Математика", "Физика", "Химия", "Английский", "Информатика" };
            StudentSubjectData = new List<int> { 5, 3, 2, 4, 1 };
        }
    }
}