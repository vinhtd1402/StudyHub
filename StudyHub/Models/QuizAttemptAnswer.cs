using System.ComponentModel.DataAnnotations;

namespace StudyHub.Models
{
    public class QuizAttemptAnswer
    {
        public int Id { get; set; }

        public int QuizAttemptId { get; set; }
        public QuizAttempt QuizAttempt { get; set; } = null!;

        public int QuestionId { get; set; }
        public Question? Question { get; set; }

        [Required]
        public string QuestionContent { get; set; } = string.Empty;

        [Required]
        public string OptionA { get; set; } = string.Empty;

        [Required]
        public string OptionB { get; set; } = string.Empty;

        [Required]
        public string OptionC { get; set; } = string.Empty;

        [Required]
        public string OptionD { get; set; } = string.Empty;

        [StringLength(1)]
        public string? SelectedAnswer { get; set; }

        [Required]
        [StringLength(1)]
        public string CorrectAnswer { get; set; } = string.Empty;

        public bool IsCorrect { get; set; }
    }
}
