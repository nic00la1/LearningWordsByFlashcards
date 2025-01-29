using System.Text.Json;

namespace LearningWordsByFlashcards;

public class Settings
{
    public string Difficulty { get; set; }
    public int NumberOfFlashcards { get; set; }
    public bool ShowDescription { get; set; }
}
