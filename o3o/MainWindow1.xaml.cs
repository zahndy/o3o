using OpenAL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using Twitterizer;
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
        public delegate void DelTweetDelegate(string deletedreason);
        public DelTweetDelegate cheese;
        public delegate void clearTweetStackDelegate(string reason);
        public clearTweetStackDelegate bacon;
        public delegate void FetchTweets(UserDatabase.User usr);
        public FetchTweets fetch;
        public delegate void SetTime();
        public SetTime derp;
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
        string AppData = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "o3o");
        System.Timers.Timer _timer;


        #region loading stuff

        [DllImport("kernel32", SetLastError = true)]
        static extern IntPtr LoadLibrary(string lpFileName);

        static bool CheckLibrary(string fileName) // return true if present
        {
            return LoadLibrary(fileName) != IntPtr.Zero;
        }

        public MainWindow1()
        {
            if (!CheckLibrary("OpenAl.dll") && !CheckLibrary("OpenAL32.dll"))
            {
                DialogResult fuckup = System.Windows.Forms.MessageBox.Show("OpenAl is not installed \nPress OK to close and start the download of the \nCreative OpenAL Installer",
         "ERROR",
         MessageBoxButtons.OK,
         MessageBoxIcon.Error,
         MessageBoxDefaultButton.Button1);
                Process.Start("http://connect.creativelabs.com/openal/Downloads/oalinst.zip");
                Process.GetCurrentProcess().Kill(); // added this because the next line isnt working.
                System.Windows.Application.Current.Shutdown(); 
            }
            else
            {
                InitializeComponent();
                MouseDown += delegate { if (MouseButtonState.Pressed == System.Windows.Input.Mouse.LeftButton) { DragMove(); } };
                this.Loaded += new RoutedEventHandler(Window_Loaded);
            }
        }

        void Window_Loaded(object sender, RoutedEventArgs e)
        {
            changeUI(o3o.Properties.Settings.Default.Layout);
            if (this.CheckDwm())
            {
                this.SetAeroGlass(); 
            }
             
            this.Left = o3o.Properties.Settings.Default.LastWindowPosition.X;
            this.Top = o3o.Properties.Settings.Default.LastWindowPosition.Y;
            this.Height = o3o.Properties.Settings.Default.LastWindowHeight;

            this.TweetsDisplaySlider.Value = o3o.Properties.Settings.Default.amountOfTWeetsToDisplay;
            loadsounds();

            if (o3o.Properties.Settings.Default.use_system_color)
            {
                this.checkBox1.IsChecked = true;
            }

            if (o3o.Properties.Settings.Default.PlayNotificationSound)
            {
                this.SoundCheckBox.IsChecked = true;
            }

            if (o3o.Properties.Settings.Default.ShowNotificationPopup)
            {
                this.PopupCheckBox.IsChecked = true;
            }

            if (Properties.Settings.Default.TopMostNotify)
            {
                this.topmostcheckbox.IsChecked = true;
            }
            Color col = System.Windows.Media.Color.FromArgb(Properties.Settings.Default.system_color.A,Properties.Settings.Default.system_color.R,Properties.Settings.Default.system_color.G,Properties.Settings.Default.system_color.B);
            colorpicker.recContent.Fill = new SolidColorBrush(col);
            colorpicker.SelectedColor = col;
            if (UsrDB.load() == false || UsrDB.Users.Count == 0)
                UsrDB.CreateUser();
            if (UsrDB.Users[0].UserDetails != null)
            {
            this.UserSelectionMenuCurrentName.Header = UsrDB.Users[0].UserDetails.ScreenName;
            }
            else
            {
                UsrDB.WipeUsers();
                System.Windows.Application.Current.Shutdown();
            }

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
                usr.tweetStack.clear += new TweetStack.clearTweetStack(o3o_clearTweetStack);
                usr.tweetStack.FetchTweets += new TweetStack.fetchTweets(o3o_FetchTweets);
                prefetch(usr);
            }

            UpdateUserMenu(UsrDB);

            _timer = new System.Timers.Timer(2000);
            _timer.Enabled = true;
            _timer.AutoReset = true;
            _timer.Elapsed += new System.Timers.ElapsedEventHandler(_timer_Elapsed);
            _timer.Start();

        }

        
        void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            derp = new SetTime(settime);
            maindispatcher.Invoke(derp);
        }
        
        public void settime()
        {
            foreach (TweetElement tweet in this.TweetElements.Items)
            {
                TimeSpan Difference = DateTime.Now.Subtract(tweet.Status.CreatedDate);
                SetTweetDate(tweet, Difference);
            }
            foreach (TweetElement mention in this.TweetMentions.Items)
            {
                TimeSpan Difference = DateTime.Now.Subtract(mention.Status.CreatedDate);
                SetTweetDate(mention, Difference);
            }
            foreach (DMElement message in this.TweetMessages.Items)
            {
                TimeSpan Difference = DateTime.Now.Subtract(message.Status.CreatedDate);
                SetTweetDate(message, Difference);
            }
        }

        void SetTweetDate(TweetElement tweet, TimeSpan Difference)
        {
            
            if (Difference.Days > 0)
            {
                tweet.datelabel.Text = tweet.Status.CreatedDate.Day.ToString() + " " + CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(tweet.Status.CreatedDate.Month).Substring(0, 3);
            }
            else if (Difference.Hours > 0)
            {
                tweet.datelabel.Text = Difference.Hours.ToString() + "h";
            }
            else if (Difference.Hours <= 1 && Difference.Minutes >= 1)
            {
                tweet.datelabel.Text = Difference.Minutes.ToString() + "m";
            }
            else if (Difference.Minutes < 1)
            {
                tweet.datelabel.Text = Difference.Seconds.ToString() + "s";
            }
        }

        void SetTweetDate(DMElement tweet, TimeSpan Difference)
        {

            if (Difference.Days > 0)
            {
                tweet.datelabel.Text = tweet.Status.CreatedDate.Day.ToString() + " " + CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(tweet.Status.CreatedDate.Month).Substring(0, 3) +" "+ tweet.Status.CreatedDate.Year;
            }
            else if (Difference.Hours > 0)
            {
                tweet.datelabel.Text = Difference.Hours.ToString() + "h";
            }
            else if (Difference.Hours <= 1 && Difference.Minutes >= 1)
            {
                tweet.datelabel.Text = Difference.Minutes.ToString() + "m";
            }
            else if (Difference.Minutes < 1)
            {
                tweet.datelabel.Text = Difference.Seconds.ToString() + "s";
            }
        }

        void o3o_FetchTweets(UserDatabase.User usr)
        {
            fetch = new FetchTweets(prefetch);
            maindispatcher.Invoke(fetch, new object[] { usr });
        }

        private void prefetch(UserDatabase.User usr)
        {
            TwitterStatusCollection prefetch = usr.tweetStack.Twitter.GetTweets();
            foreach (TwitterStatus status in prefetch)
            {

                TweetElement element = new TweetElement(this, status, usr, ImageCache.GetImage(status.User.Id, status.User.ProfileImageLocation));
                element.polyOpacity = polygonOpacity;
                this.TweetElements.Items.Add(element);

                if (this.TweetElements.Items.Count > o3o.Properties.Settings.Default.amountOfTWeetsToDisplay)
                {
                    TweetElement el = (TweetElement)this.TweetElements.Items[this.TweetElements.Items.Count - 1];
                    this.TweetElements.Items.Remove(el);
                    el.Dispose();
                }
            }

            TwitterStatusCollection prefetchMentions = usr.tweetStack.Twitter.GetMentions();

            foreach (TwitterStatus status in prefetchMentions)
            {

                TweetElement element = new TweetElement(this, status, usr, ImageCache.GetImage(status.User.Id, status.User.ProfileImageLocation));
                element.polyOpacity = polygonOpacity;
                this.TweetMentions.Items.Add(element);

                if (this.TweetElements.Items.Count > o3o.Properties.Settings.Default.amountOfTWeetsToDisplay)
                {
                    TweetElement el = (TweetElement)this.TweetMentions.Items[this.TweetMentions.Items.Count - 1];
                    this.TweetMentions.Items.Remove(el);
                    el.Dispose();
                }
            }

            TwitterDirectMessageCollection fetchmessages = usr.tweetStack.Twitter.GetMessages();

            foreach (TwitterDirectMessage message in fetchmessages)
            {

                DMElement element = new DMElement(this, message, usr, ImageCache.GetImage(message.SenderId, message.Sender.ProfileImageLocation));
                element.polyOpacity = polygonOpacity;
                this.TweetMessages.Items.Add(element);

                if (this.TweetMessages.Items.Count > o3o.Properties.Settings.Default.amountOfTWeetsToDisplay)
                {
                    DMElement el = (DMElement)this.TweetMessages.Items[this.TweetMessages.Items.Count - 1];
                    this.TweetMessages.Items.Remove(el);
                    el.Dispose();
                }
            }
        }

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

                            this.TweetButton.Content = "Tweet";
                            this.TweetElements.Margin = new Thickness(0, 0, 0, 17);
                            this.TweetMentions.Margin = new Thickness(0, 0, 0, 17);
                            this.TweetMessages.Margin = new Thickness(0, 0, 0, 17);
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

                        this.TweetButton.Content = "Tweet";
                        this.TweetElements.Margin = new Thickness(0, 0, 0, 17);
                        this.TweetMentions.Margin = new Thickness(0, 0, 0, 17);
                        this.TweetMessages.Margin = new Thickness(0, 0, 0, 17);
                        this.textBox1.Visibility = Visibility.Collapsed;
                        this.charleft.Visibility = Visibility.Collapsed;
                        this.TweetLbl.Visibility = Visibility.Collapsed;
                        inreply = false;
                        replystatus = null;
                    }
                }
                else
                {
                    this.TweetButton.Content = "Tweet";
                    this.TweetElements.Margin = new Thickness(0, 0, 0, 17);
                    this.TweetMentions.Margin = new Thickness(0, 0, 0, 17);
                    this.TweetMessages.Margin = new Thickness(0, 0, 0, 17);
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
            maindispatcher.Invoke(dostuffdel, new object[] { status, _usr});

            dostuffdel = new dostuff(Notification);
            maindispatcher.Invoke(dostuffdel, new object[] { status, _usr });

        }

        void o3o_TweetDeleted(Twitterizer.Streaming.TwitterStreamDeletedEvent deletedreason)
        {
            cheese = new DelTweetDelegate(deleteTweet);
            maindispatcher.Invoke(cheese, new object[] { deletedreason.Id.ToString() });
        }
        
        public void deleteTweet(string ID)
        {
            foreach (TweetElement tweet in this.TweetElements.Items)
            {
                if (tweet.tweetElement.ID == ID)
                {
                    this.TweetElements.Items.Remove(tweet);
                    break;
                }
            }

            foreach (TweetElement mention in this.TweetMentions.Items)
            {
                if (mention.tweetElement.ID == ID)
                {
                    this.TweetMentions.Items.Remove(mention);
                    break;
                }
            }

            foreach (DMElement DM in this.TweetMessages.Items)
            {
                if (DM.DM_Element.ID == ID)
                {
                    this.TweetMessages.Items.Remove(DM);
                    break;
                }
            }
        }

        public void deleteTweetById(string tweetElemetId)
        {
            cheese = new DelTweetDelegate(deleteTweet);
            maindispatcher.Invoke(cheese, new object[] { tweetElemetId });
        }


        
        void o3o_NewDM(TwitterDirectMessage DM, UserDatabase.User _usr)  // PLZ CHECK IF WORK
        {
            DMElement element;
            if (UsrDB.Users.Count > 1)
            {
                 element = new DMElement(this, DM, _usr, ImageCache.GetImage(DM.Sender.Id, DM.Sender.ProfileImageLocation), true);
            }
            else
            {
                 element = new DMElement(this, DM, _usr, ImageCache.GetImage(DM.Sender.Id, DM.Sender.ProfileImageLocation));
            }
            element.polyOpacity = polygonOpacity;
            this.TweetMessages.Items.Add(element);
            if (this.TweetMessages.Items.Count > o3o.Properties.Settings.Default.amountOfTWeetsToDisplay)
            {
                TweetElement el = (TweetElement)this.TweetMessages.Items[this.TweetMessages.Items.Count - 1];
                this.TweetMessages.Items.Remove(el);
                el.Dispose();
            }
        }


        public void FillHome(TwitterStatus status, UserDatabase.User _usr)
        {
            if (status.InReplyToScreenName == UsrDB.Users.Find(u => u.UserDetails.ScreenName == _usr.UserDetails.ScreenName).UserDetails.ScreenName)
            {
                FillMentions(status, _usr);
            }
            TweetElement element;
            if (UsrDB.Users.Count > 1)
            {
                 element = new TweetElement(this, status, _usr, ImageCache.GetImage(status.User.Id, status.User.ProfileImageLocation),true);
            }
            else
            {
                 element = new TweetElement(this, status, _usr, ImageCache.GetImage(status.User.Id, status.User.ProfileImageLocation));
            }
            element.polyOpacity = polygonOpacity;
              this.TweetElements.Items.Insert(0, element); 

            if (this.TweetElements.Items.Count > o3o.Properties.Settings.Default.amountOfTWeetsToDisplay)
            {
                TweetElement el = (TweetElement)this.TweetElements.Items[this.TweetElements.Items.Count - 1];
                this.TweetElements.Items.Remove(el);
                el.Dispose();
            }

        }

        public void FillMentions(TwitterStatus status, UserDatabase.User _usr)
        {
            TweetElement element;
            if (UsrDB.Users.Count > 1)
            {
                element = new TweetElement(this, status, _usr, ImageCache.GetImage(status.User.Id, status.User.ProfileImageLocation), true);
            }
            else
            {
                element = new TweetElement(this, status, _usr, ImageCache.GetImage(status.User.Id, status.User.ProfileImageLocation));
            }
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
            if (o3o.Properties.Settings.Default.ShowNotificationPopup)
            {
                notify notification = new notify(this);
                TweetElement element;
                if (UsrDB.Users.Count > 1)
                {
                    element = new TweetElement(this, status, _usr, ImageCache.GetImage(status.User.Id, status.User.ProfileImageLocation), true);
                }
                else
                {
                    element = new TweetElement(this, status, _usr, ImageCache.GetImage(status.User.Id, status.User.ProfileImageLocation));
                }
                element.polyOpacity = polygonOpacity;
                notification.content.Items.Add(element);
                playsound();
            }
        }

        public void reply(TwitterStatus Status)
        {
            inreply = true;
            replystatus = Status;

            textBox1.Text = "@" + Status.User.ScreenName + " ";
            if (textBox1.Visibility == Visibility.Collapsed)
            {
                TweetButton.Content = "Tweet";
                TweetElements.Margin = new Thickness(0, 0, 0, 70);
                TweetMentions.Margin = new Thickness(0, 0, 0, 70);
                TweetMessages.Margin = new Thickness(0, 0, 0, 70);
                textBox1.Visibility = Visibility.Visible;
                charleft.Visibility = Visibility.Visible;
                TweetLbl.Visibility = Visibility.Visible;

            }
            textBox1.Focus();

        }

        public void DMreply(TwitterDirectMessage Status)
        {
            Process.Start("https://twitter.com/messages");
        }

        public void favoriteTweet(decimal id, string user)
        {
            UsrDB.Users.Find(u => u.UserDetails.ScreenName == user).tweetStack.Twitter.favorite(id);
        }
        public void unfavoriteTweet(decimal id, string user)
        {
            UsrDB.Users.Find(u => u.UserDetails.ScreenName == user).tweetStack.Twitter.unfavorite(id);
        }
        public void retweet(decimal id, string user)
        {
            UsrDB.Users.Find(u => u.UserDetails.ScreenName == user).tweetStack.Twitter.Retweet(id);

        }

        public void Block(decimal userid, string user, string elementID)
        {
            if (UsrDB.Users.Find(u => u.UserDetails.ScreenName == user).tweetStack.Twitter.Block(userid))
            { 
                deleteTweetById(elementID);
            }
        }

        public void Report(decimal userid, string user, string elementID)
        {
            if (UsrDB.Users.Find(u => u.UserDetails.ScreenName == user).tweetStack.Twitter.Report(userid))
            {
                deleteTweetById(elementID);
            }
        }

        
        #endregion

        #region UI interactions

        void o3o_clearTweetStack(string reason)
        {
            bacon = new clearTweetStackDelegate(clearTweetStack);
            maindispatcher.Invoke(bacon, new object[] { reason });
        }

        public void clearTweetStack(string reason = "")
        {
            this.TweetElements.Items.Clear();
            this.TweetMentions.Items.Clear();
            this.TweetMessages.Items.Clear();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        private void TweetButton_Click(object sender, RoutedEventArgs e)
        {
            if (textBox1.Visibility == Visibility.Collapsed)
            {
                TweetButton.Content = "Cancel";
                TweetElements.Margin = new Thickness(0, 0, 0, 70);
                TweetMentions.Margin = new Thickness(0, 0, 0, 70);
                TweetMessages.Margin = new Thickness(0, 0, 0, 70);
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

        private void forceGC_Click(object sender, RoutedEventArgs e)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        private void closebutton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void minimisebutton_Click(object sender, RoutedEventArgs e)
        {
             WindowState = WindowState.Minimized;
        }

        private int CharactersLeft = 140;

        private void textBox1_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            

        }

        private void textBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            int chars = 140;
            string tempstring = textBox1.Text;
            chars -= System.Text.RegularExpressions.Regex.Matches(tempstring, "http").Count * 20;
            chars -= System.Text.RegularExpressions.Regex.Matches(tempstring, "www").Count * 20;

            System.Text.RegularExpressions.Regex urlRx = new System.Text.RegularExpressions.Regex(@"(?<url>(http:[/][/]|www.)([a-z]|[A-Z]|[0-9]|[/.]|[~])*)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            System.Text.RegularExpressions.MatchCollection matches = urlRx.Matches(tempstring);

            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                var url = match.Groups["url"].Value;
                tempstring = tempstring.Replace(url, "");
            }
            chars -= tempstring.Length;
            CharactersLeft = chars;

            charleft.Text = CharactersLeft.ToString();
            if (CharactersLeft < 0)
            {
                charleft.Foreground = new SolidColorBrush(Colors.Red);
            }
            else if (CharactersLeft > 140)
            {
                TweetButton.Content = "Cancel";
            }
            else
            {
                charleft.Foreground = new SolidColorBrush(Colors.Black);
                TweetButton.Content = "Send";
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

        
        public void tbox(string inc) 
        {
            textBox1.Text = inc;
            if (textBox1.Visibility == Visibility.Collapsed)
            {
                TweetButton.Content = "Tweet";
                TweetElements.Margin = new Thickness(0, 0, 0, 70);
                TweetMentions.Margin = new Thickness(0, 0, 0, 70);
                TweetMessages.Margin = new Thickness(0, 0, 0, 70);
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

        private void tweetsdisplayLabel_TextChanged(object sender, TextChangedEventArgs e)
        {
            string[] bla = tweetsdisplayLabel.Text.Split('.');
            tweetsdisplayLabel.Text = bla[0].ToString();

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

        private void TweetsDisplaySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Properties.Settings.Default.amountOfTWeetsToDisplay = Convert.ToInt32(TweetsDisplaySlider.Value);
            while (TweetElements.Items.Count > Properties.Settings.Default.amountOfTWeetsToDisplay)
            {
                TweetElements.Items.RemoveAt(TweetElements.Items.Count - 1);
            }
        }

        private void PopupCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            o3o.Properties.Settings.Default.ShowNotificationPopup = true;
        }

        private void SoundCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            o3o.Properties.Settings.Default.PlayNotificationSound = true;
        }

        private void PopupCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            o3o.Properties.Settings.Default.ShowNotificationPopup = false;
        }

        private void SoundCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            o3o.Properties.Settings.Default.PlayNotificationSound = false;
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

                this.Width = 771;
                this.MinWidth = 771;
                this.MaxWidth = 771;

                Grid men = mentionsgrid;

                mentionstabcontent.Children.Remove(men);
                tabControl1.Items.Remove(MentionsTab);

                MentionsTab.Content = null;
                MentionsTab = null;

                men.Width = 374;
                men.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;

                homegrid.Width = 374;
                homegrid.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;

                hometabcontent.Children.Add(men);
                HomeTab.Header = "Home & mentions";
            }
            else if (state == 3)
            {
                LayoutButton1.Checked += new RoutedEventHandler(LayoutButton1_Checked);
                LayoutButton2.Checked += new RoutedEventHandler(LayoutButton2_Checked);
                LayoutButton3.IsChecked = true;

                this.Width = 1145;
                this.MinWidth = 1145;
                this.MaxWidth = 1145;

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

                men.Width = 374;
                men.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;

                mesg.Width = 374;
                mesg.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;

                homegrid.Width = 374;
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

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (textBox1.Visibility == Visibility.Collapsed)
                {
                    TweetButton.Content = "Cancel";
                    TweetElements.Margin = new Thickness(0, 0, 0, 70);
                    TweetMentions.Margin = new Thickness(0, 0, 0, 70);
                    TweetMessages.Margin = new Thickness(0, 0, 0, 70);
                    textBox1.Visibility = Visibility.Visible;
                    charleft.Visibility = Visibility.Visible;
                    TweetLbl.Visibility = Visibility.Visible;
                    textBox1.Focus();
                }
                else
                {

                    SendTweet();

                }
            }

            if (e.Key == Key.Escape)
            {
                textBox1.Text = "";
                TweetButton.Content = "Tweet";
                TweetElements.Margin = new Thickness(0, 0, 0, 17);
                TweetMentions.Margin = new Thickness(0, 0, 0, 17);
                TweetMessages.Margin = new Thickness(0, 0, 0, 17);
                textBox1.Visibility = Visibility.Collapsed;
                charleft.Visibility = Visibility.Collapsed;
                TweetLbl.Visibility = Visibility.Collapsed;
                inreply = false;
                replystatus = null;
            }
        }

        private void ClearTweetstackButton_Click(object sender, RoutedEventArgs e)
        {
            clearTweetStack();
            GC.Collect();
        }

        private void ClearImageCacheButton_Click(object sender, RoutedEventArgs e)
        {
            ImageCache.ClearCache();
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
            ImageCache.SaveCache();
            al.DeleteSources(1, new int[1] { FSource });
            al.DeleteBuffers(1, new int[1] { FBuffer });
            FContext.Dispose();
            Point WindowPosition = new Point((int)this.Left, (int)this.Top);
            o3o.Properties.Settings.Default.LastWindowPosition = WindowPosition;
            o3o.Properties.Settings.Default.LastWindowHeight = this.Height;
            o3o.Properties.Settings.Default.Save();
            System.Windows.Application.Current.Shutdown();
            Process.GetCurrentProcess().Kill();
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
            if (o3o.Properties.Settings.Default.PlayNotificationSound)
            {
                al.SourcePlay(FSource);
            }
        }
        
        void loadsounds()
        {
            

            if (!Directory.Exists(System.IO.Path.Combine(AppData, "Sounds")))
            {
                Directory.CreateDirectory(System.IO.Path.Combine(AppData, "Sounds"));
            }

            string path = Directory.GetCurrentDirectory();
            string[] filenameswav = Directory.GetFiles(Directory.GetCurrentDirectory() + "\\Sounds", "*.wav");
            string[] filenamesmp3 = Directory.GetFiles(Directory.GetCurrentDirectory() + "\\Sounds", "*.mp3");
            string[] AppDatafilenameswav = Directory.GetFiles(AppData + "\\Sounds", "*.wav");
            string[] AppDatafilenamesmp3 = Directory.GetFiles(AppData + "\\Sounds", "*.mp3");
            for (int o = 0; o < filenamesmp3.Length; o++)
            {
                string Sname = filenamesmp3[o];

                SoundFile file = new SoundFile();
                file.soundname = System.IO.Path.GetFileNameWithoutExtension(Sname);
                ConvertMp3 converter = new ConvertMp3(Sname, AppData + @"\Sounds\" + file.soundname + ".wav", Sname);
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



            for (int e = 0; e < AppDatafilenamesmp3.Length; e++)
            {
                string Sname = filenamesmp3[e];

                SoundFile file = new SoundFile();
                file.soundname = System.IO.Path.GetFileNameWithoutExtension(Sname);
                ConvertMp3 converter = new ConvertMp3(Sname, AppData + @"\Sounds\" + file.soundname + ".wav", Sname);
                converter.ShowDialog();

                file.filepath = AppData + @"\Sounds\" + file.soundname + ".wav";
                file.extension = "wav";

                sounds.Add(file);
            }

            for (int n = 0; n < AppDatafilenameswav.Length; n++)
            {
                string Sname = filenameswav[n];
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
                    File.Copy(dialog.FileName, AppData + "\\Sounds\\" + System.IO.Path.GetFileName(dialog.FileName), true);
                    file.extension = "wav";
                }
                else if (System.IO.Path.GetExtension(dialog.FileName) == ".mp3")
                {
                    ConvertMp3 converter = new ConvertMp3(dialog.FileName, AppData + "\\Sounds\\" + file.soundname + ".wav");
                    converter.ShowDialog();
                    file.extension = "mp3";
                }
                file.filepath = AppData + "\\Sounds\\" + System.IO.Path.GetFileName(dialog.FileName);
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

            

         

           


    
            
    }

}
