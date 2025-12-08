using BlueBerryDictionary.Models;
using System.Windows.Controls;
using System.Windows.Input;

namespace BlueBerryDictionary.Views.UserControls
{
    /// <summary>
    /// Interaction logic for WordItem.xaml
    /// </summary>
    public partial class WordItem : UserControl
    {
        public Action<string> OnWordClick; 
        public WordItem()
        {
            InitializeComponent();
            this.MouseLeftButtonDown += WordItem_MouseLeftButtonDown;
        }

        public void SetUpDisplay(Word mainWord)
        {
            tbMainWord.Text = mainWord.word;
            tbPhonetic.Text = mainWord.phonetic;
            tbMeaningText.Text = mainWord.meanings[0].definitions[0].definition ?? string.Empty;
        }
        public void SetUpDisplay(ShortenWord mainWord)
        {
            tbMainWord.Text = mainWord.word;
            tbPhonetic.Text = mainWord.phonetic;
            tbMeaningText.Text = mainWord.meaning;
        }

        private void WordItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OnWordClick?.Invoke(tbMainWord.Text); 
        }
    }
}
