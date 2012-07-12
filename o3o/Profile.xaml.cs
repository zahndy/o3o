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
using System.Windows.Shapes;
using System.Windows.Interop;
using System.Runtime.InteropServices;

namespace o3o
{
    /// <summary>
    /// Interaction logic for Profile.xaml
    /// </summary>
    /// 
    public partial class Profile : Window
    {
        public System.Windows.Media.ImageSource image;
        public MainWindow parent;

        
        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hwnd, int index);
        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);
        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter, int x, int y, int width, int height, uint flags);
        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam);
        const int GWL_EXSTYLE = -20;
        const int WS_EX_DLGMODALFRAME = 0x0001;
        const int SWP_NOSIZE = 0x0001;
        const int SWP_NOMOVE = 0x0002;
        const int SWP_NOZORDER = 0x0004;
        const int SWP_FRAMECHANGED = 0x0020;
        const uint WM_SETICON = 0x0080;

        Twitterizer.TwitterUser User;
        public Profile(MainWindow prnt, string name)
        {
            parent = prnt;
            InitializeComponent();
            MouseDown += delegate { if (MouseButtonState.Pressed == System.Windows.Input.Mouse.LeftButton) { DragMove(); } };
           User = parent.UsrDB.Users[0].tweetStack.Twitter.GetUser(name);
            
        }

        private void twitterpagelabel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            string target = "http://twitter.com/#!/" + User.Name;
            System.Diagnostics.Process.Start(target);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UserNameLabel.Content = User.Name;
            Description.Text = User.Description;
            image1.Source = image;
            tweetsLbl.Content = "Tweets: " + User.NumberOfStatuses;
            followersLbl.Content = "Followers: " + User.NumberOfFollowers;
            language.Content = "Language: " + User.Language.ToString();
            creationdate.Content = "Created: " + User.CreatedDate.ToString();
            if(!User.Verified.HasValue)
            {
                
                if ((bool)User.Verified)
                {
                    VerifiedLbl.Content = "Verified";
                    VerifiedLbl.Foreground = new SolidColorBrush(Colors.Green);
                }
                else if (!(bool)User.Verified)
                    VerifiedLbl.Content = "Not Verified";
                    VerifiedLbl.Foreground = new SolidColorBrush(Colors.Red);
            }
            try
            {
                if(!String.IsNullOrEmpty(User.Website))
                {
                    WebsiteLabl.Content="Website: "+Environment.NewLine+User.Website;
                }
                else
                {
                    WebsiteLabl.Visibility = Visibility.Hidden;
                }
            }
            catch
            {
                WebsiteLabl.Visibility = Visibility.Hidden;
            }
          
            this.SetAeroGlass();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            int extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_DLGMODALFRAME);
            SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
        }

        private void WebsiteLabl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            string target = User.Website.ToString();
            System.Diagnostics.Process.Start(target);
        }
        
    }
}
