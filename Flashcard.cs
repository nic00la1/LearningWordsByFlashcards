using System.Text.Json.Serialization;

public class Flashcard
{
    [JsonPropertyName("difficulty")] public string Difficulty { get; set; }

    [JsonPropertyName("PL")] public List<string> PL { get; set; }

    [JsonPropertyName("ENG")] public List<string> ENG { get; set; }

    [JsonPropertyName("description")] public string Description { get; set; }
}
