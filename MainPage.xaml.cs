namespace LearningWordsByFlashcards
{
    public partial class MainPage : ContentPage
    {
        private string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "flashcards.txt");

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
                DisplayFlashcardsInConsole(content);
            }
            else
            {
                FileContentLabel.Text = "Plik nie istnieje.";
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

}
