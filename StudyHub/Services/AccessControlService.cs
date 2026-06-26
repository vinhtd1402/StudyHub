using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudyHub.Data.Repositories;
using StudyHub.Models;

namespace StudyHub.Services
{
    public class AccessControlService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccessControlService(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<bool> TeacherOwnsCourseAsync(string teacherId, int courseId)
        {
            return await _unitOfWork.Courses.AnyAsync(c =>
                c.Id == courseId &&
                c.TeacherId == teacherId);
        }

        public async Task<bool> TeacherOwnsLessonAsync(string teacherId, int lessonId)
        {
            return await _unitOfWork.Lessons.Query()
                .Include(l => l.Course)
                .AnyAsync(l =>
                    l.Id == lessonId &&
                    l.Course != null &&
                    l.Course.TeacherId == teacherId);
        }

        public async Task<bool> StudentCanViewLessonAsync(string studentId, int lessonId)
        {
            var lesson = await _unitOfWork.Lessons.Query()
                .Where(l => l.Id == lessonId)
                .Select(l => new { l.CourseId })
                .FirstOrDefaultAsync();

            if (lesson == null)
            {
                return false;
            }

            return await _unitOfWork.Enrollments.AnyAsync(e =>
                e.StudentId == studentId &&
                e.CourseId == lesson.CourseId);
        }

        public async Task<bool> IsUserInRoleAsync(ApplicationUser user, string role)
        {
            return await _userManager.IsInRoleAsync(user, role);
        }
    }
}
