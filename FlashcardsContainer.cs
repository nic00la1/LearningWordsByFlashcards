using System.Text.Json.Serialization;

public class FlashcardsContainer
{
    [JsonPropertyName("flashcards")]
    public List<Flashcard> Flashcards { get; set; }
}
