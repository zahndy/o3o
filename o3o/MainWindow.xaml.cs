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
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Collections.ObjectModel;

namespace o3o
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {


        public MainWindow()
        {
            InitializeComponent();
            MouseDown += delegate { if (MouseButtonState.Pressed == Mouse.LeftButton) { DragMove(); } };
            this.Loaded += new RoutedEventHandler(Window1_Loaded);  

        }


        void Window1_Loaded(object sender, RoutedEventArgs e)
        {
            this.SetAeroGlass();
           
        }

        int i;
        private void testbutton_Click(object sender, RoutedEventArgs e)
        {
            i++;

            Tweet(i.ToString(), "zanderroxley", "somedate");
            
        }

        public void Tweet(string message, string user, string date) // image is fetched in Tweetelement.xaml.cs
        {
            TweetElement element = new TweetElement();
            element.Tweet = message;
            element.name = user;
            element.Date = date;
            TweetElements.Items.Insert(0, element);
        }

        public void Notification(string message)
        {
            notify notification = new notify();
            notification.Text = message;
            
        }

        private void closebutton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void minimisebutton_Click(object sender, RoutedEventArgs e)
        {
             WindowState = WindowState.Minimized;
        }

        private void textBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            int left = 140 - (textBox1.Text.Length);
            charleft.Text = left.ToString();
            if (left < 0)
                charleft.Foreground = new SolidColorBrush(Colors.Red); 
            else
                charleft.Foreground = new SolidColorBrush(Colors.Black); 
        }

     

    }

    


}
