using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Maui.Controls;

namespace LearningWordsByFlashcards;

public partial class MainPage : ContentPage
{
    private List<Flashcard> flashcards = new();
    private Settings settings = new();
    private int currentFlashcardIndex;
    private int correctAnswers;
    private string currentMode;
    private string selectedLanguage;
    private bool isTrainingActive;

    public MainPage()
    {
        InitializeComponent();
        LoadFlashcards();
        LoadSettings();
        resetButton.IsVisible = false; // Ukryj przycisk resetowania na początku
    }

    private void LoadFlashcards()
    {
        try
        {
            Debug.WriteLine("Attempting to load flashcards.json...");
            string filePath = Path.Combine(FileSystem.AppDataDirectory,
                "flashcards.json");
            Debug.WriteLine($"flashcards.json path: {filePath}");

            if (!File.Exists(filePath))
            {
                Debug.WriteLine("flashcards.json file does not exist.");
                flashcards = new List<Flashcard>();
                return;
            }

            using StreamReader reader = new(filePath);
            string json = reader.ReadToEnd();
            Debug.WriteLine($"flashcards.json content: {json}");
            FlashcardsContainer? flashcardsContainer =
                JsonSerializer.Deserialize<FlashcardsContainer>(json);
            if (flashcardsContainer != null &&
                flashcardsContainer.Flashcards != null)
            {
                flashcards = flashcardsContainer.Flashcards;
                Debug.WriteLine("Flashcards successfully loaded.");
                Debug.WriteLine($"Loaded {flashcards.Count} flashcards.");
            } else
            {
                Debug.WriteLine(
                    "Failed to deserialize flashcards.json or flashcards are null.");
                flashcards = new List<Flashcard>();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading flashcards: {ex.Message}");
            flashcards = new List<Flashcard>();
        }
    }

    private void LoadSettings()
    {
        try
        {
            Debug.WriteLine("Attempting to load settings.json...");
            string filePath = Path.Combine(FileSystem.AppDataDirectory,
                "settings.json");
            Debug.WriteLine($"settings.json path: {filePath}");

            if (!File.Exists(filePath))
            {
                Debug.WriteLine("settings.json file does not exist.");
                settings = new Settings();
                return;
            }

            using StreamReader reader = new(filePath);
            string json = reader.ReadToEnd();
            Debug.WriteLine($"settings.json content: {json}");
            settings = JsonSerializer.Deserialize<Settings>(json);
            Debug.WriteLine("Settings successfully loaded.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading settings: {ex.Message}");
            settings = new Settings();
        }
    }

    private void OnStartTrainingClicked(object sender, EventArgs e)
    {
        if (isTrainingActive)
        {
            Debug.WriteLine("Training is already active.");
            return;
        }

        if (flashcards == null || !flashcards.Any())
        {
            Debug.WriteLine("Flashcards are not loaded.");
            return;
        }

        if (modePicker == null)
        {
            Debug.WriteLine("modePicker is not initialized.");
            return;
        }

        if (languagePicker == null || languagePicker.SelectedItem == null)
        {
            DisplayAlert("Wybierz język", "Language not selected.", "Ok");
            return;
        }

        selectedLanguage = languagePicker.SelectedItem.ToString();
        languagePicker.IsVisible = false; // Ukryj Picker po wybraniu języka
        modePicker.IsVisible = false; // Ukryj Picker po wybraniu trybu
        startTrainingButton.IsVisible =
            false; // Ukryj przycisk "Zacznij trening"
        chooseLevelLabel.IsVisible = false; // Ukryj etykietę "Wybierz poziom"
        welcomeLabel.Text = "Trening rozpoczęty"; // Zmień tekst etykiety
        answerEntry.IsVisible = true; // Pokaż pole do wpisywania odpowiedzi
        submitButton.IsVisible = true; // Pokaż przycisk "Zatwierdź"

        currentMode = modePicker.SelectedItem?.ToString();
        if (string.IsNullOrEmpty(currentMode))
        {
            Debug.WriteLine("No mode selected.");
            return;
        }

        currentFlashcardIndex = 0;
        correctAnswers = 0;
        isTrainingActive = true;
        ShowNextFlashcard();
    }

    private void ShowNextFlashcard()
    {
        if (currentFlashcardIndex < settings.NumberOfFlashcards &&
            currentFlashcardIndex < flashcards.Count)
        {
            Flashcard flashcard = flashcards[currentFlashcardIndex];
            flashcardLabel.Text = selectedLanguage == "PL"
                ? flashcard.PL[0]
                : flashcard.ENG[0];
            answerEntry.Text = string.Empty;
            resultLabel.Text = string.Empty;
        } else
        {
            scoreLabel.Text =
                $"Score: {correctAnswers}/{settings.NumberOfFlashcards} ({correctAnswers / (double)settings.NumberOfFlashcards * 100}%)";
            resetButton.IsVisible = true; // Pokaż przycisk resetowania
            welcomeLabel.Text = "Zakończono trening!"; // Zmień tytuł
            flashcardLabel.Text = string.Empty; // Ukryj tekst fiszki
            answerEntry.IsVisible =
                false; // Ukryj pole do wpisywania odpowiedzi
            submitButton.IsVisible = false; // Ukryj przycisk "Zatwierdź"
        }
    }

    private void OnSubmitClicked(object sender, EventArgs e)
    {
        if (!isTrainingActive)
        {
            Debug.WriteLine("Training is not active.");
            return;
        }

        if (currentFlashcardIndex >= flashcards.Count)
        {
            Debug.WriteLine("No more flashcards to show.");
            return;
        }

        Flashcard flashcard = flashcards[currentFlashcardIndex];
        string answer = answerEntry.Text;

        if (selectedLanguage == "PL")
        {
            if (flashcard.ENG.Contains(answer))
            {
                correctAnswers++;
                resultLabel.Text = "Correct!";
            } else
                resultLabel.Text =
                    $"Wrong! Correct answers: {string.Join(", ", flashcard.ENG)}";
        } else if (selectedLanguage == "ENG")
        {
            if (flashcard.PL.Contains(answer))
            {
                correctAnswers++;
                resultLabel.Text = "Correct!";
            } else
                resultLabel.Text =
                    $"Wrong! Correct answers: {string.Join(", ", flashcard.PL)}";
        }

        currentFlashcardIndex++;
        ShowNextFlashcard();
    }

    private void AddNewFlashcard(Flashcard newFlashcard)
    {
        flashcards.Add(newFlashcard);
        SaveFlashcards();
    }

    private void UpdateSettings(Settings newSettings)
    {
        settings = newSettings;
        SaveSettings();
    }

    private void SaveFlashcards()
    {
        try
        {
            string filePath = Path.Combine(FileSystem.AppDataDirectory,
                "flashcards.json");
            FlashcardsContainer flashcardsContainer =
                new() { Flashcards = flashcards };
            string json = JsonSerializer.Serialize(flashcardsContainer);
            File.WriteAllText(filePath, json);
            Debug.WriteLine("Flashcards successfully saved.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error saving flashcards: {ex.Message}");
        }
    }

    private void SaveSettings()
    {
        try
        {
            string filePath = Path.Combine(FileSystem.AppDataDirectory,
                "settings.json");
            string json = JsonSerializer.Serialize(settings);
            File.WriteAllText(filePath, json);
            Debug.WriteLine("Settings successfully saved.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error saving settings: {ex.Message}");
        }
    }

    private void OnResetTrainingClicked(object sender, EventArgs e)
    {
        languagePicker.IsVisible = true;
        modePicker.IsVisible = true;
        startTrainingButton.IsVisible =
            true; // Pokaż przycisk "Zacznij trening"
        chooseLevelLabel.IsVisible = true; // Pokaż etykietę "Wybierz poziom"
        welcomeLabel.Text =
            "Witaj w aplikacji Nicolingo!"; // Przywróć oryginalny tekst etykiety
        answerEntry.IsVisible = false; // Ukryj pole do wpisywania odpowiedzi
        submitButton.IsVisible = false; // Ukryj przycisk "Zatwierdź"
        currentFlashcardIndex = 0;
        correctAnswers = 0;
        scoreLabel.Text = string.Empty;
        flashcardLabel.Text = string.Empty;
        answerEntry.Text = string.Empty;
        resultLabel.Text = string.Empty;
        resetButton.IsVisible = false;
        isTrainingActive = false; // Zresetuj stan treningu

        // Resetuj wybór języka początkowego i poziomu
        languagePicker.SelectedIndex = -1;
        modePicker.SelectedIndex = -1;
    }
}
