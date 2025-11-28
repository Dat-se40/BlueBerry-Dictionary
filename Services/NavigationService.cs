using BlueBerryDictionary.Views.Pages;
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
        void Navigate(Page page, string namePage);
    }
    public class NavigationService : INavigationService 
    {
        private Frame _frame;
        private Stack<string> namePages = new Stack<string>() ; 
        public void Navigate(Page page, string namePage) 
        {
            Console.WriteLine("[NaviService] " + namePage);
            bool isValid = false ;
            if (namePages.Count != 0 && namePage != namePages.Peek() || namePages.Count == 0 ) isValid = true ;
           
            if (isValid)
            {
                namePages.Push(namePage);
                _frame.Navigate(page);
            }
             
        } 
        public NavigationService(Frame frame) => _frame = frame;    
    }
}
    