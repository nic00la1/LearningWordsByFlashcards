using System.Text.Json;

namespace LearningWordsByFlashcards;

public partial class SettingsPage : ContentPage
{
    private Settings settings;
    public MainPage MainPage { get; set; }

    public SettingsPage(Settings settings, int maxFlashcards)
    {
        InitializeComponent();
        this.settings = settings;
        difficultyModePicker.SelectedItem =
            settings.DifficultyMode ?? string.Empty;
        showDescriptionSwitch.IsToggled = settings.ShowDescription;
        useDefaultNumberOfFlashcardsSwitch.IsToggled =
            settings.UseDefaultNumberOfFlashcards;
        wordDifficultyLevelsEntry.Text = string.Join(",",
            settings.WordDifficultyLevels ?? new List<string>());
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        SaveSettings();
        MainPage.ApplySettings(); // Zastosuj nowe ustawienia w MainPage
        await Navigation.PopAsync();
    }

    private void SaveSettings()
    {
        settings.DifficultyMode =
            difficultyModePicker.SelectedItem?.ToString() ?? string.Empty;
        settings.ShowDescription = showDescriptionSwitch.IsToggled;
        settings.UseDefaultNumberOfFlashcards =
            useDefaultNumberOfFlashcardsSwitch.IsToggled;
        settings.WordDifficultyLevels = wordDifficultyLevelsEntry.Text
            .Split(',').Select(s => s.Trim()).ToList();

        // Save settings
        string filePath =
            Path.Combine(FileSystem.AppDataDirectory, "settings.json");
        string json = JsonSerializer.Serialize(settings);
        File.WriteAllText(filePath, json);
    }
}
