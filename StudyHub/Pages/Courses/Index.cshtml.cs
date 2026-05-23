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

namespace StudyHub.Pages_Courses
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly StudyHub.Data.ApplicationDbContext _context;

        public IndexModel(StudyHub.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Course> Course { get; set; } = default!;
        public SelectList TeacherOptions { get; set; } = default!;

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? TeacherId { get; set; }

        public async Task OnGetAsync()
        {
            var coursesQuery = _context.Courses
                .Include(c => c.Teacher)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                coursesQuery = coursesQuery.Where(c =>
                    c.Title.Contains(SearchTerm) ||
                    c.Description.Contains(SearchTerm) ||
                    (c.Teacher != null && c.Teacher.FullName.Contains(SearchTerm)) ||
                    (c.Teacher != null && c.Teacher.Email != null && c.Teacher.Email.Contains(SearchTerm)));
            }

            if (!string.IsNullOrWhiteSpace(TeacherId))
            {
                coursesQuery = coursesQuery.Where(c => c.TeacherId == TeacherId);
            }

            Course = await coursesQuery
                .OrderBy(c => c.Title)
                .ToListAsync();

            var teachers = await _context.Users
                .Where(u => _context.Courses.Any(c => c.TeacherId == u.Id))
                .OrderBy(u => u.FullName)
                .Select(u => new
                {
                    u.Id,
                    Name = string.IsNullOrWhiteSpace(u.FullName) ? u.Email : u.FullName
                })
                .ToListAsync();

            TeacherOptions = new SelectList(teachers, "Id", "Name", TeacherId);
        }
    }
}
