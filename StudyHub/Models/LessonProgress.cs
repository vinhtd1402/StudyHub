namespace StudyHub.Models
{
    public class LessonProgress
    {
        public int Id { get; set; }

        public int LessonId { get; set; }
        public Lesson Lesson { get; set; } = default!;

        public string StudentId { get; set; } = string.Empty;
        public ApplicationUser Student { get; set; } = default!;

        public bool IsCompleted { get; set; }

        public DateTime CompletedAt { get; set; }
    }
}
