﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Reflection;
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
using System.Diagnostics;
using System.Net;
using System.IO;
using System.Web;
using System.Globalization;
using System.Text.RegularExpressions;
using Twitterizer;

namespace o3o
{


    /// <summary>
    /// Interaction logic for TweetElement.xaml
    /// </summary>
    public partial class TweetElement : UserControl, IDisposable
    {
        public float PolyOpacity
        {
            get { return polyOpacity; }
            set
            {
                polyOpacity = value;
                SolidColorBrush gBrush = new SolidColorBrush(Color.FromArgb((byte)(polyOpacity * 255), 0, 0, 0));
                messagePolygon.Fill = gBrush;
            }
        }
        public TweetElement() { }

        public string name;
        public float polyOpacity = 0.6f;
        public string Date;
        public string imagelocation;
        public string ID;
        public bool loaded = false;
        TwitterStatus Status;
        UserDatabase.User dbUser;

        private dynamic parent;
        public TweetElement(dynamic prnt, TwitterStatus status, UserDatabase.User usr)
        {

            InitializeComponent();
            dbUser = usr;
            name = status.User.ScreenName;
            Date = status.CreatedDate.Month.ToString() + "/" + status.CreatedDate.Day.ToString() + " " + status.CreatedDate.Hour.ToString() + ":" + status.CreatedDate.Minute.ToString();
            imagelocation = status.User.ProfileImageLocation;
            ID = status.Id.ToString();
            Status = status;
            datelabel.Text = Date;
            parent = prnt;
            SolidColorBrush gBrush = new SolidColorBrush(Color.FromArgb((byte)(polyOpacity * 255), 0, 0, 0));
            messagePolygon.Fill = gBrush;
        }

       

        BitmapImage image = new BitmapImage();
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
                string tweet = Status.Text.Trim().Replace(Environment.NewLine, "");
                string[] kaas = tweet.Split(' ');
                for (int b = 0; b < kaas.Length; b++)
                {
                    string a = kaas[b];
                    if (a.Length > 1)
                    {
                        if (a.StartsWith("@"))
                        {
                            string username = a.Replace("@", "");
                            username.Replace(":", "");
                            Hyperlink uname = new Hyperlink(new Run(a)) { NavigateUri = new Uri("http://twitter.com/" + username) };
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

                            if (a != "http://" && a != "http" && a != "http:" && !String.IsNullOrEmpty(url))
                            {
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
                                TweetBlock.Inlines.Add(a);
                                TweetBlock.Inlines.Add(new Run(" "));
                            }

                        }
                        else
                        {
                            TweetBlock.Inlines.Add(new Run(HttpUtility.HtmlDecode(a)));
                            TweetBlock.Inlines.Add(new Run(" "));
                        }
                    }
                    else
                    {
                        TweetBlock.Inlines.Add(new Run(HttpUtility.HtmlDecode(a)));
                        TweetBlock.Inlines.Add(new Run(" "));
                    }
                }

                datelabel.Text = Date;
                label1.Text = "To: " + dbUser.UserDetails.ScreenName;
                AtNameLabel.Text = "@" + Status.User.ScreenName;
                NameLabel.Text = Status.User.Name;

                if (Status.IsFavorited == true)
                {
                    favBtn.Source = new BitmapImage(new Uri("/o3o;component/Images/facorite_on.png", UriKind.Relative));
                }

                if (imagelocation.Length > 0)
                {
                    try
                    {
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

                        request = null;
                        response = null;
                        responseStream = null;
                        reader = null;
                        memoryStream = null;
                        bytebuffer = null;
                        bytesRead = 0;
                        BytesToRead = 0;
                    }
                    catch (Exception)
                    {
                        tweetImg.Source = new BitmapImage(new Uri("/o3o;component/Images/image_Failed.png", UriKind.Relative));
                    }
                }
                generatePolygonAndMargins(Status.Text.Length, Status.Text);
                loaded = true;

                image = null;
                GC.Collect();
            }
        }
        Polygon messagePolygon = new Polygon();
        void generatePolygonAndMargins(int charlength, string text)
        {
            FormattedText formattedText = new FormattedText(
            text,
            CultureInfo.GetCultureInfo("en-us"),
            FlowDirection.LeftToRight,
            new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
            13,
            Brushes.Black);
            formattedText.MaxTextWidth = 346;
            formattedText.MaxTextHeight = 65;
            double textheight = formattedText.Height;

            messagePolygon.Name = "messagePolygon";
            PointCollection points = new PointCollection();
            if (textheight <= 18) //17.29
            {
                points.Add(new Point(10, 6));
                points.Add(new Point(397, 6));
                points.Add(new Point(397, 64));
                points.Add(new Point(25, 64));
                points.Add(new Point(10, 80));

                tweetelementgrid.Height = 85;
                tweetElement.Height = 85;
                TweetBlock.Height = 36;

                datelabel.Margin = new Thickness(26, 64, 0, 0);
                label1.Margin = new Thickness(113, 64, 0, 0);
                replyimageborder.Margin = new Thickness(337, 65, 0, 0);
                retweetimageborder.Margin = new Thickness(359, 65, 0, 0);
                favimageborder.Margin = new Thickness(381, 65, 0, 0);
            }
            else if ((textheight > 18 && textheight <= 35) && charlength <= 113) //34.58
            {
                points.Add(new Point(10, 6));
                points.Add(new Point(397, 6));
                points.Add(new Point(397, 74));
                points.Add(new Point(25, 74));
                points.Add(new Point(10, 90));

                tweetelementgrid.Height = 95;
                tweetElement.Height = 95;
                TweetBlock.Height = 50;

                datelabel.Margin = new Thickness(26, 74, 0, 0);
                label1.Margin = new Thickness(113, 74, 0, 0);
                replyimageborder.Margin = new Thickness(337, 75, 0, 0);
                retweetimageborder.Margin = new Thickness(359, 75, 0, 0);
                favimageborder.Margin = new Thickness(381, 75, 0, 0);
            }
            else  //(textheight > 35) //51.87
            {

                points.Add(new Point(10, 6));
                points.Add(new Point(397, 6));
                points.Add(new Point(397, 87));
                points.Add(new Point(25, 87));
                points.Add(new Point(10, 105));

                tweetelementgrid.Height = 110;
                tweetElement.Height = 110;
                TweetBlock.Height = 65;

                datelabel.Margin = new Thickness(26, 87, 0, 0);
                label1.Margin = new Thickness(113, 87, 0, 0);
                replyimageborder.Margin = new Thickness(337, 90, 0, 0);
                retweetimageborder.Margin = new Thickness(359, 90, 0, 0);
                favimageborder.Margin = new Thickness(381, 90, 0, 0);
            }
            SolidColorBrush brush = new SolidColorBrush(Color.FromArgb((byte)(polyOpacity * 255), 0, 0, 0));
            messagePolygon.Fill = brush;
            messagePolygon.Points = points;
            tweetelementgrid.Children.Insert(0, messagePolygon);
            /*
               <Polygon Name="messagePolygon"
            Points="10,6 397,6 397,89 25,89 10,105">
                <Polygon.Fill>
                    <SolidColorBrush Color="Black" Opacity="0.4" />
                </Polygon.Fill>
            </Polygon>
                 
             */

            formattedText = null;
            points = null;
            brush = null;
            messagePolygon = null;

        }

        private void Hyperlink_RequestNavigateEvent(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.ToString());
        }
        private void AtNameLabel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("http://twitter.com/" + Status.User.ScreenName);
        }

        //private void AtNameLabel_MouseEnter(object sender, MouseEventArgs e)
        //{
        //    label1.Foreground = new SolidColorBrush(Colors.SkyBlue); 
        //}

        //private void AtNameLabel_MouseLeave(object sender, MouseEventArgs e)
        //{
        //    label1.Foreground = new SolidColorBrush(Colors.Black);
        //}

        private void datelabel_MouseLeave(object sender, MouseEventArgs e)
        {
            datelabel.Foreground = new SolidColorBrush(Color.FromArgb(150, 0, 0, 0));
            //parent.TweetElements.Cursor = HandOpen;
        }

        private void datelabel_MouseEnter(object sender, MouseEventArgs e)
        {
            datelabel.Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
            parent.TweetElements.Cursor = System.Windows.Input.Cursors.Hand;
        }

        private void datelabel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            string target = "https://twitter.com/#!/" + name + "/statuses/" + ID;
            System.Diagnostics.Process.Start(target);
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            replyBtn.Source = new BitmapImage(new Uri("/o3o;component/Images/reply.png", UriKind.Relative));

            if (Status.IsFavorited == true)
            {
                favBtn.Source = new BitmapImage(new Uri("/o3o;component/Images/favorite_on.png", UriKind.Relative));
            }
            else
            {
                favBtn.Source = new BitmapImage(new Uri("/o3o;component/Images/favorite.png", UriKind.Relative));
            }

            if (Status.Retweeted)
            {
                retweetBtn.Source = new BitmapImage(new Uri("/o3o;component/Images/retweet_on.png", UriKind.Relative));
            }
            else
            {
                retweetBtn.Source = new BitmapImage(new Uri("/o3o;component/Images/retweet.png", UriKind.Relative));
            }

        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            replyBtn.Source = new BitmapImage(new Uri("/o3o;component/Images/empty.png", UriKind.Relative));
            retweetBtn.Source = new BitmapImage(new Uri("/o3o;component/Images/empty.png", UriKind.Relative));
            favBtn.Source = new BitmapImage(new Uri("/o3o;component/Images/empty.png", UriKind.Relative));

        }

        private void replyBtn_MouseEnter(object sender, MouseEventArgs e)
        {
            replyBtn.Source = new BitmapImage(new Uri("/o3o;component/Images/reply_hover.png", UriKind.Relative));
        }

        private void replyBtn_MouseLeave(object sender, MouseEventArgs e)
        {
            replyBtn.Source = new BitmapImage(new Uri("/o3o;component/Images/reply.png", UriKind.Relative));
        }

        private void replyBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            parent.reply(Status);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(Status.Text);
        }

        private void TweetBlock_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            contextmenu.PlacementTarget = this;
            contextmenu.IsOpen = true;
        }


        private void tweetImg_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            tweetImg.Source = new BitmapImage(new Uri("/o3o;component/Images/image_Failed.png", UriKind.Relative));
        }


        //  removed dragscroll, laggy and not working
        #region DragScroll
        //System.Windows.Input.Cursor HandClosed = new System.Windows.Input.Cursor(Assembly.GetExecutingAssembly().GetManifestResourceStream("o3o.Images.closedhand.cur"));
        //System.Windows.Input.Cursor HandOpen = new System.Windows.Input.Cursor(Assembly.GetExecutingAssembly().GetManifestResourceStream("o3o.Images.openhand.cur"));
        #endregion

        private void favBtn_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Status.IsFavorited == true)
            {
                favBtn.Source = new BitmapImage(new Uri("/o3o;component/Images/favorite_on.png", UriKind.Relative));
            }
            else
            {
                favBtn.Source = new BitmapImage(new Uri("/o3o;component/Images/favorite_hover.png", UriKind.Relative));
            }

        }

        private void favBtn_MouseLeave(object sender, MouseEventArgs e)
        {
            if (Status.IsFavorited == true)
            {
                favBtn.Source = new BitmapImage(new Uri("/o3o;component/Images/favorite_on.png", UriKind.Relative));
            }
            else
            {
                favBtn.Source = new BitmapImage(new Uri("/o3o;component/Images/favorite.png", UriKind.Relative));
            }
        }

        private void favBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Status.IsFavorited == true)
            {
                parent.unfavoriteTweet(Status.Id, dbUser.UserDetails.ScreenName);
                favBtn.Source = new BitmapImage(new Uri("/o3o;component/Images/favorite_hover.png", UriKind.Relative));
                Status.IsFavorited = false;
            }
            else
            {
                parent.favoriteTweet(Status.Id, dbUser.UserDetails.ScreenName);
                favBtn.Source = new BitmapImage(new Uri("/o3o;component/Images/favorite_on.png", UriKind.Relative));
                Status.IsFavorited = true;
            }
        }

        private void retweetBtn_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Status.Retweeted)
            {
                retweetBtn.Source = new BitmapImage(new Uri("/o3o;component/Images/retweet_on.png", UriKind.Relative));
            }
            else
            {
                retweetBtn.Source = new BitmapImage(new Uri("/o3o;component/Images/retweet_hover.png", UriKind.Relative));
            }
        }

        private void retweetBtn_MouseLeave(object sender, MouseEventArgs e)
        {
            if (Status.Retweeted)
            {
                retweetBtn.Source = new BitmapImage(new Uri("/o3o;component/Images/retweet_on.png", UriKind.Relative));
            }
            else
            {
                retweetBtn.Source = new BitmapImage(new Uri("/o3o;component/Images/retweet.png", UriKind.Relative));
            }
        }

        private void retweetBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!Status.Retweeted)
            {
                parent.retweet(Status.Id, dbUser.UserDetails.ScreenName);
                retweetBtn.Source = new BitmapImage(new Uri("/o3o;component/Images/retweet_on.png", UriKind.Relative));
                Status.Retweeted = true;
            }
        }

        private void linkMouseEnter(object sender, MouseEventArgs e)
        {
            parent.TweetElements.Cursor = System.Windows.Input.Cursors.Hand;
        }

        private void linkMouseLeave(object sender, MouseEventArgs e)
        {
           //parent.TweetElements.Cursor = HandOpen;
        }

        //public ~TweetElement(){}

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                AtNameLabel = null;
                datelabel = null;
                favBtn = null;
                replyBtn = null;
                retweetBtn = null;
                TweetBlock = null;
                tweetImg = null;
                contextmenu = null;
                Date = null;
                dbUser = null;
                favimageborder = null;
                ID = null;
                image = null;
                imageborder = null;
                imagelocation = null;
                label1 = null;
                messagePolygon = null;
                name = null;
                NameLabel = null;
                parent = null;
                polyOpacity = 0;
                replyBtn = null;
                replyimageborder = null;
                retweetBtn = null;
                retweetimageborder = null;
                Status = null;
                tweetelementgrid = null;
                tweetElement = null;
            }
        }

    }
}