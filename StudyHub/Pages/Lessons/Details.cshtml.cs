using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StudyHub.Data;
using StudyHub.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StudyHub.Models;
namespace StudyHub.Pages_Lessons
{
    public class DetailsModel : PageModel
    {
        private readonly StudyHub.Data.ApplicationDbContext _context;
        public Lesson? PreviousLesson { get; set; }
        public Lesson? NextLesson { get; set; }
        private readonly UserManager<ApplicationUser> _userManager;

        public DetailsModel(
    ApplicationDbContext context,
    UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public Lesson Lesson { get; set; } = default!;
        public List<QuizResultViewModel> QuizResults { get; set; } = new();
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lesson = await _context.Lessons
                .Include(l => l.Quizzes)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (lesson == null)
            {
                return NotFound();
            }

            Lesson = lesson;

            PreviousLesson = await _context.Lessons
                .Where(l => l.CourseId == lesson.CourseId && l.Id < lesson.Id)
                .OrderByDescending(l => l.Id)
                .FirstOrDefaultAsync();

            NextLesson = await _context.Lessons
                .Where(l => l.CourseId == lesson.CourseId && l.Id > lesson.Id)
                .OrderBy(l => l.Id)
                .FirstOrDefaultAsync();

            if (User.IsInRole("Teacher"))
            {
                QuizResults = await _context.QuizAttempts
                    .Include(x => x.User)
                    .Include(x => x.Quiz)
                    .Where(x => x.Quiz.LessonId == Lesson.Id)
                    .Select(x => new QuizResultViewModel
                    {
                        StudentName = x.User.UserName,
                        Score = x.Score
                    })
                    .ToListAsync();
            }

            return Page();
        }
        public async Task<IActionResult> OnPostCompleteAsync(int? id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var exists = await _context.LessonProgresses
                .AnyAsync(lp => lp.LessonId == id && lp.StudentId == user.Id);

            if (!exists)
            {
                _context.LessonProgresses.Add(new LessonProgress
                {
                    LessonId = id.Value,
                    StudentId = user.Id,
                    IsCompleted = true,
                    CompletedAt = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
            }

            return RedirectToPage(new { id });
        }
    }
}
