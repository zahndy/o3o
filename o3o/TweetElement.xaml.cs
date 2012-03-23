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
using System.Diagnostics;
using System.Net;
using System.IO;
using System.Web;
using System.Text.RegularExpressions;

namespace o3o
{


    /// <summary>
    /// Interaction logic for TweetElement.xaml
    /// </summary>
    public partial class TweetElement : UserControl
    {
        public string Tweet;
        public string name;
        public string Image; //?
        public string Date;
        public string imagelocation;
        public string ID;
        public bool loaded = false;
        public TweetElement()
        {
            
            InitializeComponent();
            TweetBlock.Text = Tweet;
            datelabel.Text = Date;
        }
        

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            
            if (loaded == false)
            {
            SolidColorBrush color;
            if (Properties.Settings.Default.use_system_color)
            {
                color = new SolidColorBrush(
                    System.Windows.Media.Color.FromArgb(Properties.Settings.Default.system_color.A,
                                                      Properties.Settings.Default.system_color.R,
                                                      Properties.Settings.Default.system_color.G,
                                                      Properties.Settings.Default.system_color.B));
            }
            else
            {
               color = new SolidColorBrush(Colors.SkyBlue);
            }
            
            TweetBlock.Inlines.Clear();
                var kaas = Tweet.Split(' ');
                foreach (string a in kaas)
                {
                    if (a.StartsWith("@"))
                    {
                        string username = a.Replace("@", "");
                        username.Replace(":", "");
                        Hyperlink uname = new Hyperlink( new Run(a)) { NavigateUri = new Uri("http://twitter.com/" + username) };
                        //uname.Inlines.Add(a);
                        uname.RequestNavigate += Hyperlink_RequestNavigateEvent;
                        uname.TextDecorations = null;
                        uname.Foreground = color;
                        TweetBlock.Inlines.Add(uname);
                        TweetBlock.Inlines.Add(new Run(" "));

                    }
                    else if (a.StartsWith("#"))
                    {
                        string hashtag = a.Replace("#", "");
                        Hyperlink hash = new Hyperlink() { NavigateUri = new Uri("http://search.twitter.com/search?q=" + hashtag) };
                        hash.Inlines.Add(a);
                        hash.RequestNavigate += Hyperlink_RequestNavigateEvent;
                        hash.TextDecorations = null;
                        hash.Foreground = color;
                        TweetBlock.Inlines.Add(hash);
                        TweetBlock.Inlines.Add(new Run(" "));
                    }
                    else if (a.StartsWith("http"))
                    {
                        string url = a.Replace("http://", "");
                        Hyperlink link = new Hyperlink() { NavigateUri = new Uri(a) };
                        link.Inlines.Add(url);
                        link.RequestNavigate += Hyperlink_RequestNavigateEvent;
                        link.TextDecorations = null;
                        link.Foreground = color;
                        TweetBlock.Inlines.Add(link);
                        TweetBlock.Inlines.Add(new Run(" "));
                    }
                    else
                    {
                        TweetBlock.Inlines.Add( new Run(a+ " "));
                    }
                }

                datelabel.Text = Date;
                label1.Text = name;
                var image = new BitmapImage();
                int BytesToRead = 100;
                WebRequest request = WebRequest.Create(new Uri(imagelocation));
                request.Timeout = -1;
                WebResponse response = request.GetResponse();
                Stream responseStream = response.GetResponseStream();
                BinaryReader reader = new BinaryReader(responseStream);
                MemoryStream memoryStream = new MemoryStream();

                byte[] bytebuffer = new byte[BytesToRead];
                int bytesRead = reader.Read(bytebuffer, 0, BytesToRead);

                while (bytesRead > 0)
                {
                    memoryStream.Write(bytebuffer, 0, bytesRead);
                    bytesRead = reader.Read(bytebuffer, 0, BytesToRead);
                }

                image.BeginInit();
                memoryStream.Seek(0, SeekOrigin.Begin);

                image.StreamSource = memoryStream;
                image.EndInit();

                tweetImg.Source = image;
                loaded = true;
            }
        }
        private void Hyperlink_RequestNavigateEvent(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.ToString());
        }
        private void label1_MouseDown(object sender, MouseButtonEventArgs e)
        {

            string target = "http://twitter.com/#!/"+name;
            System.Diagnostics.Process.Start(target);
            
        }

        private void label1_MouseEnter(object sender, MouseEventArgs e)
        {
            label1.Foreground = new SolidColorBrush(Colors.SkyBlue); 
        }

        private void label1_MouseLeave(object sender, MouseEventArgs e)
        {
            label1.Foreground = new SolidColorBrush(Colors.Black);
        }

        private void datelabel_MouseLeave(object sender, MouseEventArgs e)
        {
           datelabel.Foreground = new SolidColorBrush(Color.FromArgb(150,0,0,0)); 
        }

        private void datelabel_MouseEnter(object sender, MouseEventArgs e)
        {
            datelabel.Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0)); 
        }

        private void datelabel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            string target = "https://twitter.com/#!/"+name+"/statuses/"+ID;
            System.Diagnostics.Process.Start(target);
        }




        private void reply()
        {
          //  here, append @name too Mainwindow.textBox1 
            
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            replyBtn.Source = new BitmapImage(new Uri("/o3o;component/Images/reply.png", UriKind.Relative));
            menu.Source = new BitmapImage(new Uri("/o3o;component/Images/gear.png", UriKind.Relative));
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            replyBtn.Source = new BitmapImage(new Uri("/o3o;component/Images/reply_empty.png", UriKind.Relative));
            menu.Source = new BitmapImage(new Uri("/o3o;component/Images/reply_empty.png", UriKind.Relative));
        }

        private void replyBtn_MouseEnter(object sender, MouseEventArgs e)
        {
            replyBtn.Source = new BitmapImage(new Uri("/o3o;component/Images/reply_onhover.png", UriKind.Relative));
        }

        private void replyBtn_MouseLeave(object sender, MouseEventArgs e)
        {
            replyBtn.Source = new BitmapImage(new Uri("/o3o;component/Images/reply.png", UriKind.Relative));
        }

        private void replyBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            replyBtn.Source = new BitmapImage(new Uri("/o3o;component/Images/reply_onclick.png", UriKind.Relative));

            reply();
        }

        private void replyBtn_MouseUp(object sender, MouseButtonEventArgs e)
        {
            replyBtn.Source = new BitmapImage(new Uri("/o3o;component/Images/reply_onhover.png", UriKind.Relative));
        }

        private void menu_MouseDown(object sender, MouseButtonEventArgs e)
        {
            contextmenu.PlacementTarget = this;
            contextmenu.IsOpen = true;
        }

        private void menu_MouseEnter(object sender, MouseEventArgs e)
        {
            menu.Source = new BitmapImage(new Uri("/o3o;component/Images/gear_selected.png", UriKind.Relative));
        }

        private void menu_MouseLeave(object sender, MouseEventArgs e)
        {
            menu.Source = new BitmapImage(new Uri("/o3o;component/Images/gear.png", UriKind.Relative));
        }


        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(TweetBlock.Text.ToString());
        }

        private void TweetBlock_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            contextmenu.PlacementTarget = this;
            contextmenu.IsOpen = true;
        }

    }

}
