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
        public string Tweet;
        public string name;
        public string Image { get; set; }
        // add time and all that stuff
        public TweetElement()
        {
            
            InitializeComponent();
            TweetBlock.Text = Tweet;
            
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            TweetBlock.Text = Tweet;
        }

        

        
        //public double Top
        //{
        //    get { return grid.Margin.Top; }
        //    set { grid.Margin.Top = value; }
        //}
    }
}
