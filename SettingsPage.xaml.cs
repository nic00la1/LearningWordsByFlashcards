using System.Text.Json;

namespace LearningWordsByFlashcards;

public partial class SettingsPage : ContentPage
{
    private Settings settings;

    public SettingsPage(Settings settings, int maxFlashcards)
    {
        InitializeComponent();
        this.settings = settings;
        difficultyModePicker.SelectedItem =
            settings.DifficultyMode ?? string.Empty;
        showDescriptionSwitch.IsToggled = settings.ShowDescription;
        numberOfFlashcardsSlider.Minimum = 1;
        numberOfFlashcardsSlider.Maximum = maxFlashcards;
        numberOfFlashcardsSlider.Value = settings.NumberOfFlashcards;
        sliderValueLabel.Text = settings.NumberOfFlashcards.ToString();
        useDefaultNumberOfFlashcardsSwitch.IsToggled =
            settings.UseDefaultNumberOfFlashcards;
        wordDifficultyLevelsEntry.Text = string.Join(",",
            settings.WordDifficultyLevels ?? new List<string>());
        flashcardsFileNameEntry.Text = settings.FlashcardsFileName;
        flashcardsFileNameEntry.Placeholder =
            settings.FlashcardsFileName; // Set the placeholder
    }

    private void OnSliderValueChanged(object sender, ValueChangedEventArgs e)
    {
        sliderValueLabel.Text = ((int)e.NewValue).ToString();
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        SaveSettings();
        await Navigation.PopAsync();
    }

    private void SaveSettings()
    {
        settings.DifficultyMode =
            difficultyModePicker.SelectedItem?.ToString() ?? string.Empty;
        settings.ShowDescription = showDescriptionSwitch.IsToggled;
        settings.NumberOfFlashcards = (int)numberOfFlashcardsSlider.Value;
        settings.UseDefaultNumberOfFlashcards =
            useDefaultNumberOfFlashcardsSwitch.IsToggled;
        settings.WordDifficultyLevels = wordDifficultyLevelsEntry.Text
            .Split(',').Select(s => s.Trim()).ToList();
        settings.FlashcardsFileName = flashcardsFileNameEntry.Text;

        // Save settings
        string filePath =
            Path.Combine(FileSystem.AppDataDirectory, "settings.json");
        string json = JsonSerializer.Serialize(settings);
        File.WriteAllText(filePath, json);
    }
}
