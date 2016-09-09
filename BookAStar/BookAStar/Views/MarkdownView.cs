using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

using Xamarin.Forms;
          
namespace BookAStar.Views
{
    public partial class MarkdownView : ContentView
    {
        private string markdown;

        public string Markdown
        {
            get
            {
                return markdown;
            }
            set { markdown = value; }
        }

        public MarkdownView() : base()
        {
            markdown = "**Hello** _world_";
            Content = App.PlateformSpecificInstance.CreateMarkdownView(markdown);
            
        }
        
    }
}
