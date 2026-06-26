using System.ComponentModel.DataAnnotations;

namespace StudyHub.Models
{
    public class Question
    {
        public int Id { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        [Required]
        public string OptionA { get; set; } = string.Empty;

        [Required]
        public string OptionB { get; set; } = string.Empty;

        [Required]
        public string OptionC { get; set; } = string.Empty;

        [Required]
        public string OptionD { get; set; } = string.Empty;

        [Required]
        public string CorrectAnswer { get; set; } = "A";

        public int QuizId { get; set; }

        public Quiz? Quiz { get; set; }
    }
}