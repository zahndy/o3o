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
using System.Windows.Forms;
using Twitterizer;
namespace o3o
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        TweetStack o3o;
        System.Windows.Threading.Dispatcher maindispatcher;
        public delegate void dostuff(string message, string user, DateTime date, string url, string id);
        public dostuff dostuffdel;
        public MainWindow()
        {
            InitializeComponent();
            if (!String.IsNullOrEmpty(Properties.Settings.Default.OAuth_AccessToken))
            {
                o3o = new TweetStack(false);
                o3o.OAuth.AuthenticateTwitter();
                o3o.OAuth.SaveOAuth();
            }
            else
            {
                o3o = new TweetStack(true,true);
            }

            maindispatcher = this.Dispatcher;
            o3o.NewTweet += new TweetStack.newtweetDel(o3o_NewTweet);
            
            
            MouseDown += delegate { if (MouseButtonState.Pressed == System.Windows.Input.Mouse.LeftButton) { DragMove(); } };
            this.Loaded += new RoutedEventHandler(Window1_Loaded);
        }

        void o3o_NewTweet(TwitterStatus status)
        {

            dostuffdel = new dostuff(FillHome);
            maindispatcher.Invoke(dostuffdel, new object[] { status.Text, status.User.ScreenName, status.CreatedDate, status.User.ProfileImageLocation, status.Id.ToString() });

            dostuffdel = new dostuff(Notification);
            maindispatcher.Invoke(dostuffdel, new object[] { status.Text, status.User.ScreenName, status.CreatedDate, status.User.ProfileImageLocation, status.Id.ToString() });
            

        }


        
        void Window1_Loaded(object sender, RoutedEventArgs e)
        {
            this.SetAeroGlass();
           

            if (Properties.Settings.Default.use_system_color == true)
            {
                checkBox1.IsChecked = true;
            }

            
           
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



        void get_mentions()
        {

            //TweetMentions.Items.Clear();
            //foreach (Twitterizer.TwitterStatus lama in mentions)
            //{
            //    FillMentions(lama.Text, lama.User.ScreenName, lama.CreatedDate, lama.User.ProfileImageLocation, lama.Id.ToString());
            //}

        }


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
                if (textBox1.Text.Length <= 140)
                {
                    if (!String.IsNullOrEmpty(textBox1.Text))
                    {
                        o3o.Twitter.SendTweet(textBox1.Text);
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
                    }
                }
                else
                {
                    charleft.Foreground = new SolidColorBrush(Colors.Red);
                    charleft.Text = "Too long";
                }
            }
        }

        public void FillHome(string message, string user, DateTime date, string url, string id) 
        {
            TweetElement element = new TweetElement(this);
            element.Tweet = message;
            element.name = user;
            element.Date = date.Month.ToString() + "/" + date.Day.ToString() + " " + date.Hour.ToString() + ":" + date.Minute.ToString();
            element.imagelocation = url;
            element.ID = id;
            
            TweetElements.Items.Insert(0, element);
        }

        public void FillMentions(string message, string user, DateTime date, string url, string id) 
        {
            TweetElement element = new TweetElement(this);
            element.Tweet = message;
            element.name = user;
            element.Date = date.Month.ToString() + "/" + date.Day.ToString() + " " + date.Hour.ToString() + ":" + date.Minute.ToString();
            element.imagelocation = url;
            element.ID = id;
            TweetMentions.Items.Add( element);
        }

        public void Notification(string message, string user, DateTime date, string url, string id)
        {
            notify notification = new notify();
            TweetElement element = new TweetElement(this);
            element.Tweet = message;
            element.name = user;
            element.Date = date.Month.ToString() + "/" + date.Day.ToString() + " " + date.Hour.ToString() + ":" + date.Minute.ToString();
            element.imagelocation = url;
            element.ID = id;
            element.replyBtn.Source = new BitmapImage(new Uri("/o3o;component/Images/reply.png", UriKind.Relative));
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

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            get_mentions();

        }

        private void textBox1_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (textBox1.Text.Length <= 140)
                {
                    if (!String.IsNullOrEmpty(textBox1.Text))
                    {
                        o3o.Twitter.SendTweet(textBox1.Text);
                        textBox1.Text = "";
                        charleft.Text = "140";
                    }
                    else
                    {
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

        private void btn_right_MouseUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void btn_Left_MouseUp(object sender, MouseButtonEventArgs e)
        {

        }

        

        //private void reload_MouseDown(object sender, MouseButtonEventArgs e)
        //{

        //}

        //private void reload_MouseLeave(object sender, MouseEventArgs e)
        //{
        //    reload.Source = new BitmapImage(new Uri("/o3o;component/Images/Reload_normal.png", UriKind.Relative));
        //}

        //private void reload_MouseEnter(object sender, MouseEventArgs e)
        //{

        //    reload.Source = new BitmapImage(new Uri("/o3o;component/Images/Reload_Hover.png", UriKind.Relative));
        //}
        


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

        
     

    }

}
