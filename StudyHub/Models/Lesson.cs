using StudyHub.Models;
using System.ComponentModel.DataAnnotations;

public class Lesson
{
    public int Id { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;

    public string? Content { get; set; }

    public string? YoutubeUrl { get; set; }

    public int CourseId { get; set; }

    public Course? Course { get; set; }

    public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();
}