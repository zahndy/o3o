using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace o3o
{
    /// <summary>
    /// Interaction logic for TweetElement.xaml
    /// </summary>
    public partial class TweetElement : UserControl
    {
        string tweet;
        public string name { get; set; }
        public string Tweet { 
            get { return tweet; } 
            set { tweet = value; } 
        }
        public string Image { get; set; }
        // add time and all that stuff
        public TweetElement()
        {
            
            InitializeComponent();
            TweetBlock.Text = tweet;
        }
        

        
        //public double Top
        //{
        //    get { return grid.Margin.Top; }
        //    set { grid.Margin.Top = value; }
        //}
    }
}
