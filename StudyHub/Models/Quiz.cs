using StudyHub.Models;

public class Quiz
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public int LessonId { get; set; }

    public Lesson Lesson { get; set; } = null!;

    public ICollection<Question> Questions { get; set; } = new List<Question>();
}