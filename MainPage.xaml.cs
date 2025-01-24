namespace LearningWordsByFlashcards
{
    public partial class MainPage : ContentPage
    {
        private string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "sample.txt");

        public MainPage()
        {
            InitializeComponent();
        }

        private void OnLoadFileClicked(object sender, EventArgs e)
        {
            if (File.Exists(filePath))
            {
                string content = File.ReadAllText(filePath);
                FileContentLabel.Text = content;
            }
            else
            {
                FileContentLabel.Text = "Plik nie istnieje.";
            }
        }

        private void OnAddTextClicked(object sender, EventArgs e)
        {
            string newText = NewTextEntry.Text;
            if (!string.IsNullOrEmpty(newText))
            {
                File.AppendAllText(filePath, newText + Environment.NewLine);
                OnLoadFileClicked(sender, e); // Odśwież zawartość pliku
            }
        }
    }

}
