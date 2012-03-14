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
        
        public TweetElement()
        {
            
            InitializeComponent();
            TweetBlock.Text = Tweet;
            datelabel.Text = Date;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            TweetBlock.Text = Tweet;
            datelabel.Text = Date;
            label1.Text = name;
            var image = new BitmapImage();
            int BytesToRead=100;
            WebRequest request = WebRequest.Create(new Uri("http://twimg0-a.akamaihd.net/profile_images/1548278996/zandersauce_2_normal.png")); // REPLACE with requestjson(string user)
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
           // string target = URL FOR TWEET HERE;
          //  System.Diagnostics.Process.Start(target);
        }

        public string requestjson(string user)
        {
            StringBuilder sb = new StringBuilder();
            byte[] buf = new byte[8192];
            HttpWebRequest request = (HttpWebRequest)
                WebRequest.Create("http://api.twitter.com/1/users/show.json?screen_name="+name);
            HttpWebResponse response = (HttpWebResponse)
                request.GetResponse();
            Stream resStream = response.GetResponseStream();

            string tempString = null;
            int count = 0;

            do
            {
                count = resStream.Read(buf, 0, buf.Length);
                if (count != 0)
                {
                    tempString = Encoding.ASCII.GetString(buf, 0, count);
                    sb.Append(tempString);
                }
            }
            while (count > 0);

            //sb.ToString()
            // tear apart the JSON here and get the user id from it. 
            // then return the ID
            return "";
        }

    }
}
