using System;
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
using System.Text.RegularExpressions;
using Twitterizer;

namespace o3o
{


    /// <summary>
    /// Interaction logic for TweetElement.xaml
    /// </summary>
    public partial class TweetElement : UserControl
    {
        public float PolyOpacity
        {
            get { return polyOpacity; }
            set
            {
                polyOpacity = value;
                SolidColorBrush gBrush = new SolidColorBrush(Color.FromArgb((byte)(polyOpacity*255),0,0,0));
                messagePolygon.Fill = gBrush; 
            }
        }

        public string Tweet;
        public string name;
        public float  polyOpacity = 0.6f; 
        public string Date;
        public string imagelocation;
        public string ID;
        public bool loaded = false;
        TwitterStatus Status;

        private dynamic parent;
        public TweetElement(dynamic prnt, TwitterStatus status)
        {
            
            InitializeComponent();

            Tweet = status.Text;
            name = status.User.ScreenName;
            Date = status.CreatedDate.Month.ToString() + "/" + status.CreatedDate.Day.ToString() + " " + status.CreatedDate.Hour.ToString() + ":" + status.CreatedDate.Minute.ToString();
            imagelocation = status.User.ProfileImageLocation;
            ID = status.Id.ToString();
            Status = status;
            TweetBlock.Text = Tweet;
            datelabel.Text = Date;
            parent = prnt;
            SolidColorBrush gBrush = new SolidColorBrush(Color.FromArgb((byte)(polyOpacity*255),0,0,0));
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
                var kaas = Tweet.Split(' ');
                foreach (string a in kaas)
                {
                    if (a.Length > 1)
                    {
                        if (a.StartsWith("@"))
                        {
                            string username = a.Replace("@", "");
                            username.Replace(":", "");
                            Hyperlink uname = new Hyperlink(new Run(a)) { NavigateUri = new Uri("http://twitter.com/" + username) };
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
                label1.Text = name;

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

                }
                catch (Exception err)
                {
                    tweetImg.Source = new BitmapImage(new Uri("/o3o;component/Images/image_Failed.png", UriKind.Relative));
                }
                loaded = true;
            }
        }
        private void Hyperlink_RequestNavigateEvent(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.ToString());
        }
        private void label1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("http://twitter.com/" + Status.User.ScreenName);
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


        private void reply()
        {
            parent.reply("@" + name + " ",Status); 
            
        }

        private void naRetweet_Click(object sender, RoutedEventArgs e)
        {
            //Status.Retweet(  
        }

        private void tweetImg_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            tweetImg.Source = new BitmapImage(new Uri("/o3o;component/Images/image_Failed.png", UriKind.Relative));
        }


        //  IMPROVE SHIT
        #region DragScroll
        //String[] resourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
        System.Windows.Input.Cursor HandClosed = new System.Windows.Input.Cursor(Assembly.GetExecutingAssembly().GetManifestResourceStream("o3o.Images.closedhand.cur"));
        System.Windows.Input.Cursor HandOpen = new System.Windows.Input.Cursor(Assembly.GetExecutingAssembly().GetManifestResourceStream("o3o.Images.openhand.cur"));

        public void ScrollToOffset(int offset)
        {
            ScrollViewer viewer = GetScrollViewer(parent.TweetElements);
            viewer.ScrollToHorizontalOffset(offset);
        }

        public static ScrollViewer GetScrollViewer(System.Windows.Controls.ListBox listBox)
        {
            Border scroll_border = VisualTreeHelper.GetChild(listBox, 0) as Border;
            if (scroll_border is Border)
            {
                ScrollViewer scroll = scroll_border.Child as ScrollViewer;
                if (scroll is ScrollViewer)
                {
                    return scroll;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
        
        private void TweetElements_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            parent.isdown = true;
                parent.TweetElements.Cursor = HandClosed;
                parent.mouseDragStartPoint = e.GetPosition(parent);
                parent.scrollStartOffset.X = GetScrollViewer(parent.TweetElements).HorizontalOffset;
                parent.scrollStartOffset.Y = GetScrollViewer(parent.TweetElements).VerticalOffset;
        }

        private void TweetElements_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            parent.isdown = false;
            parent.TweetElements.Cursor = HandOpen;
        }

        
        private void TweetElements_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (parent.isdown)
            {
                Point mouseDragCurrentPoint = e.GetPosition(parent);

                Point delta = new Point(
           (mouseDragCurrentPoint.X > parent.mouseDragStartPoint.X) ?
           -(mouseDragCurrentPoint.X - parent.mouseDragStartPoint.X) :
           (parent.mouseDragStartPoint.X - mouseDragCurrentPoint.X),
           (mouseDragCurrentPoint.Y > parent.mouseDragStartPoint.Y) ?
           -(mouseDragCurrentPoint.Y - parent.mouseDragStartPoint.Y) :
           (parent.mouseDragStartPoint.Y - mouseDragCurrentPoint.Y));

                // Scroll to the new position. 
                GetScrollViewer(parent.TweetElements).ScrollToHorizontalOffset(parent.scrollStartOffset.X + delta.X);
                GetScrollViewer(parent.TweetElements).ScrollToVerticalOffset(parent.scrollStartOffset.Y + delta.Y);
            }
        }

        private void TweetElements_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            parent.isdown = false;
            parent.TweetElements.Cursor = System.Windows.Input.Cursors.Arrow;
        }

        private void TweetElements_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            parent.TweetElements.Cursor = HandOpen;
        }
        #endregion  
        
       

    }
}
