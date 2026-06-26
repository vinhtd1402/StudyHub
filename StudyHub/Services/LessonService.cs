using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudyHub.Data;
using StudyHub.Models;

namespace StudyHub.Services
{
    public enum LessonAccessResult
    {
        Allowed,
        NotFound,
        Forbidden,
        EnrollmentRequired
    }

    public class LessonDetailsData
    {
        public Lesson Lesson { get; set; } = default!;
        public Lesson? PreviousLesson { get; set; }
        public Lesson? NextLesson { get; set; }
        public List<QuizResultViewModel> QuizResults { get; set; } = new();
    }

    public class LessonService
    {
        private readonly ApplicationDbContext _context;
        private readonly AccessControlService _accessControlService;

        public LessonService(
            ApplicationDbContext context,
            AccessControlService accessControlService)
        {
            _context = context;
            _accessControlService = accessControlService;
        }

        public async Task<IList<Lesson>> GetLessonsAsync(
            string? userId,
            bool isAdmin,
            bool isTeacher,
            string? searchTerm,
            int? courseId)
        {
            var lessonsQuery = _context.Lessons
                .Include(l => l.Course)
                .AsQueryable();

            if (isAdmin)
            {
                // Admin can see all lessons.
            }
            else if (isTeacher)
            {
                lessonsQuery = lessonsQuery.Where(l =>
                    l.Course != null &&
                    l.Course.TeacherId == userId);
            }
            else
            {
                lessonsQuery = lessonsQuery.Where(l =>
                    _context.Enrollments.Any(e =>
                        e.StudentId == userId &&
                        e.CourseId == l.CourseId));
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                lessonsQuery = lessonsQuery.Where(l =>
                    l.Title.Contains(searchTerm) ||
                    (l.Content != null && l.Content.Contains(searchTerm)) ||
                    (l.Course != null && l.Course.Title.Contains(searchTerm)));
            }

            if (courseId.HasValue)
            {
                lessonsQuery = lessonsQuery.Where(l => l.CourseId == courseId.Value);
            }

            return await lessonsQuery
                .OrderBy(l => l.Course != null ? l.Course.Title : string.Empty)
                .ThenBy(l => l.Id)
                .ToListAsync();
        }

        public async Task<SelectList> BuildCourseOptionsAsync(
            string? userId,
            bool isAdmin,
            bool isTeacher,
            int? selectedCourseId)
        {
            IQueryable<Course> coursesQuery;

            if (isAdmin)
            {
                coursesQuery = _context.Courses;
            }
            else if (isTeacher)
            {
                coursesQuery = _context.Courses.Where(c => c.TeacherId == userId);
            }
            else
            {
                var enrolledCourseIds = _context.Enrollments
                    .Where(e => e.StudentId == userId)
                    .Select(e => e.CourseId);

                coursesQuery = _context.Courses.Where(c => enrolledCourseIds.Contains(c.Id));
            }

            var courses = await coursesQuery
                .OrderBy(c => c.Title)
                .Select(c => new { c.Id, c.Title })
                .ToListAsync();

            return new SelectList(courses, "Id", "Title", selectedCourseId);
        }

        public async Task<(LessonAccessResult Result, LessonDetailsData? Data)> GetLessonDetailsAsync(
            int id,
            string? userId,
            bool isAdmin,
            bool isTeacher)
        {
            var lesson = await _context.Lessons
                .Include(l => l.Course)
                .Include(l => l.Quizzes)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (lesson == null)
            {
                return (LessonAccessResult.NotFound, null);
            }

            if (isTeacher)
            {
                if (userId == null ||
                    !await _accessControlService.TeacherOwnsLessonAsync(userId, lesson.Id))
                {
                    return (LessonAccessResult.Forbidden, null);
                }
            }
            else if (!isAdmin)
            {
                if (userId == null ||
                    !await _accessControlService.StudentCanViewLessonAsync(userId, lesson.Id))
                {
                    return (LessonAccessResult.EnrollmentRequired, new LessonDetailsData { Lesson = lesson });
                }
            }

            var data = new LessonDetailsData
            {
                Lesson = lesson,
                PreviousLesson = await _context.Lessons
                    .Where(l => l.CourseId == lesson.CourseId && l.Id < lesson.Id)
                    .OrderByDescending(l => l.Id)
                    .FirstOrDefaultAsync(),
                NextLesson = await _context.Lessons
                    .Where(l => l.CourseId == lesson.CourseId && l.Id > lesson.Id)
                    .OrderBy(l => l.Id)
                    .FirstOrDefaultAsync()
            };

            if (isTeacher)
            {
                data.QuizResults = await _context.QuizAttempts
                    .Include(x => x.User)
                    .Include(x => x.Quiz)
                    .Where(x => x.Quiz.LessonId == lesson.Id)
                    .Select(x => new QuizResultViewModel
                    {
                        StudentName = x.User != null ? x.User.FullName : string.Empty,
                        Score = x.Score
                    })
                    .ToListAsync();
            }

            return (LessonAccessResult.Allowed, data);
        }

        public async Task<bool> MarkCompletedAsync(int lessonId, string studentId)
        {
            if (!await _accessControlService.StudentCanViewLessonAsync(studentId, lessonId))
            {
                return false;
            }

            var exists = await _context.LessonProgresses
                .AnyAsync(lp => lp.LessonId == lessonId && lp.StudentId == studentId);

            if (!exists)
            {
                _context.LessonProgresses.Add(new LessonProgress
                {
                    LessonId = lessonId,
                    StudentId = studentId,
                    IsCompleted = true,
                    CompletedAt = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
            }

            return true;
        }

        public async Task<Lesson> CreateLessonAsync(Lesson lesson)
        {
            _context.Lessons.Add(lesson);
            await _context.SaveChangesAsync();

            return lesson;
        }

        public async Task<Lesson?> GetTeacherLessonAsync(int id, string teacherId)
        {
            var lesson = await _context.Lessons
                .Include(l => l.Course)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lesson == null ||
                !await _accessControlService.TeacherOwnsLessonAsync(teacherId, lesson.Id))
            {
                return null;
            }

            return lesson;
        }

        public async Task<bool?> UpdateLessonAsync(Lesson input, string teacherId)
        {
            var lesson = await _context.Lessons
                .Include(l => l.Course)
                .FirstOrDefaultAsync(l => l.Id == input.Id);

            if (lesson == null)
            {
                return null;
            }

            if (!await _accessControlService.TeacherOwnsLessonAsync(teacherId, lesson.Id) ||
                !await _accessControlService.TeacherOwnsCourseAsync(teacherId, input.CourseId))
            {
                return false;
            }

            lesson.Title = input.Title;
            lesson.Content = input.Content;
            lesson.YoutubeUrl = input.YoutubeUrl;
            lesson.CourseId = input.CourseId;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool?> DeleteLessonAsync(int id, string teacherId)
        {
            var lesson = await _context.Lessons
                .Include(l => l.Course)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lesson == null)
            {
                return null;
            }

            if (!await _accessControlService.TeacherOwnsLessonAsync(teacherId, lesson.Id))
            {
                return false;
            }

            _context.Lessons.Remove(lesson);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
