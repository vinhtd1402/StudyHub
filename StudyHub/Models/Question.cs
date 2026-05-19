public class Question
{
    public int Id { get; set; }

    public string Content { get; set; } = string.Empty;

    public int QuizId { get; set; }

    public Quiz Quiz { get; set; } = null!;

    public ICollection<Answer> Answers { get; set; } = new List<Answer>();
}