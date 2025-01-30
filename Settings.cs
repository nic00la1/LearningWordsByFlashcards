using System.Text.Json;

namespace LearningWordsByFlashcards;

public class Settings
{
    public string DifficultyMode { get; set; }
    public bool ShowDescription { get; set; }
    public List<string> WordDifficultyLevels { get; set; }
    public string FlashcardsFileName { get; set; }
    public int NumberOfFlashcards { get; set; }
    public bool UseDefaultNumberOfFlashcards { get; set; }
}
