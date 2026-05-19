using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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

        public IList<Course> Course { get;set; } = default!;

        public async Task OnGetAsync()
        {
            Course = await _context.Courses
                .Include(c => c.Teacher).ToListAsync();
        }
    }
}
