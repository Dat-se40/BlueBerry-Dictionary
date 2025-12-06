using BlueBerryDictionary.Models;
using BlueBerryDictionary.Services;
using System.Windows;

namespace BlueBerryDictionary.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for NoteWriterDialog.xaml
    /// </summary>
    public partial class NoteWriterDialog : Window
    {
        WordShortened mainWord;
        public NoteWriterDialog(WordShortened _mainWord)
        {
            InitializeComponent();
            mainWord = _mainWord;
            Display(); 
        }
        public NoteWriterDialog(Word word) 
        {
            InitializeComponent();
            if (TagService.Instance.FindWordInsensitive(word.word) is WordShortened ws && ws != null)
            {
                mainWord = ws;
            }
            else 
            {
                mainWord = WordShortened.FromWord(word);    
                TagService.Instance.AddNewWordShortened(mainWord);  
            }
            Display();
        }
        void Display() 
        {
            tbNote.Text = mainWord.note;
            WordTitleText.Text = mainWord.Word;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            mainWord.note = tbNote.Text;
            TagService.Instance.SaveWords(); 
            Close();
        }
    }
}
