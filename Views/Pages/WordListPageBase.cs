using BlueBerryDictionary.Models;
using BlueBerryDictionary.Views.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;

namespace BlueBerryDictionary.Views.Pages
{
    public class WordListPageBase : System.Windows.Controls.Page  
    {
        protected Action<string> _onWordClick;
        public WordListPageBase(Action<string> onWordClick)
        {
            _onWordClick = onWordClick;
        }
        protected void HandleWordClick(string word)
        {
            _onWordClick?.Invoke(word);
        }
        public virtual void LoadData() { }

        public virtual void LoadDefCards(UniformGrid mainContent , IEnumerable<WordShortened> wordShorteneds) 
        {
            mainContent.Children.Clear();
            foreach (WordShortened ws in wordShorteneds) 
            {
                var wfc = new WordDefinitionCard(ws);
                wfc.MouseDown += (s, e) => 
                {
                    HandleWordClick(ws.Word);
                };
                mainContent.Children.Add(wfc);  
            }
        }
    }
}
