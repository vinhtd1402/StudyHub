using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudyHub.Data;
using StudyHub.Data.Repositories;
using StudyHub.Models;
using StudyHub.Services;

namespace StudyHub
{
    public class Program
    {
        private const string AdminRole = "Admin";
        private const string TeacherRole = "Teacher";
        private const string StudentRole = "Student";

        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string not found.");

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultUI()
            .AddDefaultTokenProviders();

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy => policy.RequireRole(AdminRole));
                options.AddPolicy("TeacherOnly", policy => policy.RequireRole(TeacherRole));
                options.AddPolicy("StudentOnly", policy => policy.RequireRole(StudentRole));
                options.AddPolicy("LearningUser", policy => policy.RequireRole(StudentRole, TeacherRole));
            });

            builder.Services.Configure<VietQrOptions>(
                builder.Configuration.GetSection("VietQr"));

            builder.Services.Configure<EmailOptions>(
                builder.Configuration.GetSection("Email"));

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddSingleton<VietQrPaymentService>();
            builder.Services.AddScoped<AccessControlService>();
            builder.Services.AddScoped<CourseService>();
            builder.Services.AddScoped<LessonService>();
            builder.Services.AddScoped<WalletService>();
            builder.Services.AddScoped<QuizService>();
            builder.Services.AddScoped<StudyHubEmailSender>();
            builder.Services.AddScoped<TeacherBillingService>();
            builder.Services.AddHostedService<TeacherBillingBackgroundService>();

            builder.Services.AddRazorPages();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapRazorPages();

            if (app.Environment.IsDevelopment())
            {
                await SeedDevelopmentIdentityAsync(app.Services);
            }

            await app.RunAsync();
        }

        private static async Task SeedDevelopmentIdentityAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            foreach (var role in new[] { AdminRole, TeacherRole, StudentRole })
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            await EnsureUserInRoleAsync(
                userManager,
                email: "admin@studyhub.local",
                password: "123456Aa@",
                fullName: "StudyHub Admin",
                role: AdminRole);

            await EnsureUserInRoleAsync(
                userManager,
                email: "vinh@gmail.com",
                password: "123456Aa@",
                fullName: "StudyHub Teacher",
                role: TeacherRole);

            await EnsureUserInRoleAsync(
                userManager,
                email: "student@studyhub.local",
                password: "123456Aa@",
                fullName: "StudyHub Student",
                role: StudentRole);
        }

        private static async Task EnsureUserInRoleAsync(
            UserManager<ApplicationUser> userManager,
            string email,
            string password,
            string fullName,
            string role)
        {
            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = fullName,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                var createResult = await userManager.CreateAsync(user, password);

                if (!createResult.Succeeded)
                {
                    var errors = string.Join(", ", createResult.Errors.Select(error => error.Description));
                    throw new InvalidOperationException($"Could not create development user '{email}': {errors}");
                }
            }

            if (!await userManager.IsInRoleAsync(user, role))
            {
                var roleResult = await userManager.AddToRoleAsync(user, role);

                if (!roleResult.Succeeded)
                {
                    var errors = string.Join(", ", roleResult.Errors.Select(error => error.Description));
                    throw new InvalidOperationException($"Could not add '{email}' to '{role}' role: {errors}");
                }
            }

            if (role == TeacherRole &&
                (user.TeacherBillingStartsAt == null || user.NextTeacherBillingAt == null))
            {
                user.TeacherBillingStartsAt ??= user.CreatedAt.AddDays(30);
                user.NextTeacherBillingAt ??= user.TeacherBillingStartsAt;
                await userManager.UpdateAsync(user);
            }
        }
    }
}
