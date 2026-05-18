namespace StudyHub.Models
{
    public class LessonProgress
    {
        public int Id { get; set; }

        public int LessonId { get; set; }
        public Lesson Lesson { get; set; }

        public string StudentId { get; set; }
        public ApplicationUser Student { get; set; }

        public bool IsCompleted { get; set; }

        public DateTime CompletedAt { get; set; }
    }
}