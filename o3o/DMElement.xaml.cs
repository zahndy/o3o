﻿using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Twitterizer;

namespace o3o
{


    /// <summary>
    /// Interaction logic for TweetElement.xaml
    /// </summary>
    public partial class DMElement : UserControl, IDisposable
    {
        public float PolyOpacity
        {
            get { return polyOpacity; }
            set
            {
                polyOpacity = value;
                SolidColorBrush gBrush = new SolidColorBrush(Color.FromArgb((byte)(polyOpacity * 255), 0, 0, 0));
                //messagePolygon.Fill = gBrush;
                if (tweetelementgrid.Children.OfType<Polygon>().FirstOrDefault() != null)
                    tweetelementgrid.Children.OfType<Polygon>().FirstOrDefault().Fill = gBrush;

            }
        }
        //public TweetElement() { }
        Polygon messagePolygon = new Polygon();
        public string name;
        public float polyOpacity = 0.6f;
        public string imagelocation;
        public string ID;
        public bool loaded = false;
        public TwitterDirectMessage Status;
        UserDatabase.User dbUser;
        bool moreusers;
        private MainWindow1 parent;
        public DMElement(MainWindow1 prnt, TwitterDirectMessage status, UserDatabase.User usr, ImageSource Imagesource, bool MoreThanOneUser = false)
        {

            InitializeComponent();
            dbUser = usr;
            moreusers = MoreThanOneUser;
            name = status.Sender.ScreenName;
            tweetImg.Source = Imagesource;
            ID = status.Id.ToString();
            Status = status;

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
                        bool hasperiodend = false;
                        bool hascommaend = false;
                        if(a.StartsWith("."))
                        {
                         TweetBlock.Inlines.Add(new Run(HttpUtility.HtmlDecode(".")));
                         a = a.Remove(0, 1);
                        }
                        if (a.StartsWith(","))
                        {
                            TweetBlock.Inlines.Add(new Run(HttpUtility.HtmlDecode(",")));
                            a = a.Remove(0, 1);
                        }

                        if (a.EndsWith("."))
                        {
                            a = a.Substring(0, a.LastIndexOf("."));
                            hasperiodend = true;
                        }
                        if (a.EndsWith(","))
                        {
                            a = a.Substring(0, a.LastIndexOf(","));
                            hascommaend = true;
                        }
                        
                        if (a.StartsWith("@"))
                        {
                            string username = a.Replace("@", "");
                            username.Replace(":", "");
                            Hyperlink uname = new Hyperlink(new Run(a)) { NavigateUri = new Uri("http://twitter.com/" + username) };
                            uname.RequestNavigate += Hyperlink_RequestNavigateEvent;
                            uname.TextDecorations = null;
                            uname.Foreground = color;
                            TweetBlock.Inlines.Add(uname);
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
                                    }
                                    catch (Exception)
                                    {
                                        TweetBlock.Inlines.Add(a);
                                    }
                                }
                                else
                                {
                                    TweetBlock.Inlines.Add(a);
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
                                    }
                                    catch (Exception)
                                    {
                                        TweetBlock.Inlines.Add(a);
                                    }
                                }
                                else
                                {
                                    TweetBlock.Inlines.Add(a);
                                }
                            }

                        }
                        else
                        {
                            TweetBlock.Inlines.Add(new Run(HttpUtility.HtmlDecode(a)));
                            
                        }

                        if (hasperiodend)
                        {
                            TweetBlock.Inlines.Add(new Run(HttpUtility.HtmlDecode(".")));
                            
                        }
                        if (hascommaend)
                        {
                            TweetBlock.Inlines.Add(new Run(HttpUtility.HtmlDecode(",")));
                            
                        }
                        TweetBlock.Inlines.Add(new Run(" "));
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
                
                AtNameLabel.Text = "@" + Status.Sender.ScreenName;
                NameLabel.Text = Status.Sender.Name;


               
                generatePolygonAndMargins(Status.Text.Length, Status.Text.Trim().Replace("\n", " "));
                loaded = true;

                GC.Collect();
         
            }
        }  

        public void setimage(ImageSource _image)
        {
            tweetImg.Source = _image;
        }

        
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
            #region elementsize
            if (textheight <= 18) //17.29
            {
                points.Add(new Point(10, 6));
                points.Add(new Point(352, 6));
                points.Add(new Point(352, 64));
                points.Add(new Point(25, 64));
                points.Add(new Point(10, 80));

                tweetelementgrid.Height = 85;
                DM_Element.Height = 85;
                TweetBlock.Height = 36;

                AtNameLabel.Margin = new Thickness(26, 64, 0, 0);
                label1.Margin = new Thickness(52, 64, 0, 0);
                replyimageborder.Margin = new Thickness(331, 65, 0, 0);
            }
            else if (textheight > 18 && textheight <= 35) //34.58
            {
                points.Add(new Point(10, 6));
                points.Add(new Point(352, 6));
                points.Add(new Point(352, 74));
                points.Add(new Point(25, 74));
                points.Add(new Point(10, 90));

                tweetelementgrid.Height = 95;
                DM_Element.Height = 95;
                TweetBlock.Height = 50;

                AtNameLabel.Margin = new Thickness(26, 74, 0, 0);
                label1.Margin = new Thickness(52, 74, 0, 0);
                replyimageborder.Margin = new Thickness(331, 75, 0, 0);
            }
            else if (textheight > 35 && textheight <= 52)  //51.87
            {
                points.Add(new Point(10, 6));
                points.Add(new Point(352, 6));
                points.Add(new Point(352, 87));
                points.Add(new Point(25, 87));
                points.Add(new Point(10, 105));

                tweetelementgrid.Height = 110;
                DM_Element.Height = 110;
                TweetBlock.Height = 65;

                AtNameLabel.Margin = new Thickness(26, 87, 0, 0);
                label1.Margin = new Thickness(52, 87, 0, 0);
                replyimageborder.Margin = new Thickness(331, 88, 0, 0);
            }
            else if (textheight > 52 && textheight <= 69) // > 69.16
            {
                points.Add(new Point(10, 6));
                points.Add(new Point(352, 6));
                points.Add(new Point(352, 100));
                points.Add(new Point(25, 100));
                points.Add(new Point(10, 118));

                tweetelementgrid.Height = 125;
                DM_Element.Height = 125;
                TweetBlock.Height = 75;

                AtNameLabel.Margin = new Thickness(26, 100, 0, 0);
                label1.Margin = new Thickness(52, 100, 0, 0);
                replyimageborder.Margin = new Thickness(331, 101, 0, 0);

            }
            else if (textheight > 69 && textheight <= 87) // > 86,45
            {
                points.Add(new Point(10, 6));
                points.Add(new Point(352, 6));
                points.Add(new Point(352, 113));
                points.Add(new Point(25, 113));
                points.Add(new Point(10, 131));

                tweetelementgrid.Height = 140;
                DM_Element.Height = 140;
                TweetBlock.Height = 85;

                AtNameLabel.Margin = new Thickness(26, 113, 0, 0);
                label1.Margin = new Thickness(52, 113, 0, 0);
                replyimageborder.Margin = new Thickness(331, 114, 0, 0);
            }
            else
            {
                points.Add(new Point(10, 6));
                points.Add(new Point(352, 6));
                points.Add(new Point(352, 126));
                points.Add(new Point(25, 126));
                points.Add(new Point(10, 144));

                tweetelementgrid.Height = 153;
                DM_Element.Height = 153;
                TweetBlock.Height = 95;

                AtNameLabel.Margin = new Thickness(26, 126, 0, 0);
                label1.Margin = new Thickness(52, 126, 0, 0);
                replyimageborder.Margin = new Thickness(331, 127, 0, 0);
            }
            #endregion
            SolidColorBrush brush = new SolidColorBrush(Color.FromArgb((byte)(polyOpacity * 255), 0, 0, 0));
            messagePolygon.Fill = brush;
            messagePolygon.Points = points;
            
            tweetelementgrid.Children.Insert(0, messagePolygon);
   
            formattedText = null;
            points = null;
            brush = null;

        }

        private void Hyperlink_RequestNavigateEvent(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.ToString());
        }
        private void AtNameLabel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("http://twitter.com/" + Status.Sender.ScreenName);
        }

        private void datelabel_MouseLeave(object sender, MouseEventArgs e)
        {
            datelabel.Foreground = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255));
            //parent.TweetElements.Cursor = HandOpen;        #C2000000
            parent.TweetElements.Cursor = System.Windows.Input.Cursors.Arrow;
        }

        private void datelabel_MouseEnter(object sender, MouseEventArgs e)
        {
            datelabel.Foreground = new SolidColorBrush(Color.FromArgb(150, 255, 255, 255));
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
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            replyBtn.Source = new BitmapImage(new Uri("/o3o;component/Images/empty.png", UriKind.Relative));
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
            parent.DMreply(Status);
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

        private void linkMouseEnter(object sender, MouseEventArgs e)
        {
            parent.TweetElements.Cursor = System.Windows.Input.Cursors.Hand;
        }

        private void linkMouseLeave(object sender, MouseEventArgs e)
        {
            parent.TweetElements.Cursor = System.Windows.Input.Cursors.Arrow;
        }

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
                replyBtn = null;
                TweetBlock = null;
                tweetImg = null;
                contextmenu = null;
                dbUser = null;
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
                Status = null;
                tweetelementgrid = null;
                DM_Element = null;
            }
        }

    }
}
