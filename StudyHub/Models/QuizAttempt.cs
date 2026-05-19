using System;

namespace StudyHub.Models
{
    public class QuizAttempt
    {
        public int Id { get; set; }

        public int QuizId { get; set; }
        public Quiz Quiz { get; set; } = null!;

        public string UserId { get; set; } = string.Empty;

        public int Score { get; set; }

        public int TotalQuestions { get; set; }

        public DateTime TakenAt { get; set; } = DateTime.Now;
    }
}