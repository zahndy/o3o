﻿﻿using System;
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
using System.Web;
using System.Globalization;
using Twitterizer;
using System.Timers;

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
        //public TweetElement() { }
        
        public string name;
        public float polyOpacity = 0.6f;
        public string imagelocation;
        public string ID;
        public bool loaded = false;
        public TwitterStatus Status;
        UserDatabase.User dbUser;
        bool moreusers;
        private MainWindow1 parent;
        public TweetElement(MainWindow1 prnt, TwitterStatus status, UserDatabase.User usr, ImageSource Imagesource, bool MoreThanOneUser = false)
        {

            InitializeComponent();
            dbUser = usr;
            moreusers = MoreThanOneUser;
            name = status.User.ScreenName;
            tweetImg.Source = Imagesource;
            ID = status.Id.ToString();
            Status = status;

            favBtn.MouseDown += new MouseButtonEventHandler(favBtn_MouseDown);

            if (status.Retweeted != true)
            {
                retweetBtn.MouseDown += new MouseButtonEventHandler(retweetBtn_MouseDown);
            }

            parent = prnt;
            SolidColorBrush gBrush = new SolidColorBrush(Color.FromArgb((byte)(polyOpacity * 255), 0, 0, 0));
            messagePolygon.Fill = gBrush;
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
                    //color = new SolidColorBrush(Colors.SkyBlue);
                    color = (SolidColorBrush)new BrushConverter().ConvertFromString(AeroGlassHelper.GetColor());
                }

                TweetBlock.Inlines.Clear();
                string tweet = Status.Text.Trim().Replace("\n", " ");
                string[] kaas = tweet.Split(' ');
                #region textprocessing
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
                            if (a.StartsWith("https://"))
                            {
                                string url = a.Replace("https://", "");

                                if (a != "https://" && a != "https" && a != "https:" && !String.IsNullOrEmpty(url))
                                {
                                    try
                                    {
                                        Hyperlink link = new Hyperlink() { NavigateUri = new Uri(a) };
                                        link.Inlines.Add(url);
                                        link.RequestNavigate += Hyperlink_RequestNavigateEvent;
                                        link.TextDecorations = null;
                                        link.Foreground = color;
                                        TweetBlock.Inlines.Add(link);
                                        TweetBlock.Inlines.Add(new Run(" "));
                                    }
                                    catch (Exception)
                                    {
                                        TweetBlock.Inlines.Add(a);
                                        TweetBlock.Inlines.Add(new Run(" "));
                                    }
                                }
                                else
                                {
                                    TweetBlock.Inlines.Add(a);
                                    TweetBlock.Inlines.Add(new Run(" "));
                                }

                            }
                            else if (a.StartsWith("http://"))
                            {
                                string url = a.Replace("http://", "");

                                if (a != "http://" && a != "http" && a != "http:" && !String.IsNullOrEmpty(url))
                                {
                                    try
                                    {
                                        Hyperlink link = new Hyperlink() { NavigateUri = new Uri(a) };
                                        link.Inlines.Add(url);
                                        link.RequestNavigate += Hyperlink_RequestNavigateEvent;
                                        link.TextDecorations = null;
                                        link.Foreground = color;
                                        TweetBlock.Inlines.Add(link);
                                        TweetBlock.Inlines.Add(new Run(" "));
                                    }
                                    catch (Exception)
                                    {
                                        TweetBlock.Inlines.Add(a);
                                        TweetBlock.Inlines.Add(new Run(" "));
                                    }
                                }
                                else
                                {
                                    TweetBlock.Inlines.Add(a);
                                    TweetBlock.Inlines.Add(new Run(" "));
                                }
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
                #endregion
                if (moreusers)
                {
                    label1.Text = "To: " + dbUser.UserDetails.ScreenName;
                }
                
                AtNameLabel.Text = "@" + Status.User.ScreenName;
                NameLabel.Text = Status.User.Name;

                if (Status.IsFavorited == true)
                {
                    favBtn.Source = new BitmapImage(new Uri("/o3o;component/Images/facorite_on.png", UriKind.Relative));
                }

               
                generatePolygonAndMargins(Status.Text.Length, Status.Text.Trim().Replace("\n", " "));
                loaded = true;

                GC.Collect();
         
            }
        }  

        public void setimage(ImageSource _image)
        {
            tweetImg.Source = _image;
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

            formattedText.MaxTextWidth = 304;
            formattedText.MaxTextHeight = 75;
            double textheight = formattedText.Height;

            messagePolygon.Name = "messagePolygon";
            PointCollection points = new PointCollection();
            if (textheight <= 18) //17.29
            {
                points.Add(new Point(10, 6));
                points.Add(new Point(347, 6));
                points.Add(new Point(347, 64));
                points.Add(new Point(25, 64));
                points.Add(new Point(10, 80));

                tweetelementgrid.Height = 85;
                tweetElement.Height = 85;
                TweetBlock.Height = 36;

                datelabel.Margin = new Thickness(26, 64, 0, 0);
                label1.Margin = new Thickness(60, 64, 0, 0);
                replyimageborder.Margin = new Thickness(287, 65, 0, 0);
                retweetimageborder.Margin = new Thickness(309, 65, 0, 0);
                favimageborder.Margin = new Thickness(331, 65, 0, 0);
            }
            else if (textheight > 18 && textheight <= 35) //34.58
            {
                points.Add(new Point(10, 6));
                points.Add(new Point(347, 6));
                points.Add(new Point(347, 74));
                points.Add(new Point(25, 74));
                points.Add(new Point(10, 90));

                tweetelementgrid.Height = 95;
                tweetElement.Height = 95;
                TweetBlock.Height = 50;

                datelabel.Margin = new Thickness(26, 74, 0, 0);
                label1.Margin = new Thickness(60, 74, 0, 0);
                replyimageborder.Margin = new Thickness(287, 75, 0, 0);
                retweetimageborder.Margin = new Thickness(309, 75, 0, 0);
                favimageborder.Margin = new Thickness(331, 75, 0, 0);
            }
            else if (textheight > 35 && textheight <= 52)  //51.87
            {
                points.Add(new Point(10, 6));
                points.Add(new Point(347, 6));
                points.Add(new Point(347, 87));
                points.Add(new Point(25, 87));
                points.Add(new Point(10, 105));

                tweetelementgrid.Height = 110;
                tweetElement.Height = 110;
                TweetBlock.Height = 65;

                datelabel.Margin = new Thickness(26, 87, 0, 0);
                label1.Margin = new Thickness(60, 87, 0, 0);
                replyimageborder.Margin = new Thickness(287, 90, 0, 0);
                retweetimageborder.Margin = new Thickness(309, 90, 0, 0);
                favimageborder.Margin = new Thickness(331, 90, 0, 0);
            }
            else // > 69.16
            {
                points.Add(new Point(10, 6));
                points.Add(new Point(347, 6));
                points.Add(new Point(347, 100));
                points.Add(new Point(25, 100));
                points.Add(new Point(10, 118));

                tweetelementgrid.Height = 125;
                tweetElement.Height = 125;
                TweetBlock.Height = 75;

                datelabel.Margin = new Thickness(26, 100, 0, 0);
                label1.Margin = new Thickness(113, 100, 0, 0);
                replyimageborder.Margin = new Thickness(287, 100, 0, 0);
                retweetimageborder.Margin = new Thickness(309, 100, 0, 0);
                favimageborder.Margin = new Thickness(331, 100, 0, 0);

            }
            SolidColorBrush brush = new SolidColorBrush(Color.FromArgb((byte)(polyOpacity * 255), 0, 0, 0));
            messagePolygon.Fill = brush;
            messagePolygon.Points = points;
            tweetelementgrid.Children.Insert(0, messagePolygon);

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
            parent.TweetElements.Cursor = System.Windows.Input.Cursors.Arrow;

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
            parent.TweetElements.Cursor = System.Windows.Input.Cursors.Hand;
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
            parent.TweetElements.Cursor = System.Windows.Input.Cursors.Hand;
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
            parent.TweetElements.Cursor = System.Windows.Input.Cursors.Arrow;
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
            parent.TweetElements.Cursor = System.Windows.Input.Cursors.Hand;
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
            parent.TweetElements.Cursor = System.Windows.Input.Cursors.Arrow;
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
                dbUser = null;
                favimageborder = null;
                ID = null;
                imageborder = null;
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