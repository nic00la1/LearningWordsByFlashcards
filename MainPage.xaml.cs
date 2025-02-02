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
    private List<Flashcard> filteredFlashcards = new();
    private Settings settings = new();
    private int currentFlashcardIndex;
    private int correctAnswers;
    private string selectedLanguage;
    private bool isTrainingActive;
    private int numberOfFlashcardsToPractice;

    public MainPage()
    {
        InitializeComponent();
        LoadFlashcards();
        LoadSettings();
        ApplySettings();
        resetButton.IsVisible = false; // Ukryj przycisk resetowania na początku
        scoreLabel.IsVisible = false; // Ukryj etykietę wyniku na początku
        flashcardLabel.IsVisible = true;
    }

    private void ShuffleFlashcards()
    {
        Random rng = new();
        int n = filteredFlashcards.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            Flashcard value = filteredFlashcards[k];
            filteredFlashcards[k] = filteredFlashcards[n];
            filteredFlashcards[n] = value;
        }
    }

    private string GenerateHint(string word)
    {
        return
            $"{word[0]}{string.Join(" ", new string('_', word.Length - 1).ToCharArray())}";
    }

    private string GenerateSentenceHint(string sentence)
    {
        string[] words = sentence.Split(' ');
        return string.Join("  ", words.Select(GenerateHint));
    }

    private void LoadFlashcards()
    {
        try
        {
            string filePath = Path.Combine(FileSystem.AppDataDirectory,
                "flashcards.json");

            if (!File.Exists(filePath))
            {
                DisplayAlert("Błąd", "Plik flashcards.json nie istnieje.",
                    "OK");
                flashcards = new List<Flashcard>();
                return;
            }

            using StreamReader reader = new(filePath);
            string json = reader.ReadToEnd();
            FlashcardsContainer? flashcardsContainer =
                JsonSerializer.Deserialize<FlashcardsContainer>(json);
            if (flashcardsContainer != null &&
                flashcardsContainer.Flashcards != null)
            {
                flashcards = flashcardsContainer.Flashcards;
                DisplayAlert("Sukces", $"Załadowano {flashcards.Count} fiszek.",
                    "OK");

                // Ustaw minimalną i maksymalną wartość suwaka
                numberOfFlashcardsSlider.Minimum = 1;
                numberOfFlashcardsSlider.Maximum = flashcards.Count;
            } else
            {
                DisplayAlert("Błąd",
                    "Nie udało się zdeserializować flashcards.json lub fiszki są puste.",
                    "OK");
                flashcards = new List<Flashcard>();
            }
        }
        catch (Exception ex)
        {
            DisplayAlert("Błąd", $"Błąd podczas ładowania fiszek: {ex.Message}",
                "OK");
            flashcards = new List<Flashcard>();
        }
    }

    private void LoadSettings()
    {
        try
        {
            string filePath = Path.Combine(FileSystem.AppDataDirectory,
                "settings.json");

            if (!File.Exists(filePath))
            {
                DisplayAlert("Błąd", "Plik settings.json nie istnieje.", "OK");
                settings = new Settings();
                return;
            }

            using StreamReader reader = new(filePath);
            string json = reader.ReadToEnd();
            settings = JsonSerializer.Deserialize<Settings>(json);
            DisplayAlert("Sukces", "Ustawienia zostały załadowane.", "OK");
        }
        catch (Exception ex)
        {
            DisplayAlert("Błąd",
                $"Błąd podczas ładowania ustawień: {ex.Message}", "OK");
            settings = new Settings();
        }
    }

    public void ApplySettings()
    {
        // Ustaw wartość suwaka na podstawie ustawień
        if (settings.UseDefaultNumberOfFlashcards)
        {
            // Sprawdź, czy NumberOfFlashcards jest większe od 0, jeśli nie, ustaw domyślną wartość
            if (settings.NumberOfFlashcards <= 0)
                settings.NumberOfFlashcards =
                    10; // Ustaw domyślną wartość, np. 10
            numberOfFlashcardsSlider.Value = settings.NumberOfFlashcards;
            numberOfFlashcardsSlider.IsVisible = false;
            sliderValueLabel.IsVisible = false;
            numberOfFlashcardsLabel.IsVisible = false; // Ukryj etykietę suwaka
        } else
        {
            numberOfFlashcardsSlider.Value = settings.NumberOfFlashcards > 0
                ? settings.NumberOfFlashcards
                : 10; // Ustaw domyślną wartość, jeśli jest 0
            numberOfFlashcardsSlider.IsVisible = true;
            sliderValueLabel.IsVisible = true;
            numberOfFlashcardsLabel.IsVisible = true; // Pokaż etykietę suwaka
        }
    }

    private async void OnSettingsClicked(object sender, EventArgs e)
    {
        SettingsPage settingsPage = new(settings, flashcards.Count);
        settingsPage.MainPage = this; // Ustaw referencję do MainPage
        await Navigation.PushAsync(settingsPage);
    }

    private void OnSliderValueChanged(object sender, ValueChangedEventArgs e)
    {
        sliderValueLabel.Text = ((int)e.NewValue).ToString();
    }

    private void OnStartTrainingClicked(object sender, EventArgs e)
    {
        if (isTrainingActive)
        {
            DisplayAlert("Błąd", "Trening jest już aktywny.", "OK");
            return;
        }

        if (flashcards == null || !flashcards.Any())
        {
            DisplayAlert("Błąd", "Fiszki nie zostały załadowane.", "OK");
            return;
        }

        if (languagePicker == null || languagePicker.SelectedItem == null)
        {
            DisplayAlert("Błąd", "Nie wybrano języka.", "OK");
            return;
        }

        selectedLanguage = languagePicker.SelectedItem.ToString();
        languagePicker.IsVisible = false;
        startTrainingButton.IsVisible = false;
        welcomeLabel.Text = "Trening rozpoczęty";
        answerEntry.IsVisible = true;
        submitButton.IsVisible = true;

        // Filtrowanie fiszek na podstawie poziomu trudności
        filteredFlashcards = flashcards
            .Where(f => settings.WordDifficultyLevels.Contains(f.Difficulty))
            .ToList();

        // Użyj domyślnej liczby fiszek, jeśli jest określona w ustawieniach
        if (settings.UseDefaultNumberOfFlashcards)
            numberOfFlashcardsToPractice = Math.Min(
                settings.NumberOfFlashcards > 0
                    ? settings.NumberOfFlashcards
                    : 10, filteredFlashcards.Count);
        else
            numberOfFlashcardsToPractice = Math.Min(
                (int)numberOfFlashcardsSlider.Value, filteredFlashcards.Count);

        // Sprawdź, czy liczba fiszek do praktyki jest większa od 0
        if (numberOfFlashcardsToPractice <= 0)
        {
            DisplayAlert("Błąd",
                "Liczba fiszek do praktyki musi być większa od 0.", "OK");
            return;
        }

        // Ukryj suwak i jego etykietę
        numberOfFlashcardsSlider.IsVisible = false;
        sliderValueLabel.IsVisible = false;
        numberOfFlashcardsLabel.IsVisible = false; // Ukryj etykietę suwaka

        ShuffleFlashcards();
        currentFlashcardIndex = 0;
        correctAnswers = 0;
        isTrainingActive = true;
        ShowNextFlashcard();
    }

    private async void ShowNextFlashcard()
    {
        if (currentFlashcardIndex < numberOfFlashcardsToPractice &&
            currentFlashcardIndex < filteredFlashcards.Count)
        {
            Flashcard flashcard = filteredFlashcards[currentFlashcardIndex];
            if (selectedLanguage == "PL")
            {
                flashcardLabel.Text = $"{flashcard.PL[0]}";
                if (flashcard.PL.Count > 1)
                    flashcardLabel.Text +=
                        $" ({string.Join(", ", flashcard.PL.Skip(1))})";
            } else
            {
                flashcardLabel.Text = $"{flashcard.ENG[0]}";
                if (flashcard.ENG.Count > 1)
                    flashcardLabel.Text +=
                        $" ({string.Join(", ", flashcard.ENG.Skip(1))})";
            }

            if (settings.DifficultyMode == "Easy")
            {
                hintLabel.IsVisible = true;
                if (selectedLanguage == "PL")
                    hintLabel.Text = string.Join(", ",
                        flashcard.ENG.Select(GenerateSentenceHint));
                else
                    hintLabel.Text = string.Join(", ",
                        flashcard.PL.Select(GenerateSentenceHint));
            } else
            {
                hintLabel.IsVisible = false;
                hintLabel.Text = string.Empty;
            }

            if (settings.ShowDescription &&
                flashcard.Hint.ContainsKey(selectedLanguage))
            {
                descriptionLabel.Text = flashcard.Hint[selectedLanguage];
                descriptionLabel.IsVisible = true;
            } else
                descriptionLabel.IsVisible = false;

            answerEntry.Text = string.Empty;
            resultLabel.Text = string.Empty;

            // Zaktualizuj etykietę indeksu fiszki
            flashcardIndexLabel.Text =
                $"Fiszka {currentFlashcardIndex + 1} z {numberOfFlashcardsToPractice}";
            flashcardIndexLabel.IsVisible = true;
        } else
        {
            scoreLabel.Text =
                $"Wynik: {correctAnswers}/{numberOfFlashcardsToPractice} ({correctAnswers / (double)numberOfFlashcardsToPractice * 100}%)";
            scoreLabel.IsVisible = true; // Pokaż etykietę wyniku
            resetButton.IsVisible = true; // Pokaż przycisk resetowania
            welcomeLabel.Text = "Zakończono trening!"; // Zmień tytuł
            flashcardLabel.Text = string.Empty; // Ukryj tekst fiszki
            hintLabel.IsVisible = false; // Ukryj podpowiedzi
            descriptionLabel.IsVisible = false; // Ukryj opis
            answerEntry.IsVisible =
                false; // Ukryj pole do wpisywania odpowiedzi
            submitButton.IsVisible = false; // Ukryj przycisk "Zatwierdź"
            flashcardIndexLabel.IsVisible = false; // Ukryj licznik fiszek
        }
    }

    private async void OnSubmitClicked(object sender, EventArgs e)
    {
        if (!isTrainingActive)
        {
            DisplayAlert("Błąd", "Trening nie jest aktywny.", "OK");
            return;
        }

        if (currentFlashcardIndex >= filteredFlashcards.Count)
        {
            DisplayAlert("Błąd", "Brak więcej fiszek do pokazania.", "OK");
            return;
        }

        Flashcard flashcard = filteredFlashcards[currentFlashcardIndex];
        string answer = answerEntry.Text;

        if (settings.DifficultyMode == "Easy")
        {
            if (selectedLanguage == "PL")
            {
                if (flashcard.ENG.Contains(answer))
                {
                    correctAnswers++;
                    resultLabel.Text = "Poprawnie!";
                } else
                    resultLabel.Text =
                        $"Błędnie! Poprawne odpowiedzi: {string.Join(", ", flashcard.ENG)}";
            } else if (selectedLanguage == "ENG")
            {
                if (flashcard.PL.Contains(answer))
                {
                    correctAnswers++;
                    resultLabel.Text = "Poprawnie!";
                } else
                    resultLabel.Text =
                        $"Błędnie! Poprawne odpowiedzi: {string.Join(", ", flashcard.PL)}";
            }
        } else if (settings.DifficultyMode == "Hard")
        {
            List<string> answers =
                answer.Split(',').Select(a => a.Trim()).ToList();
            bool allCorrect = false;

            if (selectedLanguage == "PL")
                allCorrect = flashcard.ENG.All(e => answers.Contains(e)) &&
                    answers.All(a => flashcard.ENG.Contains(a));
            else if (selectedLanguage == "ENG")
                allCorrect = flashcard.PL.All(p => answers.Contains(p)) &&
                    answers.All(a => flashcard.PL.Contains(a));

            if (allCorrect)
            {
                correctAnswers++;
                resultLabel.Text = "Poprawnie!";
            } else
                resultLabel.Text =
                    $"Błędnie! Poprawne odpowiedzi: {string.Join(", ", selectedLanguage == "PL" ? flashcard.ENG : flashcard.PL)}";
        }

        await Task
            .Delay(2000); // Delay for 2 seconds to show the correct answer

        currentFlashcardIndex++;
        ShowNextFlashcard();
    }

    private void OnResetTrainingClicked(object sender, EventArgs e)
    {
        languagePicker.IsVisible = true;
        startTrainingButton.IsVisible =
            true; // Pokaż przycisk "Zacznij trening"
        welcomeLabel.Text =
            "Witaj w aplikacji Nicolingo!"; // Przywróć oryginalny tekst etykiety
        answerEntry.IsVisible = false; // Ukryj pole do wpisywania odpowiedzi
        submitButton.IsVisible = false; // Ukryj przycisk "Zatwierdź"
        currentFlashcardIndex = 0;
        correctAnswers = 0;
        scoreLabel.Text = string.Empty;
        scoreLabel.IsVisible = false; // Ukryj etykietę wyniku
        flashcardLabel.Text = string.Empty;
        hintLabel.Text = string.Empty;
        hintLabel.IsVisible = false;
        answerEntry.Text = string.Empty;
        resultLabel.Text = string.Empty;
        resetButton.IsVisible = false;
        isTrainingActive = false; // Zresetuj stan treningu

        // Resetuj wybór języka początkowego
        languagePicker.SelectedIndex = -1;

        // Pokaż suwak i jego etykietę ponownie, jeśli opcja UseDefaultNumberOfFlashcards jest wyłączona
        if (!settings.UseDefaultNumberOfFlashcards)
        {
            numberOfFlashcardsSlider.IsVisible = true;
            sliderValueLabel.IsVisible = true;
            numberOfFlashcardsLabel.IsVisible = true; // Pokaż etykietę suwaka
        }
    }
}
