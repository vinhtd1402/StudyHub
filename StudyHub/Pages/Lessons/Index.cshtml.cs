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
    public class IndexModel : PageModel
    {
        private readonly StudyHub.Data.ApplicationDbContext _context;

        public IndexModel(StudyHub.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Lesson> Lesson { get;set; } = default!;

        public async Task OnGetAsync()
        {
            Lesson = await _context.Lessons
                .Include(l => l.Course).ToListAsync();
        }
    }
}
