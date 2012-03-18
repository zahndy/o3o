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

        public TweetElement()
        {
            
            InitializeComponent();
            TweetBlock.Text = Tweet;
            datelabel.Text = Date;
        }
        public Hyperlink Username(string x)
        {
            string username = x.Replace("@", "");
            Hyperlink link = new Hyperlink();
            link.NavigateUri = new Uri("http://twitter.com/" + username);
            link.RequestNavigate += new RequestNavigateEventHandler(Hyperlink_RequestNavigateEvent);
            link.Inlines.Add(x+" ");
            link.Foreground = new SolidColorBrush(Colors.SkyBlue);
            return link;

        }

        public Hyperlink Hashtag(string x)
        {
            string username = x.Replace("#", "");
            Hyperlink link = new Hyperlink();
            link.NavigateUri = new Uri("http://search.twitter.com/search?q=" + username);
            link.RequestNavigate += new RequestNavigateEventHandler(Hyperlink_RequestNavigateEvent);
            link.Inlines.Add(x + " ");
            link.Foreground = new SolidColorBrush(Colors.SkyBlue);
            return link;

        }

        public Hyperlink http(string x)
        {
            string url = x.Replace("http://", "");
            Hyperlink link = new Hyperlink();
            link.NavigateUri = new Uri(x);
            link.RequestNavigate += new RequestNavigateEventHandler(Hyperlink_RequestNavigateEvent);
            link.Inlines.Add(url + " ");
            link.Foreground = new SolidColorBrush(Colors.SkyBlue);
            return link;

        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var kaas = Tweet.Split(' ');
            foreach (string a in kaas)
            {
                if (a.StartsWith("@"))
                {
                    TweetBlock.Inlines.Add(Username(a));
                }
                else if (a.StartsWith("#"))
                {
                    TweetBlock.Inlines.Add(Hashtag(a));
                }
                else if (a.StartsWith("http"))
                {
                    TweetBlock.Inlines.Add(http(a));
                }
                else
                {
                    TweetBlock.Text += a+" ";
                }
            }

            // find hashtags and @user in Tweet
            //TweetBlock.Text = Tweet.ParseURL().ParseUsername().ParseHashtag();
            
            datelabel.Text = Date;
            label1.Text = name;
            var image = new BitmapImage();
            int BytesToRead=100;
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
        
        private void RequestNavigateHandler(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.ToString());
        }
    }

}
