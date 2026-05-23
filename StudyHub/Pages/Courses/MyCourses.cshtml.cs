using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StudyHub.Data;
using StudyHub.Models;

namespace StudyHub.Pages_Courses
{
    [Authorize(Roles = "Student,Teacher")]
    public class MyCoursesModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public Dictionary<int, int> CompletedLessonsCount { get; set; } = new();
        public Dictionary<int, int> TotalLessonsCount { get; set; } = new();

        public MyCoursesModel(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IList<Course> Courses { get; set; } = new List<Course>();

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string StatusFilter { get; set; } = "all";

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null) return;

            if (User.IsInRole("Student"))
            {
                Courses = await _context.Enrollments
                    .Where(e => e.StudentId == user.Id)
                    .Include(e => e.Course)
                    .ThenInclude(c => c.Teacher)
                    .Where(e => e.Course != null)
                    .Select(e => e.Course!)
                    .ToListAsync();
            }
            else if (User.IsInRole("Teacher"))
            {
                Courses = await _context.Courses
                    .Where(c => c.TeacherId == user.Id)
                    .Include(c => c.Teacher)
                    .ToListAsync();
            }

            await LoadLessonCountsAsync(user.Id);
            ApplyFilters();
        }

        private async Task LoadLessonCountsAsync(string userId)
        {
            foreach (var course in Courses)
            {
                TotalLessonsCount[course.Id] = await _context.Lessons
                    .CountAsync(l => l.CourseId == course.Id);

                CompletedLessonsCount[course.Id] = await _context.LessonProgresses
                    .Include(lp => lp.Lesson)
                    .CountAsync(lp =>
                        lp.StudentId == userId &&
                        lp.IsCompleted &&
                        lp.Lesson.CourseId == course.Id);
            }
        }

        private void ApplyFilters()
        {
            var filteredCourses = Courses.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                filteredCourses = filteredCourses.Where(course =>
                    course.Title.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    course.Description.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (course.Teacher?.FullName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (course.Teacher?.Email?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false));
            }

            if (User.IsInRole("Student"))
            {
                filteredCourses = StatusFilter switch
                {
                    "completed" => filteredCourses.Where(IsCompleted),
                    "in-progress" => filteredCourses.Where(IsInProgress),
                    "not-started" => filteredCourses.Where(IsNotStarted),
                    _ => filteredCourses
                };
            }

            Courses = filteredCourses
                .OrderBy(course => course.Title)
                .ToList();
        }

        private bool IsCompleted(Course course)
        {
            var total = TotalLessonsCount.GetValueOrDefault(course.Id);
            var completed = CompletedLessonsCount.GetValueOrDefault(course.Id);

            return total > 0 && completed == total;
        }

        private bool IsInProgress(Course course)
        {
            var total = TotalLessonsCount.GetValueOrDefault(course.Id);
            var completed = CompletedLessonsCount.GetValueOrDefault(course.Id);

            return total > 0 && completed > 0 && completed < total;
        }

        private bool IsNotStarted(Course course)
        {
            return CompletedLessonsCount.GetValueOrDefault(course.Id) == 0;
        }
        public async Task<IActionResult> OnGetContinueAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null) return Challenge();

            var lessons = await _context.Lessons
                .Where(l => l.CourseId == id)
                .OrderBy(l => l.Id)
                .ToListAsync();

            foreach (var lesson in lessons)
            {
                var completed = await _context.LessonProgresses.AnyAsync(lp =>
                    lp.StudentId == user.Id &&
                    lp.LessonId == lesson.Id &&
                    lp.IsCompleted);

                if (!completed)
                {
                    return RedirectToPage("/Lessons/Details", new { id = lesson.Id });
                }
            }

            // all completed -> open first lesson
            var firstLesson = lessons.FirstOrDefault();

            if (firstLesson != null)
            {
                return RedirectToPage("/Lessons/Details", new { id = firstLesson.Id });
            }

            return RedirectToPage();
        }

    }
}
