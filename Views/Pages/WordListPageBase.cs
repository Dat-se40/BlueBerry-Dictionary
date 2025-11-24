using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
