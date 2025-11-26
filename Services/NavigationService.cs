using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace BlueBerryDictionary.Services
{
    public interface INavigationService
    {
        void Navigate(Page page);
    }
    public class NavigationService : INavigationService 
    {
        private Frame _frame; 
        public void Navigate(Page page) 
        {
            _frame.Navigate(page);        
        } 
        public NavigationService(Frame frame) => _frame = frame;    
    }
}
    