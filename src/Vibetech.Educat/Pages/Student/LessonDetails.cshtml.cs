using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.IO;
using Vibetech.Educat.Common.Interfaces;
using Vibetech.Educat.Common.Models;

namespace Vibetech.Educat.Pages.Student;

[Authorize(Roles = "Student")]
public class LessonDetailsModel : PageModel
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<LessonDetailsModel> _logger;

    public Lesson? Lesson { get; private set; }
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }
    
    // Property for attachments
    public List<Attachment> Attachments { get; set; } = new();
    
    // Properties for reviews
    public bool HasReview { get; set; }
    public Review? Review { get; set; }
    
    public int CurrentUserId { get; set; }
    
    [BindProperty]
    public IFormFile? UploadedFile { get; set; }
    
    public LessonDetailsModel(IUnitOfWork unitOfWork, ILogger<LessonDetailsModel> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        try
        {
            // Get the current student ID
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int studentId))
            {
                ErrorMessage = "Не удалось определить пользователя. Пожалуйста, войдите в систему заново.";
                return Page();
            }
            
            CurrentUserId = studentId;

            // Get the lesson with related entities
            var lessons = await _unitOfWork.Lessons.GetAllWithIncludesAsync(
                l => l.Subject,
                l => l.TeacherProfile,
                l => l.TeacherProfile.User,
                l => l.Attachments
            );

            // Find the specific lesson
            Lesson = lessons.FirstOrDefault(l => l.Id == id && l.StudentId == studentId);

            if (Lesson == null)
            {
                ErrorMessage = "Урок не найден или у вас нет прав для его просмотра.";
                return Page();
            }
            
            // Load attachments for this lesson
            Attachments = Lesson.Attachments.ToList();
            
            // Check if the student has already submitted a review for this teacher
            var reviews = await _unitOfWork.Reviews.FindAsync(r => 
                r.TeacherProfileId == Lesson.TeacherProfileId && 
                r.UserId == studentId);
            
            HasReview = reviews.Any();
            if (HasReview)
            {
                Review = reviews.FirstOrDefault();
            }
            
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading lesson details");
            ErrorMessage = $"Произошла ошибка при загрузке урока: {ex.Message}";
            return Page();
        }
    }
    
    public async Task<IActionResult> OnPostUploadAttachmentAsync(int lessonId)
    {
        try
        {
            // Verify the student has access to this lesson
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int studentId))
            {
                ErrorMessage = "Не удалось определить пользователя. Пожалуйста, войдите в систему заново.";
                return RedirectToPage(new { id = lessonId });
            }

            var lessons = await _unitOfWork.Lessons.GetAllWithIncludesAsync();
            var lesson = lessons.FirstOrDefault(l => l.Id == lessonId && l.StudentId == studentId);

            if (lesson == null)
            {
                ErrorMessage = "Урок не найден или у вас нет прав для загрузки файлов.";
                return RedirectToPage(new { id = lessonId });
            }

            // Check if file was uploaded
            if (UploadedFile == null || UploadedFile.Length == 0)
            {
                ErrorMessage = "Пожалуйста, выберите файл для загрузки.";
                return RedirectToPage(new { id = lessonId });
            }

            // Check file size (50MB limit)
            if (UploadedFile.Length > 50 * 1024 * 1024)
            {
                ErrorMessage = "Размер файла превышает допустимый лимит в 50 МБ.";
                return RedirectToPage(new { id = lessonId });
            }

            // Convert file to Base64
            string base64String;
            using (var memoryStream = new MemoryStream())
            {
                await UploadedFile.CopyToAsync(memoryStream);
                base64String = Convert.ToBase64String(memoryStream.ToArray());
            }

            // Create and save the attachment
            var attachment = new Attachment
            {
                LessonId = lessonId,
                UploadedById = studentId,
                FileName = UploadedFile.FileName,
                ContentType = UploadedFile.ContentType,
                FileBase64 = base64String,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Attachments.AddAsync(attachment);
            await _unitOfWork.SaveChangesAsync();

            SuccessMessage = "Файл успешно загружен.";
            return RedirectToPage(new { id = lessonId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading attachment");
            ErrorMessage = $"Произошла ошибка при загрузке файла: {ex.Message}";
            return RedirectToPage(new { id = lessonId });
        }
    }

    public async Task<IActionResult> OnGetDownloadAsync(int id, int attachmentId)
    {
        try
        {
            // Verify the student has access to this lesson
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int studentId))
            {
                ErrorMessage = "Не удалось определить пользователя. Пожалуйста, войдите в систему заново.";
                return RedirectToPage(new { id });
            }

            var lessons = await _unitOfWork.Lessons.GetAllWithIncludesAsync();
            var lesson = lessons.FirstOrDefault(l => l.Id == id && l.StudentId == studentId);

            if (lesson == null)
            {
                ErrorMessage = "Урок не найден или у вас нет прав для скачивания файлов.";
                return RedirectToPage(new { id });
            }

            // Get the attachment
            var attachment = await _unitOfWork.Attachments.GetByIdAsync(attachmentId);
            if (attachment == null || attachment.LessonId != id)
            {
                ErrorMessage = "Файл не найден или не принадлежит к этому уроку.";
                return RedirectToPage(new { id });
            }

            // Convert Base64 back to bytes
            byte[] fileBytes = Convert.FromBase64String(attachment.FileBase64);

            // Return file
            return File(fileBytes, attachment.ContentType, attachment.FileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading attachment");
            ErrorMessage = $"Произошла ошибка при скачивании файла: {ex.Message}";
            return RedirectToPage(new { id });
        }
    }

    public async Task<IActionResult> OnPostDeleteAttachmentAsync(int lessonId, int attachmentId)
    {
        try
        {
            // Verify the student has access to this lesson and attachment
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int studentId))
            {
                ErrorMessage = "Не удалось определить пользователя. Пожалуйста, войдите в систему заново.";
                return RedirectToPage(new { id = lessonId });
            }

            var lessons = await _unitOfWork.Lessons.GetAllWithIncludesAsync();
            var lesson = lessons.FirstOrDefault(l => l.Id == lessonId && l.StudentId == studentId);

            if (lesson == null)
            {
                ErrorMessage = "Урок не найден или у вас нет прав для удаления файлов.";
                return RedirectToPage(new { id = lessonId });
            }

            // Get the attachment
            var attachment = await _unitOfWork.Attachments.GetByIdAsync(attachmentId);
            if (attachment == null || attachment.LessonId != lessonId)
            {
                ErrorMessage = "Файл не найден или не принадлежит к этому уроку.";
                return RedirectToPage(new { id = lessonId });
            }
            
            // Verify the student owns this attachment
            if (attachment.UploadedById != studentId)
            {
                ErrorMessage = "Вы не можете удалить файл, загруженный другим пользователем.";
                return RedirectToPage(new { id = lessonId });
            }

            // Remove the attachment
            await _unitOfWork.Attachments.DeleteAsync(attachment.Id);
            await _unitOfWork.SaveChangesAsync();

            SuccessMessage = "Файл успешно удален.";
            return RedirectToPage(new { id = lessonId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting attachment");
            ErrorMessage = $"Произошла ошибка при удалении файла: {ex.Message}";
            return RedirectToPage(new { id = lessonId });
        }
    }
    
    public async Task<IActionResult> OnPostSubmitReviewAsync(int lessonId, int teacherProfileId, int rating, string comment)
    {
        try
        {
            // Verify the student has access to this lesson
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int studentId))
            {
                ErrorMessage = "Не удалось определить пользователя. Пожалуйста, войдите в систему заново.";
                return RedirectToPage(new { id = lessonId });
            }

            var lessons = await _unitOfWork.Lessons.GetAllWithIncludesAsync();
            var lesson = lessons.FirstOrDefault(l => l.Id == lessonId && l.StudentId == studentId);

            if (lesson == null)
            {
                ErrorMessage = "Урок не найден или у вас нет прав для оставления отзыва.";
                return RedirectToPage(new { id = lessonId });
            }
            
            // Verify the lesson is completed
            if (lesson.Status != LessonStatus.Completed)
            {
                ErrorMessage = "Вы можете оставить отзыв только после завершения урока.";
                return RedirectToPage(new { id = lessonId });
            }
            
            // Verify the teacher profile exists
            var teacherProfile = await _unitOfWork.TeacherProfiles.GetByIdAsync(teacherProfileId);
            if (teacherProfile == null)
            {
                ErrorMessage = "Профиль преподавателя не найден.";
                return RedirectToPage(new { id = lessonId });
            }
            
            // Check if student already submitted a review
            var existingReviews = await _unitOfWork.Reviews.FindAsync(r => 
                r.TeacherProfileId == teacherProfileId && 
                r.UserId == studentId);
            
            if (existingReviews.Any())
            {
                ErrorMessage = "Вы уже оставили отзыв об этом преподавателе.";
                return RedirectToPage(new { id = lessonId });
            }
            
            // Create and save the review
            var review = new Review
            {
                TeacherProfileId = teacherProfileId,
                UserId = studentId,
                Rating = rating,
                Comment = comment,
                CreatedAt = DateTime.UtcNow
            };
            
            await _unitOfWork.Reviews.AddAsync(review);
            await _unitOfWork.SaveChangesAsync();
            
            // Update teacher rating
            await UpdateTeacherRatingAsync(teacherProfileId);
            
            SuccessMessage = "Спасибо за отзыв! Он поможет другим студентам выбрать преподавателя.";
            return RedirectToPage(new { id = lessonId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting review");
            ErrorMessage = $"Произошла ошибка при отправке отзыва: {ex.Message}";
            return RedirectToPage(new { id = lessonId });
        }
    }
    
    public async Task<IActionResult> OnPostEditReviewAsync(int lessonId, int reviewId, int rating, string comment)
    {
        try
        {
            // Verify the student has access to this lesson
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int studentId))
            {
                ErrorMessage = "Не удалось определить пользователя. Пожалуйста, войдите в систему заново.";
                return RedirectToPage(new { id = lessonId });
            }

            var lessons = await _unitOfWork.Lessons.GetAllWithIncludesAsync();
            var lesson = lessons.FirstOrDefault(l => l.Id == lessonId && l.StudentId == studentId);

            if (lesson == null)
            {
                ErrorMessage = "Урок не найден или у вас нет прав для редактирования отзыва.";
                return RedirectToPage(new { id = lessonId });
            }
            
            // Get the review
            var review = await _unitOfWork.Reviews.GetByIdAsync(reviewId);
            if (review == null)
            {
                ErrorMessage = "Отзыв не найден.";
                return RedirectToPage(new { id = lessonId });
            }
            
            // Verify the student owns this review
            if (review.UserId != studentId)
            {
                ErrorMessage = "Вы не можете редактировать отзыв, оставленный другим пользователем.";
                return RedirectToPage(new { id = lessonId });
            }
            
            // Update the review
            review.Rating = rating;
            review.Comment = comment;
            
            await _unitOfWork.Reviews.UpdateAsync(review);
            await _unitOfWork.SaveChangesAsync();
            
            // Update teacher rating
            await UpdateTeacherRatingAsync(review.TeacherProfileId);
            
            SuccessMessage = "Ваш отзыв был успешно обновлен.";
            return RedirectToPage(new { id = lessonId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error editing review");
            ErrorMessage = $"Произошла ошибка при редактировании отзыва: {ex.Message}";
            return RedirectToPage(new { id = lessonId });
        }
    }
    
    private async Task UpdateTeacherRatingAsync(int teacherProfileId)
    {
        try
        {
            // Get all reviews for this teacher
            var reviews = await _unitOfWork.Reviews.FindAsync(r => r.TeacherProfileId == teacherProfileId);
            
            if (!reviews.Any())
                return;
                
            // Calculate average rating
            double averageRating = reviews.Average(r => r.Rating);
            int reviewsCount = reviews.Count();
            
            // Update teacher profile
            var teacherProfile = await _unitOfWork.TeacherProfiles.GetByIdAsync(teacherProfileId);
            if (teacherProfile != null)
            {
                teacherProfile.Rating = averageRating;
                teacherProfile.ReviewsCount = reviewsCount;
                
                await _unitOfWork.TeacherProfiles.UpdateAsync(teacherProfile);
                await _unitOfWork.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating teacher rating");
            // We don't want to stop the flow if rating update fails
        }
    }
} 