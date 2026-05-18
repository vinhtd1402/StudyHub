using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StudyHub.Data;
using StudyHub.Models;

namespace StudyHub.Pages_Lessons
{
    public class DetailsModel : PageModel
    {
        private readonly StudyHub.Data.ApplicationDbContext _context;
        public Lesson? PreviousLesson { get; set; }
        public Lesson? NextLesson { get; set; }

        public DetailsModel(StudyHub.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public Lesson Lesson { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lesson = await _context.Lessons
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

            return Page();
        }
    }
}
