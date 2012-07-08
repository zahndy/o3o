﻿using System;
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
using System.Windows.Forms;
using Twitterizer;
namespace o3o
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region loading stuff
        UserDatabase UsrDB = new UserDatabase();
        System.Windows.Threading.Dispatcher maindispatcher;
        public delegate void dostuff(string message, string user, DateTime date, string url, string id, string Description);
        public dostuff dostuffdel;

        public MainWindow()
        {
            InitializeComponent();

            
            MouseDown += delegate { if (MouseButtonState.Pressed == System.Windows.Input.Mouse.LeftButton) { DragMove(); } };
            this.Loaded += new RoutedEventHandler(Window1_Loaded);
        }


        void Window1_Loaded(object sender, RoutedEventArgs e)
        {
            this.SetAeroGlass();



            if (Properties.Settings.Default.use_system_color == true)
            {
                checkBox1.IsChecked = true;
            }

            if (UsrDB.load() == false || UsrDB.Users.Count == 0)
                UsrDB.CreateUser("Default user");



            maindispatcher = this.Dispatcher;
            foreach (UserDatabase.User usr in UsrDB.Users)
            {
                usr.tweetStack.NewTweet += new TweetStack.newtweetDel(o3o_NewTweet);

            }

            UpdateUserMenu(UsrDB);

        }

        #endregion

        #region tweet helpers

        void SendTweet()
        {
            if (textBox1.Text.Length <= 140)
            {
                if (!String.IsNullOrEmpty(textBox1.Text))
                {
                    UsrDB.Users.Find(u => u.Name == UserSelectionMenuCurrentName.Header).tweetStack.Twitter.SendTweet(textBox1.Text);
                    textBox1.Text = "";
                    charleft.Text = "140";
                }
                else
                {
                    //charleft.Foreground = new SolidColorBrush(Colors.Red);
                    //charleft.Text = "no text";
                    testbutton.Content = "Tweet";
                    TweetElements.Margin = new Thickness(0, 0, 0, 17);
                    textBox1.Visibility = Visibility.Collapsed;
                    charleft.Visibility = Visibility.Collapsed;
                    TweetLbl.Visibility = Visibility.Collapsed;
                    charleft.Foreground = new SolidColorBrush(Colors.Red);
                    charleft.Text = "no text";
                }
            }
            else
            {
                charleft.Foreground = new SolidColorBrush(Colors.Red);
                charleft.Text = "too long";
            }

        }

        void o3o_NewTweet(TwitterStatus status)
        {

            dostuffdel = new dostuff(FillHome);
            maindispatcher.Invoke(dostuffdel, new object[] { status.Text, status.User.ScreenName, status.CreatedDate, status.User.ProfileImageLocation, status.Id.ToString(), status.User.Description });

            dostuffdel = new dostuff(Notification);
            maindispatcher.Invoke(dostuffdel, new object[] { status.Text, status.User.ScreenName, status.CreatedDate, status.User.ProfileImageLocation, status.Id.ToString(), status.User.Description });


        }

        public void FillHome(string message, string user, DateTime date, string url, string id, string Description) 
        {
            TweetElement element = new TweetElement(this);
            element.Tweet = message;
            element.name = user;
            element.Date = date.Month.ToString() + "/" + date.Day.ToString() + " " + date.Hour.ToString() + ":" + date.Minute.ToString();
            element.imagelocation = url;
            element.ID = id;
            element.about = Description;
            
            TweetElements.Items.Insert(0, element);
        }

        public void FillMentions(string message, string user, DateTime date, string url, string id, string Description) 
        {
            TweetElement element = new TweetElement(this);
            element.Tweet = message;
            element.name = user;
            element.Date = date.Month.ToString() + "/" + date.Day.ToString() + " " + date.Hour.ToString() + ":" + date.Minute.ToString();
            element.imagelocation = url;
            element.ID = id;
            element.about = Description;
            TweetMentions.Items.Add( element);
        }

        public void Notification(string message, string user, DateTime date, string url, string id, string Description)
        {
            notify notification = new notify();
            TweetElement element = new TweetElement(this);
            element.Tweet = message;
            element.name = user;
            element.Date = date.Month.ToString() + "/" + date.Day.ToString() + " " + date.Hour.ToString() + ":" + date.Minute.ToString();
            element.imagelocation = url;
            element.ID = id;
            element.about = Description;
            element.replyBtn.Source = new BitmapImage(new Uri("/o3o;component/Images/reply.png", UriKind.Relative));
            notification.content.Items.Add(element);
            
        }
        #endregion

        #region UI interactions
        private void testbutton_Click(object sender, RoutedEventArgs e)
        {
            if (textBox1.Visibility == Visibility.Collapsed)
            {
                testbutton.Content = "Cancel";
                TweetElements.Margin = new Thickness(0, 0, 0, 70);
                textBox1.Visibility = Visibility.Visible;
                charleft.Visibility = Visibility.Visible;
                TweetLbl.Visibility = Visibility.Visible;
            }
            else if (textBox1.Visibility == Visibility.Visible)
            {
                SendTweet();
            }
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
            {
                charleft.Foreground = new SolidColorBrush(Colors.Red);
            }
            else if (left == 140)
            {
                testbutton.Content = "Cancel";
            }
            else
            {
                charleft.Foreground = new SolidColorBrush(Colors.Black);
                testbutton.Content = "Send";
            }
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AboutBox1 about = new AboutBox1();
            about.Show();
        }

        private void checkBox1_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.use_system_color = true;
            Properties.Settings.Default.Save();
        }

        private void checkBox1_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.use_system_color = false;
            Properties.Settings.Default.Save();
        }

        private void colorpicker_SelectedColorChanged(Color obj)
        {
            Properties.Settings.Default.system_color = System.Drawing.Color.FromArgb(obj.A,obj.R,obj.G,obj.B);
            Properties.Settings.Default.Save();
        }


        private void textBox1_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendTweet();
            }
            if (e.Key == Key.Escape)
            {
                textBox1.Text = "";
                testbutton.Content = "Tweet";
                TweetElements.Margin = new Thickness(0, 0, 0, 17);
                textBox1.Visibility = Visibility.Collapsed;
                charleft.Visibility = Visibility.Collapsed;
                TweetLbl.Visibility = Visibility.Collapsed;
            }

        }


        
        public void tbox(string inc)  // somehow use this in TweetElement.xaml.cs 
        {
            textBox1.Text = inc;
            if (textBox1.Visibility == Visibility.Collapsed)
            {
                testbutton.Content = "Tweet";
                TweetElements.Margin = new Thickness(0, 0, 0, 70);
                textBox1.Visibility = Visibility.Visible;
                charleft.Visibility = Visibility.Visible;
                TweetLbl.Visibility = Visibility.Visible;
            }

        }
        #endregion

        #region misc stuff
        public void UpdateUserMenu(UserDatabase usrDB)
        {
            UserSelectionMenu.Items.Clear();
            foreach (UserDatabase.User usr in usrDB.Users)
            {
                System.Windows.Controls.MenuItem newMenuItem1 = new System.Windows.Controls.MenuItem(); // here you add more users to the menu, also the events when the user selects something 
                newMenuItem1.Header = usr.Name;
                newMenuItem1.Click += new RoutedEventHandler(newMenuItem1_Click);
                this.UserSelectionMenu.Items.Add(newMenuItem1);
            }
            

            

        }

        void newMenuItem1_Click(object sender, RoutedEventArgs e)
        {
            UserSelectionMenuCurrentName.Header = ((System.Windows.Controls.MenuItem)sender).Header ; //string of current user
        }






        [DllImport("dwmapi.dll", EntryPoint = "#127", PreserveSig = false)]
        public static extern void DwmGetColorizationParameters(out WDM_COLORIZATION_PARAMS parameters);
        public struct WDM_COLORIZATION_PARAMS
        {
            public uint Color1;
            public uint Color2;
            public uint Intensity;
            public uint Unknown1;
            public uint Unknown2;
            public uint Unknown3;
            public uint Opaque;
        }
        #endregion


    }

}
