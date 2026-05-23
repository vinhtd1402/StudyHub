using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudyHub.Data;
using StudyHub.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using StudyHub.Services;
namespace StudyHub.Pages_Lessons
{
    [Authorize(Roles = "Teacher")]
    public class EditModel : PageModel
    {
        private readonly StudyHub.Data.ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AccessControlService _accessControlService;

        public EditModel(
            StudyHub.Data.ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            AccessControlService accessControlService)
        {
            _context = context;
            _userManager = userManager;
            _accessControlService = accessControlService;
        }

        [BindProperty]
        public Lesson Lesson { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            if (user.IsTeacherSuspended)
            {
                return Forbid();
            }

            var lesson =  await _context.Lessons
                .Include(l => l.Course)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (lesson == null)
            {
                return NotFound();
            }

            if (!await _accessControlService.TeacherOwnsLessonAsync(user.Id, lesson.Id))
            {
                return Forbid();
            }

            Lesson = lesson;
            await LoadCourseOptionsAsync(user.Id);
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser != null)
                {
                    await LoadCourseOptionsAsync(currentUser.Id);
                }

                return Page();
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            if (user.IsTeacherSuspended)
            {
                return Forbid();
            }

            var existingLesson = await _context.Lessons
                .Include(l => l.Course)
                .FirstOrDefaultAsync(l => l.Id == Lesson.Id);

            if (existingLesson == null)
            {
                return NotFound();
            }

            if (!await _accessControlService.TeacherOwnsLessonAsync(user.Id, existingLesson.Id))
            {
                return Forbid();
            }

            if (!await _accessControlService.TeacherOwnsCourseAsync(user.Id, Lesson.CourseId))
            {
                return Forbid();
            }

            existingLesson.Title = Lesson.Title;
            existingLesson.Content = Lesson.Content;
            existingLesson.YoutubeUrl = Lesson.YoutubeUrl;
            existingLesson.CourseId = Lesson.CourseId;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LessonExists(Lesson.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool LessonExists(int id)
        {
            return _context.Lessons.Any(e => e.Id == id);
        }

        private async Task LoadCourseOptionsAsync(string teacherId)
        {
            var courses = await _context.Courses
                .Where(c => c.TeacherId == teacherId)
                .OrderBy(c => c.Title)
                .ToListAsync();

            ViewData["CourseId"] = new SelectList(courses, "Id", "Title", Lesson.CourseId);
        }
    }
}
