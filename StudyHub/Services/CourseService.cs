using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudyHub.Data;
using StudyHub.Models;

namespace StudyHub.Services
{
    public enum CourseEnrollmentResult
    {
        Enrolled,
        AlreadyEnrolled,
        CourseNotFound,
        InsufficientBalance
    }

    public class CourseDetailsData
    {
        public Course Course { get; set; } = default!;
        public bool IsEnrolled { get; set; }
        public IList<ApplicationUser> EnrolledStudents { get; set; } = new List<ApplicationUser>();
    }

    public class MyCoursesData
    {
        public IList<Course> Courses { get; set; } = new List<Course>();
        public Dictionary<int, int> CompletedLessonsCount { get; set; } = new();
        public Dictionary<int, int> TotalLessonsCount { get; set; } = new();
    }

    public class CourseService
    {
        private readonly ApplicationDbContext _context;

        public CourseService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IList<Course>> GetPopularCoursesAsync(int count = 6)
        {
            return await _context.Courses
                .Include(c => c.Teacher)
                .Include(c => c.Enrollments)
                .OrderByDescending(c => c.Enrollments.Count)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IList<Course>> GetCoursesAsync(string? searchTerm, string? teacherId)
        {
            var coursesQuery = _context.Courses
                .Include(c => c.Teacher)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                coursesQuery = coursesQuery.Where(c =>
                    c.Title.Contains(searchTerm) ||
                    c.Description.Contains(searchTerm) ||
                    (c.Teacher != null && c.Teacher.FullName.Contains(searchTerm)) ||
                    (c.Teacher != null && c.Teacher.Email != null && c.Teacher.Email.Contains(searchTerm)));
            }

            if (!string.IsNullOrWhiteSpace(teacherId))
            {
                coursesQuery = coursesQuery.Where(c => c.TeacherId == teacherId);
            }

            return await coursesQuery
                .OrderBy(c => c.Title)
                .ToListAsync();
        }

        public async Task<IList<ApplicationUser>> GetTeachersWithCoursesAsync()
        {
            return await _context.Users
                .Where(u => _context.Courses.Any(c => c.TeacherId == u.Id))
                .OrderBy(u => u.FullName)
                .ToListAsync();
        }

        public async Task<CourseDetailsData?> GetCourseDetailsAsync(int id, string? userId)
        {
            var course = await _context.Courses
                .Include(c => c.Teacher)
                .Include(c => c.Lessons)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
            {
                return null;
            }

            var isEnrolled = !string.IsNullOrWhiteSpace(userId) &&
                await _context.Enrollments
                    .AnyAsync(e => e.CourseId == id && e.StudentId == userId);

            var enrolledStudents = await _context.Enrollments
                .Where(e => e.CourseId == id && e.Student != null)
                .Include(e => e.Student)
                .Select(e => e.Student!)
                .ToListAsync();

            return new CourseDetailsData
            {
                Course = course,
                IsEnrolled = isEnrolled,
                EnrolledStudents = enrolledStudents
            };
        }

        public async Task<CourseEnrollmentResult> EnrollAsync(int courseId, ApplicationUser student)
        {
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null)
            {
                return CourseEnrollmentResult.CourseNotFound;
            }

            var exists = await _context.Enrollments
                .AnyAsync(e => e.CourseId == courseId && e.StudentId == student.Id);

            if (exists)
            {
                return CourseEnrollmentResult.AlreadyEnrolled;
            }

            if (student.WalletBalance < course.Price)
            {
                return CourseEnrollmentResult.InsufficientBalance;
            }

            if (course.Price > 0)
            {
                student.WalletBalance -= course.Price;

                _context.CreditTransactions.Add(new CreditTransaction
                {
                    UserId = student.Id,
                    Amount = -course.Price,
                    OrderId = $"ENR{Guid.NewGuid():N}",
                    RequestId = $"ENR-{Guid.NewGuid():N}",
                    Provider = "StudyHub",
                    Status = CreditTransactionStatus.Paid,
                    Message = $"Enrollment fee for course #{course.Id}: {course.Title}",
                    PaidAt = DateTime.UtcNow
                });
            }

            _context.Enrollments.Add(new Enrollment
            {
                CourseId = courseId,
                StudentId = student.Id,
                PricePaid = course.Price,
                EnrolledAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return CourseEnrollmentResult.Enrolled;
        }

        public async Task UnenrollAsync(int courseId, string studentId)
        {
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.CourseId == courseId && e.StudentId == studentId);

            if (enrollment == null)
            {
                return;
            }

            _context.Enrollments.Remove(enrollment);
            await _context.SaveChangesAsync();
        }

        public async Task<MyCoursesData> GetMyCoursesAsync(
            string userId,
            bool isStudent,
            bool isTeacher,
            string? searchTerm,
            string statusFilter)
        {
            var courses = new List<Course>();

            if (isStudent)
            {
                courses = await _context.Enrollments
                    .Where(e => e.StudentId == userId)
                    .Include(e => e.Course)
                    .ThenInclude(c => c!.Teacher)
                    .Where(e => e.Course != null)
                    .Select(e => e.Course!)
                    .ToListAsync();
            }
            else if (isTeacher)
            {
                courses = await _context.Courses
                    .Where(c => c.TeacherId == userId)
                    .Include(c => c.Teacher)
                    .ToListAsync();
            }

            var data = new MyCoursesData
            {
                Courses = courses
            };

            foreach (var course in courses)
            {
                data.TotalLessonsCount[course.Id] = await _context.Lessons
                    .CountAsync(l => l.CourseId == course.Id);

                data.CompletedLessonsCount[course.Id] = await _context.LessonProgresses
                    .Include(lp => lp.Lesson)
                    .CountAsync(lp =>
                        lp.StudentId == userId &&
                        lp.IsCompleted &&
                        lp.Lesson.CourseId == course.Id);
            }

            data.Courses = ApplyMyCoursesFilters(
                data.Courses,
                data.CompletedLessonsCount,
                data.TotalLessonsCount,
                isStudent,
                searchTerm,
                statusFilter);

            return data;
        }

        public async Task<Lesson?> GetNextLessonToContinueAsync(int courseId, string studentId)
        {
            var lessons = await _context.Lessons
                .Where(l => l.CourseId == courseId)
                .OrderBy(l => l.Id)
                .ToListAsync();

            foreach (var lesson in lessons)
            {
                var completed = await _context.LessonProgresses.AnyAsync(lp =>
                    lp.StudentId == studentId &&
                    lp.LessonId == lesson.Id &&
                    lp.IsCompleted);

                if (!completed)
                {
                    return lesson;
                }
            }

            return lessons.FirstOrDefault();
        }

        public async Task<Course> CreateCourseAsync(Course course, ApplicationUser teacher)
        {
            course.TeacherId = teacher.Id;
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            return course;
        }

        public async Task<Course?> GetTeacherCourseAsync(int id, string teacherId)
        {
            return await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == id && c.TeacherId == teacherId);
        }

        public async Task<bool?> UpdateCourseAsync(Course input, string teacherId)
        {
            var course = await GetTeacherCourseAsync(input.Id, teacherId);

            if (course == null)
            {
                return await CourseExistsAsync(input.Id) ? false : null;
            }

            course.Title = input.Title;
            course.Description = input.Description;
            course.Price = input.Price;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool?> DeleteCourseAsync(int id, string teacherId)
        {
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
            {
                return null;
            }

            if (course.TeacherId != teacherId)
            {
                return false;
            }

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Course?> GetCourseAsync(int id)
        {
            return await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        private async Task<bool> CourseExistsAsync(int id)
        {
            return await _context.Courses.AnyAsync(e => e.Id == id);
        }

        private static IList<Course> ApplyMyCoursesFilters(
            IList<Course> courses,
            IReadOnlyDictionary<int, int> completedLessonsCount,
            IReadOnlyDictionary<int, int> totalLessonsCount,
            bool isStudent,
            string? searchTerm,
            string statusFilter)
        {
            var filteredCourses = courses.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                filteredCourses = filteredCourses.Where(course =>
                    course.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    course.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (course.Teacher?.FullName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (course.Teacher?.Email?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false));
            }

            if (isStudent)
            {
                filteredCourses = statusFilter switch
                {
                    "completed" => filteredCourses.Where(course => IsCompleted(course, completedLessonsCount, totalLessonsCount)),
                    "in-progress" => filteredCourses.Where(course => IsInProgress(course, completedLessonsCount, totalLessonsCount)),
                    "not-started" => filteredCourses.Where(course => completedLessonsCount.GetValueOrDefault(course.Id) == 0),
                    _ => filteredCourses
                };
            }

            return filteredCourses
                .OrderBy(course => course.Title)
                .ToList();
        }

        private static bool IsCompleted(
            Course course,
            IReadOnlyDictionary<int, int> completedLessonsCount,
            IReadOnlyDictionary<int, int> totalLessonsCount)
        {
            var total = totalLessonsCount.GetValueOrDefault(course.Id);
            var completed = completedLessonsCount.GetValueOrDefault(course.Id);

            return total > 0 && completed == total;
        }

        private static bool IsInProgress(
            Course course,
            IReadOnlyDictionary<int, int> completedLessonsCount,
            IReadOnlyDictionary<int, int> totalLessonsCount)
        {
            var total = totalLessonsCount.GetValueOrDefault(course.Id);
            var completed = completedLessonsCount.GetValueOrDefault(course.Id);

            return total > 0 && completed > 0 && completed < total;
        }
    }
}
