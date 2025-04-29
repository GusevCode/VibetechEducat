using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Vibetech.Educat.Domain.Services;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Authorization;
using Vibetech.Educat.DataAccess.Data;
using Vibetech.Educat.Common.Interfaces;
using Vibetech.Educat.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Vibetech.Educat.Common.Models;
using Vibetech.Educat.Services.Services.TeacherService;
using Vibetech.Educat.Utilities;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// Add localization
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// Configure culture settings
var supportedCultures = new[] { "ru-RU" };
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("ru-RU");
    options.SupportedCultures = supportedCultures.Select(c => new CultureInfo(c)).ToList();
    options.SupportedUICultures = options.SupportedCultures;
});

// Add services to the container.
builder.Services.AddRazorPages(options =>
{
    // Add global authorization filter
    options.Conventions.AuthorizeFolder("/");
    // Configure exceptions for public pages
    options.Conventions.AllowAnonymousToPage("/Account/Login");
    options.Conventions.AllowAnonymousToPage("/Account/Register");
    options.Conventions.AllowAnonymousToPage("/Account/AccessDenied");
    options.Conventions.AllowAnonymousToPage("/Error");
    options.Conventions.AllowAnonymousToPage("/Index");
    options.Conventions.AllowAnonymousToPage("/Search/Tutors");
    options.Conventions.AllowAnonymousToPage("/Search/Subjects");
    options.Conventions.AllowAnonymousToPage("/About");
});

// Configure model binding error messages
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        return new BadRequestObjectResult(context.ModelState);
    };
});

// Add custom ModelBinder for date formats
builder.Services.Configure<Microsoft.AspNetCore.Mvc.ModelBinding.Binders.SimpleTypeModelBinderProvider>(options =>
{
    // Configure error messages for date conversion errors
    builder.Services.Configure<MvcOptions>(options =>
    {
        options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor(
            (x, y) => $"Значение '{x}' некорректно для поля {y}.");
            
        options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(
            x => $"Поле {x} обязательно для заполнения.");
            
        options.ModelBindingMessageProvider.SetMissingBindRequiredValueAccessor(
            x => $"Поле {x} обязательно для заполнения.");
            
        options.ModelBindingMessageProvider.SetValueIsInvalidAccessor(
            (x) => $"Значение некорректно.");
    });
});

// Add DbContext
builder.Services.AddDbContext<EducatDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Добавляем сервисы Identity
builder.Services.AddIdentity<User, IdentityRole<int>>(options => {
    // Настройки пароля
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 1;
    options.Password.RequiredUniqueChars = 1;

    // Настройки блокировки
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // Настройки пользователя
    options.User.AllowedUserNameCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
})
.AddEntityFrameworkStores<EducatDbContext>()
.AddDefaultTokenProviders();

// Настраиваем cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
});

// Add Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Add services
builder.Services.AddScoped<IStorageService, DatabaseStorageService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITeacherService, TeacherService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();

// Add session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Register the PreparationProgramMigrator
builder.Services.AddScoped<PreparationProgramMigrator>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Apply database migrations
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<EducatDbContext>();
        context.Database.Migrate();
        
        // Migrate preparation programs
        var migrator = services.GetRequiredService<PreparationProgramMigrator>();
        Task.Run(() => migrator.MigrateAsync()).GetAwaiter().GetResult();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database or preparation programs.");
    }
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRequestLocalization();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapRazorPages();

// Add root path routing
app.MapGet("/", () => Results.Redirect("/Index"));

app.Run();