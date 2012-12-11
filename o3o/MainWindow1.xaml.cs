using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
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
using System.Threading;
using System.Net;
using System.Windows.Forms;
using System.Xml;
using System.Windows.Markup;
using System.Diagnostics;
using System.IO;
using Twitterizer;
using OpenAL;
using Unf;

namespace o3o
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow1 : Window
    {
        public static UserDatabase UsrDB = new UserDatabase();
        public delegate void dostuff(TwitterStatus status, UserDatabase.User _usr);
        public dostuff dostuffdel;
        public Screen[] Displays = System.Windows.Forms.Screen.AllScreens;
        public float polygonOpacity = o3o.Properties.Settings.Default.PolygonOpacity;
        public bool isloaded = false;
        public bool inreply = false;
        public TwitterStatus replystatus;
        public struct SoundFile
        {
            public string soundname, filepath, extension;
        }
        public SoundFile CurrentSelectedSound;
        public int Volume = o3o.Properties.Settings.Default.Volume;
        public List<SoundFile> sounds = new List<SoundFile>();
        System.Windows.Threading.Dispatcher maindispatcher;
        public int FBuffer;
        public int FSource;
        public ContextAL FContext;
        ImageHandler ImageCache = new ImageHandler();
        System.Timers.Timer _timer;

        #region loading stuff
       

        public MainWindow1()
        {
            InitializeComponent();

            MouseDown += delegate { if (MouseButtonState.Pressed == System.Windows.Input.Mouse.LeftButton) { DragMove(); } };
            this.Loaded += new RoutedEventHandler(Window_Loaded);
        }

        void Window_Loaded(object sender, RoutedEventArgs e)
        {
            changeUI(o3o.Properties.Settings.Default.Layout);
            this.SetAeroGlass();

            this.Left = o3o.Properties.Settings.Default.LastWindowPosition.X;
            this.Top = o3o.Properties.Settings.Default.LastWindowPosition.Y;
            this.Height = o3o.Properties.Settings.Default.LastWindowHeight;

            this.TweetsDisplaySlider.Value = o3o.Properties.Settings.Default.amountOfTWeetsToDisplay;
            loadsounds();

            if (o3o.Properties.Settings.Default.use_system_color == true)
            {
                this.checkBox1.IsChecked = true;
            }

            if (UsrDB.load() == false || UsrDB.Users.Count == 0)
                UsrDB.CreateUser();
            this.UserSelectionMenuCurrentName.Header = UsrDB.Users[0].UserDetails.ScreenName;

            #region displaystuff
            this.DisplaysComboBox.SelectedIndex = o3o.Properties.Settings.Default.DisplayIndex;
            int dex;
            int upperBound = Displays.GetUpperBound(0);
            for (dex = 0; dex <= upperBound; dex++)
            {
                if (Displays[dex].Primary)
                {
                    this.DisplaysComboBox.Items.Add(dex + ": " + Displays[dex].DeviceName + " Width:" + Displays[dex].Bounds.Width.ToString() + " Height:" + Displays[dex].Bounds.Height.ToString() + " (primary)");
                }
                else
                {
                    string entry = dex + ": " + Displays[dex].DeviceName + " Width:" + Displays[dex].Bounds.Width.ToString() + " Height:" + Displays[dex].Bounds.Height.ToString();
                    System.Windows.Point primaryscreen = new System.Windows.Point(Screen.PrimaryScreen.Bounds.X, Screen.PrimaryScreen.Bounds.Y);
                    if (Displays[dex].Bounds.X < primaryscreen.X)
                    {
                        if (Displays[dex].Bounds.Y > primaryscreen.Y)
                        {
                            entry = entry + " (Bottom Left)";
                        }
                        else if (Displays[dex].Bounds.Y < primaryscreen.Y)
                        {
                            entry = entry + " (Top Left)";
                        }
                        else
                        {
                            entry = entry + " (Left)";
                        }
                    }
                    else if (Displays[dex].Bounds.X > primaryscreen.X)
                    {
                        if (Displays[dex].Bounds.Y > primaryscreen.Y)
                        {
                            entry = entry + " (Bottom Right)";
                        }
                        else if (Displays[dex].Bounds.Y < primaryscreen.Y)
                        {
                            entry = entry + " (Top Right)";
                        }
                        else
                        {
                            entry = entry + " (Right)";
                        }
                    }
                    else if (Displays[dex].Bounds.X == primaryscreen.X)
                    {
                        if (Displays[dex].Bounds.Y < primaryscreen.Y)
                        {
                            entry = entry + " (Bottom)";
                        }
                        else if (Displays[dex].Bounds.Y > primaryscreen.Y)
                        {
                            entry = entry + " (Top)";
                        }
                        else
                        {
                            entry = entry + " (What?)";
                        }
                    }
                    this.DisplaysComboBox.Items.Add(entry);
                }

            }
            #endregion
            maindispatcher = this.Dispatcher;
            foreach (UserDatabase.User usr in UsrDB.Users)
            {
                usr.tweetStack.NewTweet += new TweetStack.newtweetDel(o3o_NewTweet);
                usr.tweetStack.DMReceived += new TweetStack.DMReceivedDel(o3o_NewDM);
                usr.tweetStack.TweetDeleted += new TweetStack.TweetDeletedDel(o3o_TweetDeleted);
            }

            UpdateUserMenu(UsrDB);

            //_timer = new System.Timers.Timer(5);
            //_timer.Enabled = true;
            //_timer.AutoReset = true;
            //_timer.Elapsed += new System.Timers.ElapsedEventHandler(_timer_Elapsed);
            //_timer.Start();

        }

        //delegate void sausages(object sender, System.Timers.ElapsedEventArgs e);
        //void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        //{
        //    foreach (TweetElement tweet in this.TweetElements.Items)  
        //    {
        //        TimeSpan Difference = DateTime.Now.Subtract(tweet.Status.CreatedDate);

        //        if(Difference.Hours > 0)
        //        {
        //            tweet.datelabel.Text = Difference.Hours.ToString() + "h";
        //        }
        //        else if (Difference.Hours < 1 && Difference.Minutes > 1)
        //        {
        //            tweet.datelabel.Text = Difference.Minutes.ToString() + "m";
        //        }
        //        else if (Difference.Minutes < 1)
        //        {
        //            tweet.datelabel.Text = Difference.Seconds.ToString() + "s";
        //        }
        //    }
        //}

        public void UpdateUserMenu(UserDatabase usrDB)
        {
            this.UserSelectionMenu.Items.Clear();
            foreach (UserDatabase.User usr in usrDB.Users)
            {
                System.Windows.Controls.MenuItem newMenuItem1 = new System.Windows.Controls.MenuItem(); // here you add more users to the menu, also the events when the user selects something 
                newMenuItem1.Header = usr.UserDetails.ScreenName;
                newMenuItem1.Click += new RoutedEventHandler(newMenuItem1_Click);
                this.UserSelectionMenu.Items.Add(newMenuItem1);
            }

        }
        void newMenuItem1_Click(object sender, RoutedEventArgs e)
        {
            this.UserSelectionMenuCurrentName.Header = ((System.Windows.Controls.MenuItem)sender).Header; //string of current user
        }


        #endregion

        #region tweet helpers


        public void SendTweet()
        {
            if (this.textBox1.Text.Length <= 140)
            {
                if (!String.IsNullOrEmpty(this.textBox1.Text))
                {
                    if (replystatus != null && !String.IsNullOrEmpty(replystatus.StringId))
                    {
                        if (inreply && this.textBox1.Text.StartsWith("@" + replystatus.User.ScreenName))
                        {
                            UsrDB.Users.Find(u => u.UserDetails.ScreenName == this.UserSelectionMenuCurrentName.Header.ToString()).tweetStack.Twitter.Reply(replystatus.Id, this.textBox1.Text);
                            this.textBox1.Text = "";
                            this.charleft.Text = "140";

                            this.testbutton.Content = "Tweet";
                            this.TweetElements.Margin = new Thickness(0, 0, 0, 17);
                            this.textBox1.Visibility = Visibility.Collapsed;
                            this.charleft.Visibility = Visibility.Collapsed;
                            this.TweetLbl.Visibility = Visibility.Collapsed;
                            inreply = false;
                            replystatus = null;
                        }
                    }
                    else
                    {
                        UsrDB.Users.Find(u => u.UserDetails.ScreenName == this.UserSelectionMenuCurrentName.Header.ToString()).tweetStack.Twitter.SendTweet(this.textBox1.Text);
                        this.textBox1.Text = "";
                        this.charleft.Text = "140";

                        this.testbutton.Content = "Tweet";
                        this.TweetElements.Margin = new Thickness(0, 0, 0, 17);
                        this.textBox1.Visibility = Visibility.Collapsed;
                        this.charleft.Visibility = Visibility.Collapsed;
                        this.TweetLbl.Visibility = Visibility.Collapsed;
                        inreply = false;
                        replystatus = null;
                    }
                }
                else
                {
                    this.testbutton.Content = "Tweet";
                    this.TweetElements.Margin = new Thickness(0, 0, 0, 17);
                    this.textBox1.Visibility = Visibility.Collapsed;
                    this.charleft.Visibility = Visibility.Collapsed;
                    this.TweetLbl.Visibility = Visibility.Collapsed;
                    this.charleft.Foreground = new SolidColorBrush(Colors.Red);
                    this.charleft.Text = "no text";
                    inreply = false;
                    replystatus = null;
                }
            }
            else
            {
                this.charleft.Foreground = new SolidColorBrush(Colors.Red);
                this.charleft.Text = "too long";
            }

        }



        void o3o_NewTweet(TwitterStatus status, UserDatabase.User _usr)
        {
            dostuffdel = new dostuff(FillHome);
            maindispatcher.Invoke(dostuffdel, new object[] { status, _usr });

            dostuffdel = new dostuff(Notification);
            maindispatcher.Invoke(dostuffdel, new object[] { status, _usr });

        }

        void o3o_TweetDeleted(Twitterizer.Streaming.TwitterStreamDeletedEvent deletedreason)
        {

            //The calling thread cannot access this object because a different thread owns it.

            //foreach (TweetElement tweet in this.TweetElements.Items)
            //{
            //    if (tweet.tweetElement.ID == deletedreason.Id.ToString())
            //    {
            //        this.TweetElements.Items.Remove(tweet);
            //    }
            //}

        }

        void o3o_NewDM(TwitterDirectMessage DM, UserDatabase.User _usr)  // PLZ CHECK IF WORK
        {
            DMElement element = new DMElement(this, DM, _usr);
            element.polyOpacity = polygonOpacity;
            this.TweetMessages.Items.Add(element);
            if (this.TweetMessages.Items.Count > o3o.Properties.Settings.Default.amountOfTWeetsToDisplay)
            {
                this.TweetMessages.Items.RemoveAt(this.TweetElements.Items.Count);
            }
        }

        #region backgroundworker

        //private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        //{
        //    BackgroundWorker worker = sender as BackgroundWorker;
        //    Tuple<string, string> parameters = e.Argument as Tuple<string, string>;

        //    ImageSource source;

        //    if (parameters.Item2.Length > 0)
        //    {
        //        try
        //        {
        //            int BytesToRead = 100;
        //            WebRequest request = WebRequest.Create(new Uri(parameters.Item2));
        //            request.Timeout = -1;
        //            WebResponse response = request.GetResponse();
        //            Stream responseStream = response.GetResponseStream();
        //            BinaryReader reader = new BinaryReader(responseStream);
        //            MemoryStream memoryStream = new MemoryStream();

        //            byte[] bytebuffer = new byte[BytesToRead];
        //            int bytesRead = reader.Read(bytebuffer, 0, BytesToRead);

        //            while (bytesRead > 0)
        //            {
        //                memoryStream.Write(bytebuffer, 0, bytesRead);
        //                bytesRead = reader.Read(bytebuffer, 0, BytesToRead);
        //            }
        //            BitmapImage image = new BitmapImage();
        //            image.BeginInit();
        //            memoryStream.Seek(0, SeekOrigin.Begin);

        //            image.StreamSource = memoryStream;
        //            image.EndInit();

        //            source = image;

        //            request = null;
        //            response = null;
        //            responseStream = null;
        //            reader = null;
        //            memoryStream = null;
        //            bytebuffer = null;
        //            bytesRead = 0;
        //            BytesToRead = 0;
        //        }
        //        catch (Exception)
        //        {
        //            source = new BitmapImage(new Uri("/o3o;component/Images/image_Failed.png", UriKind.Relative));
        //        }
        //        e.Result = Tuple.Create<string, ImageSource>(parameters.Item1, source);
        //    }
 
        //}

        //private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        //{
        //    if (e.Cancelled == true)
        //    {

        //    }
        //    else if (e.Error != null)
        //    {

        //    }
        //    else
        //    {
        //        Tuple<string, ImageSource> res = e.Result as Tuple<string, ImageSource>;

        //        if (TweetElements.Dispatcher.CheckAccess())
        //        {
        //            setimg(res);
        //        }
        //         else
        //        {
        //            TweetElements.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new setimgDel(setimg), new object[] {res});
        //        }
        //    }
        //}

        //private delegate void setimgDel(Tuple<string, ImageSource> res);
        //void setimg(Tuple<string, ImageSource> res)
        //{

        //    foreach (TweetElement tweet in this.TweetElements.Items)  //The calling thread cannot access this object because a different thread owns it.
        //    {
        //            if (tweet.tweetElement.ID == res.Item1.ToString())
        //            {
        //                tweet.tweetImg.Source = res.Item2;
        //            }

        //    }
           
        //}

        #endregion

        public void FillHome(TwitterStatus status, UserDatabase.User _usr)
        {
            if (status.InReplyToScreenName == UsrDB.Users.Find(u => u.UserDetails.ScreenName == _usr.UserDetails.ScreenName).UserDetails.ScreenName)
            {
                FillMentions(status, _usr);
            }
            
            TweetElement element = new TweetElement(this, status, _usr, ImageCache.GetImage(status.User.Id, status.User.ProfileImageLocation));
            element.polyOpacity = polygonOpacity;
            this.TweetElements.Items.Insert(0, element);
            
            //BackgroundWorker backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            //backgroundWorker1.DoWork +=
            //   new DoWorkEventHandler(backgroundWorker1_DoWork);
            //backgroundWorker1.RunWorkerCompleted +=
            //    new RunWorkerCompletedEventHandler(
            //backgroundWorker1_RunWorkerCompleted);


            //Tuple<string, string> _params = Tuple.Create<string, string>(element.ID, status.User.ProfileImageLocation);

            //backgroundWorker1.RunWorkerAsync(_params);


            if (this.TweetElements.Items.Count > o3o.Properties.Settings.Default.amountOfTWeetsToDisplay)
            {
                TweetElement el = (TweetElement)this.TweetElements.Items[this.TweetElements.Items.Count - 1];
                this.TweetElements.Items.Remove(el);
                el.Dispose();
            }

        }

        public void FillMentions(TwitterStatus status, UserDatabase.User _usr)
        {
            TweetElement element = new TweetElement(this, status, _usr, ImageCache.GetImage(status.User.Id, status.User.ProfileImageLocation));
            element.polyOpacity = polygonOpacity;
            this.TweetMentions.Items.Insert(0, element);
            if (this.TweetMentions.Items.Count > o3o.Properties.Settings.Default.amountOfTWeetsToDisplay)
            {
                TweetElement el = (TweetElement)this.TweetMentions.Items[this.TweetElements.Items.Count - 1];
                this.TweetMentions.Items.Remove(el);
                el.Dispose();
            }
        }


        public void Notification(TwitterStatus status, UserDatabase.User _usr)
        {
            notify notification = new notify(this);
            TweetElement element = new TweetElement(this, status, _usr, ImageCache.GetImage(status.User.Id, status.User.ProfileImageLocation));

            element.polyOpacity = polygonOpacity;
            element.replyBtn.Source = new BitmapImage(new Uri("/o3o;component/Images/reply.png", UriKind.Relative));
            notification.content.Items.Add(element);
            playsound();
        }

        public void reply(TwitterStatus Status)
        {
            inreply = true;
            replystatus = Status;

            textBox1.Text = "@" + Status.User.ScreenName + " ";
            if (textBox1.Visibility == Visibility.Collapsed)
            {
                testbutton.Content = "Tweet";
                TweetElements.Margin = new Thickness(0, 0, 0, 70);
                textBox1.Visibility = Visibility.Visible;
                charleft.Visibility = Visibility.Visible;
                TweetLbl.Visibility = Visibility.Visible;

            }
            textBox1.Focus();

        }

        public void favoriteTweet(decimal id, string user)
        {
            UsrDB.Users.Find(u => u.UserDetails.ScreenName == user).tweetStack.Twitter.favorite(id);
        }
        public void unfavoriteTweet(decimal id, string user)
        {
            UsrDB.Users.Find(u => u.UserDetails.ScreenName == user).tweetStack.Twitter.favorite(id);
        }
        public void retweet(decimal id, string user)
        {
            UsrDB.Users.Find(u => u.UserDetails.ScreenName == user).tweetStack.Twitter.Retweet(id);

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
                textBox1.Focus();
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
                inreply = false;
                replystatus = null;
            }

        }

        
        
        public void tbox(string inc) 
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
            textBox1.Focus();

        }

        private void ClearUserDataButton_Click(object sender, RoutedEventArgs e)
        {
            clearuserdata();
        }

        private void OpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            polygonOpacity = (float)OpacitySlider.Value;
            Properties.Settings.Default.PolygonOpacity = polygonOpacity;
            foreach (TweetElement tweet in TweetElements.Items)
            {
                tweet.PolyOpacity = polygonOpacity;
            }

            foreach (TweetElement tweet in TweetMentions.Items)
            {
                tweet.PolyOpacity = polygonOpacity;
            }
        }

        private void volumeLabel_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (volumeLabel.Text.Length > 1)
                volumeLabel.Text = volumeLabel.Text.Substring(0, 2);
            else if (volumeLabel.Text.Length > 0)
                volumeLabel.Text = volumeLabel.Text.Substring(0, 1);
        }


        private void DisplaysComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Properties.Settings.Default.DisplayIndex = DisplaysComboBox.SelectedIndex;
            Properties.Settings.Default.Save();
        }

        private void topmostcheckbox_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.TopMostNotify = true;
        }

        private void topmostcheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.TopMostNotify = false;
        }

        private void button_Manage_Accounts_Click(object sender, RoutedEventArgs e)
        {
            UserAccounts AccountsWindow = new UserAccounts(UsrDB);
            AccountsWindow.ShowDialog();
            //grid1.Children.Clear(); 
           
        }

        void changeUI(int state)
        {
            if (state == 1)
            {
                LayoutButton1.IsChecked = true;
                LayoutButton2.Checked +=new RoutedEventHandler(LayoutButton2_Checked);
                LayoutButton3.Checked +=new RoutedEventHandler(LayoutButton3_Checked);
            }
            else if (state == 2)
            {
                LayoutButton1.Checked += new RoutedEventHandler(LayoutButton1_Checked);
                LayoutButton2.IsChecked = true;
                LayoutButton3.Checked += new RoutedEventHandler(LayoutButton3_Checked);

                this.Width = 855;
                this.MinWidth = 855;
                this.MaxWidth = 855;

                Grid men = mentionsgrid;

                mentionstabcontent.Children.Remove(men);
                tabControl1.Items.Remove(MentionsTab);

                MentionsTab.Content = null;
                MentionsTab = null;

                men.Width = 416;
                men.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;

                homegrid.Width = 416;
                homegrid.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;

                hometabcontent.Children.Add(men);
                HomeTab.Header = "Home & mentions";
            }
            else if (state == 3)
            {
                LayoutButton1.Checked += new RoutedEventHandler(LayoutButton1_Checked);
                LayoutButton2.Checked += new RoutedEventHandler(LayoutButton2_Checked);
                LayoutButton3.IsChecked = true;

                this.Width = 1271;
                this.MinWidth = 1271;
                this.MaxWidth = 1271;

                Grid men = mentionsgrid;
                Grid mesg = messagesgrid;

                mentionstabcontent.Children.Remove(men);
                messagestabcontent.Children.Remove(mesg);

                tabControl1.Items.Remove(MessagesTab);
                tabControl1.Items.Remove(MentionsTab);

                MentionsTab.Content = null;
                MentionsTab = null;
                MessagesTab.Content = null;
                MessagesTab = null;

                men.Width = 416;
                men.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;

                mesg.Width = 416;
                mesg.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;

                homegrid.Width = 416;
                homegrid.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;

                hometabcontent.Children.Add(men);
                hometabcontent.Children.Add(mesg);
                HomeTab.Header = "Home ,mentions, messages";
            }

        }

        private void LayoutButton1_Checked(object sender, RoutedEventArgs e)
        {
            setLayOut(1);
        }

        private void LayoutButton2_Checked(object sender, RoutedEventArgs e)
        {
            setLayOut(2);
        }

        private void LayoutButton3_Checked(object sender, RoutedEventArgs e)
        {
            setLayOut(3);
        }

        private void Twitter_Button_MouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("http://twitter.com/");
        }

        private void Twitter_Button_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            grid1.Cursor = System.Windows.Input.Cursors.Hand;
        }

        private void Twitter_Button_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            grid1.Cursor = System.Windows.Input.Cursors.Arrow;
        }

        #endregion

        #region misc stuff


        public void clearuserdata()
        {
            if (System.Windows.Forms.MessageBox.Show("R U SUR", "Y U DO DIS", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == System.Windows.Forms.DialogResult.No)
                return;
            UsrDB.WipeUsers();
            o3o.Properties.Settings.Default.Reset();
            o3o.Properties.Settings.Default.Save();
            System.Diagnostics.Process.Start(System.Windows.Application.ResourceAssembly.Location);
            System.Windows.Application.Current.Shutdown();
        }

        public void Window_Closed(object sender, EventArgs e)
        {
            al.DeleteSources(1, new int[1] { FSource });
            al.DeleteBuffers(1, new int[1] { FBuffer });
            FContext.Dispose();
            ImageCache.SaveCache();
            Point WindowPosition = new Point((int)this.Left, (int)this.Top);
            o3o.Properties.Settings.Default.LastWindowPosition = WindowPosition;
            o3o.Properties.Settings.Default.LastWindowHeight = this.Height;
            o3o.Properties.Settings.Default.Save();
        }

        public void setLayOut(int index)
        {
            o3o.Properties.Settings.Default.Layout = index;
            o3o.Properties.Settings.Default.Save();
            System.Diagnostics.Process.Start(System.Windows.Application.ResourceAssembly.Location);
            System.Windows.Application.Current.Shutdown();
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

        #region sound stuff

        public void playsound()
        {
            al.SourcePlay(FSource);
        }

        void loadsounds()
        {

            string path = Directory.GetCurrentDirectory();
            string[] filenameswav = Directory.GetFiles(Directory.GetCurrentDirectory() + "\\Sounds", "*.wav");
            string[] filenamesmp3 = Directory.GetFiles(Directory.GetCurrentDirectory() + "\\Sounds", "*.mp3");

            for (int o = 0; o < filenamesmp3.Length; o++)
            {
                string Sname = filenamesmp3[o];

                SoundFile file = new SoundFile();
                file.soundname = System.IO.Path.GetFileNameWithoutExtension(Sname);
                ConvertMp3 converter = new ConvertMp3(Sname, Directory.GetCurrentDirectory() + @"\Sounds\" + file.soundname + ".wav", Sname);
                converter.ShowDialog();

                file.filepath = Directory.GetCurrentDirectory() + @"\Sounds\" + file.soundname + ".wav";
                file.extension = "wav";

                sounds.Add(file);
            }
            for (int d = 0; d < filenameswav.Length; d++)
            {
                string Sname = filenameswav[d];
                SoundFile file = new SoundFile();
                file.soundname = System.IO.Path.GetFileNameWithoutExtension(Sname);
                file.filepath = Sname;
                file.extension = "wav";
                sounds.Add(file);
            }


            for (int s = 0; s < sounds.Count; s++)
            {
                SoundFile sound = sounds[s];
                System.Windows.Controls.ComboBoxItem SoundMenuItem = new System.Windows.Controls.ComboBoxItem();
                SoundMenuItem.Content = sound.soundname;
                this.soundselection.Items.Add(SoundMenuItem);
            }

            CurrentSelectedSound = sounds[0]; // Bleep.mp3 will be the default sound, unless the user picked a different one previously
            this.soundselection.SelectedIndex = 0;


        }

        public void changesound(int index)
        {
            CurrentSelectedSound = sounds[index];
            if (isloaded)
            {
                al.DeleteSources(1, new int[1] { FSource });
                al.DeleteBuffers(1, new int[1] { FBuffer });
                FContext.Dispose();
            }
            int[] Buf = new int[1];
            FContext = new ContextAL();
            FBuffer = FileWAV.LoadFromFile(CurrentSelectedSound.filepath);
            al.GenSources(1, Buf);
            FSource = Buf[0];
            al.Sourcei(FSource, al.BUFFER, FBuffer);
            al.Sourcef(FSource, al.PITCH, 1.0f);
            float newvol = Volume / 100f;
            al.Sourcef(FSource, al.GAIN, newvol);
            al.Sourcefv(FSource, al.POSITION, new float[3] { 0, 0, 0 });
            al.Sourcefv(FSource, al.VELOCITY, new float[3] { 0, 0, 0 });
            al.Listenerfv(al.POSITION, new float[3] { 0, 0, 0 });
            al.Listenerfv(al.VELOCITY, new float[3] { 0, 0, 0 });
            al.Listenerfv(al.ORIENTATION, new float[6] { 0, 0, -1, 0, 1, 0 });
            isloaded = true;
        }

        public void opensoundfile()
        {

            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.DefaultExt = ".mp3";
            dialog.Filter = "Sound Files(*.mp3;*.wav)|*.mp3;*.wav";
            Nullable<bool> result = dialog.ShowDialog();
            if (result == true)
            {

                SoundFile file = new SoundFile();
                file.soundname = System.IO.Path.GetFileNameWithoutExtension(dialog.FileName);

                if (System.IO.Path.GetExtension(dialog.FileName) == ".wav")
                {
                    File.Copy(dialog.FileName, Directory.GetCurrentDirectory() + "\\Sounds\\" + System.IO.Path.GetFileName(dialog.FileName), true);
                    file.extension = "wav";
                }
                else if (System.IO.Path.GetExtension(dialog.FileName) == ".mp3")
                {
                    ConvertMp3 converter = new ConvertMp3(dialog.FileName, Directory.GetCurrentDirectory() + "\\Sounds\\" + file.soundname + ".wav");
                    converter.ShowDialog();
                    file.extension = "mp3";
                }
                file.filepath = Directory.GetCurrentDirectory() + "\\Sounds\\" + System.IO.Path.GetFileName(dialog.FileName);
                sounds.Add(file);

                System.Windows.Controls.ComboBoxItem SoundMenuItem = new System.Windows.Controls.ComboBoxItem();
                this.soundselection.Items.Add(SoundMenuItem);
            }

        }
        
            private void soundselection_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                changesound(soundselection.SelectedIndex);
                        
            }

            private void playbutton_Click(object sender, RoutedEventArgs e)
            {
                playsound();
            }

            private void button1_Click(object sender, RoutedEventArgs e)
            {
                opensoundfile();
            }

            private void slider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
            {
                Volume = Convert.ToInt32(Math.Round(slider1.Value));
                Properties.Settings.Default.Volume = Volume;
                float newvol = Volume / 100f;
                al.Sourcef(FSource, al.GAIN, newvol);
            }
        #endregion

            private void TweetsDisplaySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
            {
                Properties.Settings.Default.amountOfTWeetsToDisplay = Convert.ToInt32(TweetsDisplaySlider.Value);
                while (TweetElements.Items.Count > Properties.Settings.Default.amountOfTWeetsToDisplay)
                {
                    TweetElements.Items.RemoveAt(TweetElements.Items.Count - 1);
                }
            }


    
            
    }

}
