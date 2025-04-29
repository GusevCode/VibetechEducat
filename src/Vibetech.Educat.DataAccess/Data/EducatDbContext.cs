using Microsoft.EntityFrameworkCore;
using Vibetech.Educat.Common.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Vibetech.Educat.DataAccess.Data.Configuration;

namespace Vibetech.Educat.DataAccess.Data;

public class EducatDbContext : IdentityDbContext<User, IdentityRole<int>, int>
{
    public EducatDbContext(DbContextOptions<EducatDbContext> options)
        : base(options)
    {
    }

    public DbSet<TeacherProfile> TeacherProfiles { get; set; }
    public DbSet<Subject> Subjects { get; set; }
    public DbSet<TeacherSubject> TeacherSubjects { get; set; }
    public DbSet<TeacherStudent> TeacherStudents { get; set; }
    public DbSet<Lesson> Lessons { get; set; }
    public DbSet<Attachment> Attachments { get; set; }
    public DbSet<Review> Reviews { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations
        modelBuilder.ApplyConfiguration(new SubjectConfiguration());

        // Настройка TeacherProfile
        modelBuilder.Entity<TeacherProfile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Education).IsRequired();
            entity.Property(e => e.ExperienceYears).IsRequired();
            entity.Property(e => e.PreparationPrograms).HasColumnType("text[]");
            entity.Property(e => e.HourlyRate).HasColumnType("decimal(18,2)");
            entity.Property(e => e.IsModerated).IsRequired();
            entity.Property(e => e.Rating).HasColumnType("double precision");
            entity.Property(e => e.ReviewsCount).IsRequired();

            entity.HasOne(t => t.User)
                .WithMany(u => u.TeacherProfiles)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Настройка Subject
        modelBuilder.Entity<Subject>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
        });

        // Настройка TeacherSubject
        modelBuilder.Entity<TeacherSubject>(entity =>
        {
            entity.HasKey(e => new { e.TeacherProfileId, e.SubjectId });

            entity.HasOne(ts => ts.TeacherProfile)
                .WithMany(t => t.TeacherSubjects)
                .HasForeignKey(ts => ts.TeacherProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ts => ts.Subject)
                .WithMany(s => s.TeacherSubjects)
                .HasForeignKey(ts => ts.SubjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Настройка TeacherStudent
        modelBuilder.Entity<TeacherStudent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).IsRequired().HasDefaultValue(RequestStatus.Pending);

            entity.HasOne(e => e.TeacherProfile)
                .WithMany(t => t.TeacherStudents)
                .HasForeignKey(e => e.TeacherProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Student)
                .WithMany(s => s.TeacherStudents)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
            
            

            // Индексы
            entity.HasIndex(e => new { e.TeacherProfileId, e.StudentId }).IsUnique();
        });

        // Настройка Lesson
        modelBuilder.Entity<Lesson>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ScheduledStart).IsRequired();
            entity.Property(e => e.ScheduledEnd).IsRequired();

            entity.HasOne(e => e.TeacherProfile)
                .WithMany(t => t.Lessons)
                .HasForeignKey(e => e.TeacherProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Student)
                .WithMany(u => u.Lessons)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Subject)
                .WithMany(s => s.Lessons)
                .HasForeignKey(e => e.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            // Индексы
            entity.HasIndex(e => e.ScheduledStart);
            entity.HasIndex(e => e.ScheduledEnd);
            entity.HasIndex(e => new { e.TeacherProfileId, e.ScheduledStart });
            entity.HasIndex(e => new { e.StudentId, e.ScheduledStart });
        });

        // Настройка Attachment
        modelBuilder.Entity<Attachment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).IsRequired();
            entity.Property(e => e.ContentType).IsRequired();
            entity.Property(e => e.FileBase64).IsRequired();

            entity.HasOne(e => e.Lesson)
                .WithMany(l => l.Attachments)
                .HasForeignKey(e => e.LessonId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.UploadedBy)
                .WithMany(u => u.Attachments)
                .HasForeignKey(e => e.UploadedById)
                .OnDelete(DeleteBehavior.Restrict);

            // Индексы
            entity.HasIndex(e => e.LessonId);
            entity.HasIndex(e => e.UploadedById);
        });

        // Настройка Review
        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Rating).IsRequired();
            entity.Property(e => e.Comment).IsRequired();

            entity.HasOne(e => e.TeacherProfile)
                .WithMany(t => t.Reviews)
                .HasForeignKey(e => e.TeacherProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Индексы
            entity.HasIndex(e => e.TeacherProfileId);
            entity.HasIndex(e => e.UserId);
        });
    }
} 