using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Vibetech.Educat.Common.Models;
using Vibetech.Educat.DataAccess.Data;

namespace Vibetech.Educat.Utilities;

public class PreparationProgramMigrator
{
    private readonly EducatDbContext _context;
    private readonly ILogger<PreparationProgramMigrator> _logger;

    public PreparationProgramMigrator(EducatDbContext context, ILogger<PreparationProgramMigrator> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task MigrateAsync()
    {
        _logger.LogInformation("Migrating preparation programs to the new format...");
        try
        {
            var teacherProfiles = await _context.TeacherProfiles.ToListAsync();
            var updatedProfiles = false;
            
            foreach (var profile in teacherProfiles)
            {
                var originalPrograms = profile.PreparationPrograms;
                var newPrograms = new List<string>();
                
                foreach (var program in originalPrograms)
                {
                    switch (program)
                    {
                        case "OGE":
                            newPrograms.Add("ОГЭ");
                            break;
                        case "EGE":
                            newPrograms.Add("ЕГЭ");
                            break;
                        case "Olympiad":
                            newPrograms.Add("Олимпиады");
                            break;
                        default:
                            // Keep other values as they are
                            newPrograms.Add(program);
                            break;
                    }
                }
                
                // Update only if there were changes
                if (!Enumerable.SequenceEqual(originalPrograms, newPrograms.ToArray()))
                {
                    profile.PreparationPrograms = newPrograms.ToArray();
                    updatedProfiles = true;
                }
            }
            
            if (updatedProfiles)
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Preparation programs migrated successfully.");
            }
            else
            {
                _logger.LogInformation("No preparation programs needed migration.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while migrating preparation programs.");
        }
    }
} 