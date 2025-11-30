namespace BlueBerryDictionary.Views.Pages
{
    using System;
    using System.Diagnostics;
    using System.Windows;
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : WordListPageBase
    {
        /// <summary>
        /// Defines the Navigate
        /// </summary>
        public Action<object, RoutedEventArgs> Navigate;

        /// <summary>
        /// Initializes a new instance of the <see cref="HomePage"/> class.
        /// </summary>
        /// <param name="action">The action<see cref="Action{string}"/></param>
        /// <param name="navigate">The navigate<see cref="Action{object, RoutedEventArgs}"/></param>
        public HomePage(Action<string> action, Action<object, RoutedEventArgs> navigate) : base(action)
        {
            InitializeComponent();
            Navigate += navigate;
        }

        /// <summary>
        /// The ButtnNavigate_Click
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/></param>
        /// <param name="e">The e<see cref="RoutedEventArgs"/></param>
        internal void ButtnNavigate_Click(object sender, RoutedEventArgs e)
        {
            Navigate?.Invoke(sender, e);
        }
        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }
    }

}
