using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Vibetech.Educat.DataAccess.Data;

namespace Vibetech.Educat.Startup
{
    public class StartupService
    {
        private readonly EducatDbContext _context;
        private readonly ILogger<StartupService> _logger;

        public StartupService(EducatDbContext context, ILogger<StartupService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task ApplyMigrationsAsync()
        {
            _logger.LogInformation("Applying migrations...");
            try
            {
                await _context.Database.MigrateAsync();
                _logger.LogInformation("Migrations applied successfully.");
                
                // Migrate preparation programs to the new format
                await MigratePreparationProgramsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while applying migrations.");
                throw;
            }
        }
        
        private async Task MigratePreparationProgramsAsync()
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
} 