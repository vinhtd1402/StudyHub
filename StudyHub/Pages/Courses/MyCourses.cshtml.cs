using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StudyHub.Data;
using StudyHub.Models;

namespace StudyHub.Pages_Courses
{
    [Authorize(Roles = "Student")]
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

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null) return;

            Courses = await _context.Enrollments
                .Where(e => e.StudentId == user.Id)
                .Include(e => e.Course)
                .ThenInclude(c => c.Teacher)
                .Select(e => e.Course)
                .ToListAsync();
            foreach (var course in Courses)
            {
                TotalLessonsCount[course.Id] = await _context.Lessons
                    .CountAsync(l => l.CourseId == course.Id);

                CompletedLessonsCount[course.Id] = await _context.LessonProgresses
                    .CountAsync(lp =>
                        lp.StudentId == user.Id &&
                        lp.IsCompleted &&
                        lp.Lesson.CourseId == course.Id);
            }
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