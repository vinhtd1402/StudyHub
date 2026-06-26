using Microsoft.EntityFrameworkCore;
using StudyHub.Data;
using StudyHub.Models;
using StudyHub.Services;

namespace StudyHub.Tests;

public class CourseServiceTests
{
    [Fact]
    public async Task EnrollAsync_WhenStudentHasEnoughBalance_CreatesEnrollmentAndDebitTransaction()
    {
        await using var context = CreateContext();
        var student = new ApplicationUser
        {
            Id = "student-1",
            Email = "student@studyhub.local",
            WalletBalance = 50_000
        };

        context.Users.Add(student);
        context.Courses.Add(new Course
        {
            Id = 1,
            Title = "C# Basics",
            Description = "Intro course",
            Price = 10_000,
            TeacherId = "teacher-1"
        });
        await context.SaveChangesAsync();

        var service = new CourseService(context);
        var result = await service.EnrollAsync(1, student);

        Assert.Equal(CourseEnrollmentResult.Enrolled, result);
        Assert.Equal(40_000, student.WalletBalance);
        Assert.Single(await context.Enrollments.ToListAsync());

        var transaction = Assert.Single(await context.CreditTransactions.ToListAsync());
        Assert.Equal(-10_000, transaction.Amount);
        Assert.Equal(CreditTransactionStatus.Paid, transaction.Status);
        Assert.Equal("StudyHub", transaction.Provider);
    }

    [Fact]
    public async Task EnrollAsync_WhenStudentHasInsufficientBalance_DoesNotCreateEnrollment()
    {
        await using var context = CreateContext();
        var student = new ApplicationUser
        {
            Id = "student-1",
            Email = "student@studyhub.local",
            WalletBalance = 5_000
        };

        context.Users.Add(student);
        context.Courses.Add(new Course
        {
            Id = 1,
            Title = "Paid Course",
            Description = "Costs more than the wallet balance",
            Price = 10_000,
            TeacherId = "teacher-1"
        });
        await context.SaveChangesAsync();

        var service = new CourseService(context);
        var result = await service.EnrollAsync(1, student);

        Assert.Equal(CourseEnrollmentResult.InsufficientBalance, result);
        Assert.Equal(5_000, student.WalletBalance);
        Assert.Empty(await context.Enrollments.ToListAsync());
        Assert.Empty(await context.CreditTransactions.ToListAsync());
    }

    [Fact]
    public async Task EnrollAsync_WhenAlreadyEnrolled_DoesNotCreateDuplicateEnrollment()
    {
        await using var context = CreateContext();
        var student = new ApplicationUser
        {
            Id = "student-1",
            Email = "student@studyhub.local",
            WalletBalance = 50_000
        };

        context.Users.Add(student);
        context.Courses.Add(new Course
        {
            Id = 1,
            Title = "Free Course",
            Description = "Already enrolled",
            Price = 0,
            TeacherId = "teacher-1"
        });
        context.Enrollments.Add(new Enrollment
        {
            CourseId = 1,
            StudentId = student.Id,
            PricePaid = 0
        });
        await context.SaveChangesAsync();

        var service = new CourseService(context);
        var result = await service.EnrollAsync(1, student);

        Assert.Equal(CourseEnrollmentResult.AlreadyEnrolled, result);
        Assert.Single(await context.Enrollments.ToListAsync());
        Assert.Empty(await context.CreditTransactions.ToListAsync());
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
