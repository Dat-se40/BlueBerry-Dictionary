using BlueBerryDictionary.Models;
using System;
using System.Windows;
using System.Windows.Controls;

namespace BlueBerryDictionary.Views.UserControls
{
    public partial class WordDefinitionCard : UserControl
    {
        // Dependency Properties
        public static readonly DependencyProperty WordProperty =
            DependencyProperty.Register("Word", typeof(string), typeof(WordDefinitionCard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty PronunciationProperty =
            DependencyProperty.Register("Pronunciation", typeof(string), typeof(WordDefinitionCard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty PartOfSpeechProperty =
            DependencyProperty.Register("PartOfSpeech", typeof(string), typeof(WordDefinitionCard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty DefinitionProperty =
            DependencyProperty.Register("Definition", typeof(string), typeof(WordDefinitionCard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty TimeStampProperty =
            DependencyProperty.Register("TimeStamp", typeof(string), typeof(WordDefinitionCard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty ViewCountProperty =
            DependencyProperty.Register("ViewCount", typeof(string), typeof(WordDefinitionCard), new PropertyMetadata("0 lần"));
        public static readonly DependencyProperty RegionProperty =
               DependencyProperty.Register("Region", typeof(string), typeof(WordDefinitionCard), new PropertyMetadata(string.Empty));
        // Properties
        public string Region
        {
            get => (string)GetValue(RegionProperty);
            set => SetValue(RegionProperty, value);
        }

        public static readonly DependencyProperty Example1Property =
            DependencyProperty.Register("Example1", typeof(string), typeof(WordDefinitionCard), new PropertyMetadata(string.Empty));

        public string Example1
        {
            get => (string)GetValue(Example1Property);
            set => SetValue(Example1Property, value);
        }

        public static readonly DependencyProperty Example2Property =
            DependencyProperty.Register("Example2", typeof(string), typeof(WordDefinitionCard), new PropertyMetadata(string.Empty));

        public string Example2
        {
            get => (string)GetValue(Example2Property);
            set => SetValue(Example2Property, value);
        }

        
        public string Word
        {
            get { return (string)GetValue(WordProperty); }
            set { SetValue(WordProperty, value); }
        }

        public string Pronunciation
        {
            get { return (string)GetValue(PronunciationProperty); }
            set { SetValue(PronunciationProperty, value); }
        }

        public string PartOfSpeech
        {
            get { return (string)GetValue(PartOfSpeechProperty); }
            set { SetValue(PartOfSpeechProperty, value); }
        }

        public string Definition
        {
            get { return (string)GetValue(DefinitionProperty); }
            set { SetValue(DefinitionProperty, value); }
        }

        public string TimeStamp
        {
            get { return (string)GetValue(TimeStampProperty); }
            set { SetValue(TimeStampProperty, value); }
        }

        public string ViewCount
        {
            get { return (string)GetValue(ViewCountProperty); }
            set { SetValue(ViewCountProperty, value); }
        }

        // Events
        public event EventHandler FavoriteClicked;
        public event EventHandler DeleteClicked;
        public event EventHandler CardClicked;

        public WordDefinitionCard(Word mainWord = null)
        {
            InitializeComponent();
            DataContext = this;

            // Handle card click
            MouseLeftButtonDown += (s, e) => CardClicked?.Invoke(this, EventArgs.Empty);
            
            if (mainWord != null) 
            {
                this.Word = mainWord.word;
                this.Pronunciation = mainWord.phonetic;
                this.Region = "US"; // Sẽ fix cái nì
                this.PartOfSpeech = mainWord.meanings[0].partOfSpeech;
                this.Definition = mainWord.meanings[0].definitions[0].definition;
            }
        }
        public WordDefinitionCard(WordShortened mainWord) 
        {
            InitializeComponent();
            DataContext = this;
            MouseLeftButtonDown += (s, e) => CardClicked?.Invoke(this, EventArgs.Empty);
            if (mainWord != null)
            {
                this.Word = mainWord.Word;
                this.Pronunciation = mainWord.Phonetic;
                this.Region = "UK";                 
                this.PartOfSpeech = mainWord.PartOfSpeech;
                this.Definition = mainWord.Definition;
                this.Example1 = mainWord.Example;
                this.TimeStamp = mainWord.AddedAt.ToShortDateString(); 
            }
        }
        public WordDefinitionCard() 
        {
            InitializeComponent();
        }
        private void FavoriteButton_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true; // Prevent card click event
            FavoriteClicked?.Invoke(this, EventArgs.Empty);
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true; // Prevent card click event
            DeleteClicked?.Invoke(this, EventArgs.Empty);
        }
    }
}