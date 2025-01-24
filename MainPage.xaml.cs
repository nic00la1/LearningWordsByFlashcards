using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Maui.Controls;

namespace LearningWordsByFlashcards
{
    public partial class MainPage : ContentPage
    {
        private string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "flashcards.txt");
        private List<Flashcard> flashcards = new List<Flashcard>();
        private int currentFlashcardIndex = 0;
        private int score = 0;
        private string selectedLanguage = "PL";

        public MainPage()
        {
            InitializeComponent();
            InitializeFlashcardsFile();
        }

        private void InitializeFlashcardsFile()
        {
            if (!File.Exists(filePath))
            {
                var initialContent = "PL-ENG\n" +
                                     "jabłko - apple\n" +
                                     "samochód | auto - car | auto | automobile\n" +
                                     "Czyny mówią głośniej niż słowa - Actions speak louder than words\n" +
                                     "dom - house\n" +
                                     "pies - dog\n" +
                                     "kot - cat\n" +
                                     "książka - book\n" +
                                     "stół - table\n" +
                                     "krzesło - chair\n" +
                                     "okno - window\n" +
                                     "drzwi - door\n";
                File.WriteAllText(filePath, initialContent);
            }
        }

        private void OnLoadFileClicked(object sender, EventArgs e)
        {
            if (File.Exists(filePath))
            {
                string content = File.ReadAllText(filePath);
                FileContentLabel.Text = content;
                LoadFlashcards(content);
            }
            else
            {
                FileContentLabel.Text = "Plik nie istnieje.";
            }
        }

        private void LoadFlashcards(string content)
        {
            flashcards.Clear();
            var lines = content.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 1; i < lines.Length; i++) // Pomijamy pierwszą linię z informacją o językach
            {
                var parts = lines[i].Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    var leftSide = parts[0].Split(new[] { '|', ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();
                    var rightSide = parts[1].Split(new[] { '|', ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();
                    flashcards.Add(new Flashcard { PL = leftSide, ENG = rightSide });
                }
            }
        }

        private void OnAddFlashcardClicked(object sender, EventArgs e)
        {
            string newFlashcard = NewFlashcardEntry.Text;
            if (!string.IsNullOrEmpty(newFlashcard))
            {
                File.AppendAllText(filePath, newFlashcard + Environment.NewLine);
                OnLoadFileClicked(sender, e); // Odśwież zawartość pliku
            }
        }

        private void OnRemoveFlashcardClicked(object sender, EventArgs e)
        {
            if (int.TryParse(RemoveFlashcardEntry.Text, out int lineNumber))
            {
                var lines = File.ReadAllLines(filePath).ToList();
                if (lineNumber > 0 && lineNumber < lines.Count)
                {
                    lines.RemoveAt(lineNumber);
                    File.WriteAllLines(filePath, lines);
                    OnLoadFileClicked(sender, e); // Odśwież zawartość pliku
                }
            }
        }

        private void OnStartLearningClicked(object sender, EventArgs e)
        {
            if (LanguagePicker.SelectedItem != null)
            {
                selectedLanguage = LanguagePicker.SelectedItem.ToString();
                flashcards = flashcards.OrderBy(x => Guid.NewGuid()).ToList(); // Losowa kolejność
                currentFlashcardIndex = 0;
                score = 0;
                ShowNextFlashcard();
            }
            else
            {
                DisplayAlert("Błąd", "Proszę wybrać język początkowy.", "OK");
            }
        }

        private void ShowNextFlashcard()
        {
            if (currentFlashcardIndex < flashcards.Count)
            {
                var flashcard = flashcards[currentFlashcardIndex];
                CurrentFlashcardLabel.Text = selectedLanguage == "PL" ? string.Join(", ", flashcard.PL) : string.Join(", ", flashcard.ENG);
            }
            else
            {
                ScoreLabel.Text = $"Wynik: {score}/{flashcards.Count} ({(score * 100) / flashcards.Count}%)";
            }
        }

        private void OnCheckAnswerClicked(object sender, EventArgs e)
        {
            if (currentFlashcardIndex < flashcards.Count)
            {
                var flashcard = flashcards[currentFlashcardIndex];
                var correctAnswers = selectedLanguage == "PL" ? flashcard.ENG : flashcard.PL;
                if (correctAnswers.Contains(AnswerEntry.Text.Trim(), StringComparer.OrdinalIgnoreCase))
                {
                    score++;
                }
                currentFlashcardIndex++;
                ShowNextFlashcard();
            }
        }

        private void DisplayFlashcardsInConsole(string content)
        {
            var lines = content.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var flashcards = new List<List<string[]>>();

            for (int i = 1; i < lines.Length; i++) // Pomijamy pierwszą linię z informacją o językach
            {
                var parts = lines[i].Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    var leftSide = parts[0].Split(new[] { '|', ',' }, StringSplitOptions.RemoveEmptyEntries);
                    var rightSide = parts[1].Split(new[] { '|', ',' }, StringSplitOptions.RemoveEmptyEntries);
                    flashcards.Add(new List<string[]> { leftSide, rightSide });
                }
            }

            foreach (var flashcard in flashcards)
            {
                Console.WriteLine("[");
                foreach (var side in flashcard)
                {
                    Console.Write("  [");
                    Console.Write(string.Join(", ", side));
                    Console.WriteLine("]");
                }
                Console.WriteLine("]");
            }
        }
    }

    public class Flashcard
    {
        public string[] PL { get; set; } = Array.Empty<string>();
        public string[] ENG { get; set; } = Array.Empty<string>();
    }
}
