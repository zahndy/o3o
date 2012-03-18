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
using System.Windows.Media.Effects;
using System.Collections.ObjectModel;
using Twitterizer;

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

        TweetStack o3o;
        void Window1_Loaded(object sender, RoutedEventArgs e)
        {
            this.SetAeroGlass();
            if (String.IsNullOrEmpty(Properties.Settings.Default.OAuth_AccessToken))
            {
                 o3o = new TweetStack(false);            
                o3o.OAuth.AuthenticateTwitter();
                o3o.OAuth.SaveOAuth();
            }
            else
            {
                 o3o = new TweetStack(true);
            }

            get_mentions();
            get_tweets();

           
        }

        void get_tweets()
        {
            Twitterizer.TwitterStatusCollection response = o3o.Twitter.GetTweets();
            foreach (Twitterizer.TwitterStatus tweet in response)
            {
                FillHome(tweet.Text, tweet.User.ScreenName, tweet.CreatedDate.ToString(), tweet.User.ProfileImageLocation, tweet.Id.ToString());
              
            }
            int index = response.Count - 1;
            Notification(response[index].Text, response[index].User.ScreenName, response[index].CreatedDate.ToString(), response[index].User.ProfileImageLocation, response[index].Id.ToString());
            
        }
        void get_mentions()
        {
            Twitterizer.TwitterStatusCollection menstruations = o3o.Twitter.GetMentions();


            foreach (Twitterizer.TwitterStatus drama in menstruations)
            {
                FillMentions(drama.Text, drama.User.ScreenName, drama.CreatedDate.ToString(), drama.User.ProfileImageLocation, drama.Id.ToString());
            }

        }

        private void testbutton_Click(object sender, RoutedEventArgs e)
        {
            if (textBox1.Text.Length <= 140)
            {
                o3o.Twitter.SendTweet(textBox1.Text);
                textBox1.Text = "";
                charleft.Text = "140";
                get_tweets();
            }
            else
            {
                charleft.Text = "too long";
            }
        }

        public void FillHome(string message, string user, string date, string url, string id) // image is fetched in Tweetelement.xaml.cs
        {
            TweetElement element = new TweetElement();
            element.Tweet = message;
            element.name = user;
            element.Date = date;
            element.imagelocation = url;
            element.ID = id;
            TweetElements.Items.Add(element);
        }

        public void FillMentions(string message, string user, string date, string url, string id) // image is fetched in Tweetelement.xaml.cs
        {
            TweetElement element = new TweetElement();
            element.Tweet = message;
            element.name = user;
            element.Date = date;
            element.imagelocation = url;
            element.ID = id;
            TweetMentions.Items.Add( element);
        }

        public void Notification(string message, string user, string date, string url, string id)
        {
            notify notification = new notify();
            TweetElement element = new TweetElement();
            element.Tweet = message;
            element.name = user;
            element.Date = date;
            element.imagelocation = url;
            element.ID = id;
            notification.content.Items.Add(element);
            
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
