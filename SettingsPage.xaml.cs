using System.Text.Json;

namespace LearningWordsByFlashcards;

public partial class SettingsPage : ContentPage
{
    private Settings settings;

    public SettingsPage(Settings settings)
    {
        InitializeComponent();
        this.settings = settings;
        difficultyModePicker.SelectedItem = settings.DifficultyMode;
        showDescriptionSwitch.IsToggled = settings.ShowDescription;
        numberOfFlashcardsEntry.Text = settings.NumberOfFlashcards.ToString();
        useDefaultNumberOfFlashcardsSwitch.IsToggled =
            settings.UseDefaultNumberOfFlashcards;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        settings.DifficultyMode = difficultyModePicker.SelectedItem.ToString();
        settings.ShowDescription = showDescriptionSwitch.IsToggled;
        settings.NumberOfFlashcards = int.Parse(numberOfFlashcardsEntry.Text);
        settings.UseDefaultNumberOfFlashcards =
            useDefaultNumberOfFlashcardsSwitch.IsToggled;

        // Save settings
        string filePath =
            Path.Combine(FileSystem.AppDataDirectory, "settings.json");
        string json = JsonSerializer.Serialize(settings);
        File.WriteAllText(filePath, json);

        await Navigation.PopAsync();
    }
}
