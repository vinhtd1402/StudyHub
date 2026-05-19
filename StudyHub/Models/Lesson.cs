using System.ComponentModel.DataAnnotations;

namespace StudyHub.Models
{
    public class Lesson
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public int CourseId { get; set; }
        public Course? Course { get; set; }
        public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();
    }
}