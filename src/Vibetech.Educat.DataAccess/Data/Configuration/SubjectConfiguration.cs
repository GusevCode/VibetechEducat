using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vibetech.Educat.Common.Models;

namespace Vibetech.Educat.DataAccess.Data.Configuration;

public class SubjectConfiguration : IEntityTypeConfiguration<Subject>
{
    public void Configure(EntityTypeBuilder<Subject> builder)
    {
        builder.HasData(
            new Subject { Id = 1, Name = "Математика", Description = "Математика для школьников и студентов", CreatedAt = DateTime.UtcNow },
            new Subject { Id = 2, Name = "Физика", Description = "Физика для школьников и студентов", CreatedAt = DateTime.UtcNow },
            new Subject { Id = 3, Name = "Химия", Description = "Химия для школьников и студентов", CreatedAt = DateTime.UtcNow },
            new Subject { Id = 4, Name = "Биология", Description = "Биология для школьников и студентов", CreatedAt = DateTime.UtcNow },
            new Subject { Id = 5, Name = "История", Description = "История для школьников и студентов", CreatedAt = DateTime.UtcNow },
            new Subject { Id = 6, Name = "Литература", Description = "Литература для школьников и студентов", CreatedAt = DateTime.UtcNow },
            new Subject { Id = 7, Name = "Русский язык", Description = "Русский язык для школьников и студентов", CreatedAt = DateTime.UtcNow },
            new Subject { Id = 8, Name = "Английский язык", Description = "Английский язык для школьников и студентов", CreatedAt = DateTime.UtcNow },
            new Subject { Id = 9, Name = "Информатика", Description = "Информатика для школьников и студентов", CreatedAt = DateTime.UtcNow },
            new Subject { Id = 10, Name = "Обществознание", Description = "Обществознание для школьников и студентов", CreatedAt = DateTime.UtcNow }
        );
    }
} 