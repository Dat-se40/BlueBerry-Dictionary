using System.Windows;

namespace BlueBerryDictionary.Views.Dialogs.Introduces
{
    public partial class TermsDialog : Window
    {
        public TermsDialog()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}