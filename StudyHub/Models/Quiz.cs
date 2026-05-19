using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudyHub.Models
{
    public class Quiz
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public int LessonId { get; set; }

        public Lesson? Lesson { get; set; }

        public int QuestionCount { get; set; }

        public ICollection<Question> Questions { get; set; }
            = new List<Question>();
    }
}