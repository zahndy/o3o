using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
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
using System.Windows.Interop;
using System.Windows.Media.Effects;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using Twitterizer;
using OpenAL;
using Unf;

namespace o3o
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {

        dynamic Mainwindow;

        public App()
        {
             if (o3o.Properties.Settings.Default.Layout == 1)
             {
                 Mainwindow = new MainWindow1();
             }
             else if (o3o.Properties.Settings.Default.Layout == 2)
             {
                 Mainwindow = new MainWindow2();
             }
             else if (o3o.Properties.Settings.Default.Layout == 3)
             {
                 Mainwindow = new MainWindow3();
             }
             App.Current.MainWindow = Mainwindow;
             Mainwindow.Loaded += new RoutedEventHandler(Window_Loaded);
             Mainwindow.Show();
        }

        public static UserDatabase UsrDB = new UserDatabase();
        System.Windows.Threading.Dispatcher maindispatcher;
        public delegate void dostuff(TwitterStatus status, UserDatabase.User _usr);
        public dostuff dostuffdel;
        public Screen[] Displays = System.Windows.Forms.Screen.AllScreens;
        public float polygonOpacity = o3o.Properties.Settings.Default.PolygonOpacity;
        
        public bool isloaded = false;

        public struct SoundFile
        {
            public string soundname, filepath, extension;
        }
        public SoundFile CurrentSelectedSound;
        public int Volume = o3o.Properties.Settings.Default.Volume;
        public List<SoundFile> sounds = new List<SoundFile>();

        public int FBuffer;
        public int FSource;
        public ContextAL FContext;

        void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Mainwindow.setglass();

             Mainwindow.Left = o3o.Properties.Settings.Default.LastWindowPosition.X;
             Mainwindow.Top = o3o.Properties.Settings.Default.LastWindowPosition.Y;

             Mainwindow.SettingsAmountOfTweetsTextb.Text = o3o.Properties.Settings.Default.amountOfTWeetsToDisplay.ToString();
            loadsounds();

            if (o3o.Properties.Settings.Default.use_system_color == true)
            {
                Mainwindow.checkBox1.IsChecked = true;
            }

            if (UsrDB.load() == false || UsrDB.Users.Count == 0)
                UsrDB.CreateUser();
            Mainwindow.UserSelectionMenuCurrentName.Header = UsrDB.Users[0].UserDetails.ScreenName;

            Mainwindow.DisplaysComboBox.SelectedIndex = o3o.Properties.Settings.Default.DisplayIndex;
            int dex;
            int upperBound = Displays.GetUpperBound(0);
            for (dex = 0; dex <= upperBound; dex++)
            {
                if (Displays[dex].Primary)
                {
                    Mainwindow.DisplaysComboBox.Items.Add(dex + ": " + Displays[dex].DeviceName + " Width:" + Displays[dex].Bounds.Width.ToString() + " Height:" + Displays[dex].Bounds.Height.ToString() + " (primary)");
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
                    Mainwindow.DisplaysComboBox.Items.Add(entry);
                }

            }
                maindispatcher = this.Dispatcher;
                foreach (UserDatabase.User usr in UsrDB.Users)  
                {
                    usr.tweetStack.NewTweet += new TweetStack.newtweetDel(o3o_NewTweet);
                    usr.tweetStack.DMReceived += new TweetStack.DMReceivedDel(o3o_NewDM);
                    usr.tweetStack.TweetDeleted +=new TweetStack.TweetDeletedDel(o3o_TweetDeleted);
                }

                UpdateUserMenu(UsrDB);

            
        }

        public void UpdateUserMenu(UserDatabase usrDB)
        {
            Mainwindow.UserSelectionMenu.Items.Clear();
            foreach (UserDatabase.User usr in usrDB.Users)
            {
                System.Windows.Controls.MenuItem newMenuItem1 = new System.Windows.Controls.MenuItem(); // here you add more users to the menu, also the events when the user selects something 
                newMenuItem1.Header = usr.UserDetails.ScreenName;
                newMenuItem1.Click += new RoutedEventHandler(newMenuItem1_Click);
                Mainwindow.UserSelectionMenu.Items.Add(newMenuItem1);
            }

        }
        void newMenuItem1_Click(object sender, RoutedEventArgs e)
        {
            Mainwindow.UserSelectionMenuCurrentName.Header = ((System.Windows.Controls.MenuItem)sender).Header; //string of current user
        }
        #region tweet helpers
        

       public void SendTweet()
        {
            if (Mainwindow.textBox1.Text.Length <= 140)
            {
                if (!String.IsNullOrEmpty(Mainwindow.textBox1.Text))
                {
                    if (replystatus != null && !String.IsNullOrEmpty(replystatus.StringId))
                    {
                        if (inreply && Mainwindow.textBox1.Text.StartsWith("@" + replystatus.User.ScreenName))
                        {
                            UsrDB.Users.Find(u => u.UserDetails.ScreenName == Mainwindow.UserSelectionMenuCurrentName.Header).tweetStack.Twitter.Reply(replystatus.Id, Mainwindow.textBox1.Text);
                            Mainwindow.textBox1.Text = "";
                            Mainwindow.charleft.Text = "140";

                            Mainwindow.testbutton.Content = "Tweet";
                            Mainwindow.TweetElements.Margin = new Thickness(0, 0, 0, 17);
                            Mainwindow.textBox1.Visibility = Visibility.Collapsed;
                            Mainwindow.charleft.Visibility = Visibility.Collapsed;
                            Mainwindow.TweetLbl.Visibility = Visibility.Collapsed;
                            inreply = false;
                            replystatus = null;
                        }
                    }
                    else
                    {
                        UsrDB.Users.Find(u => u.UserDetails.ScreenName == Mainwindow.UserSelectionMenuCurrentName.Header).tweetStack.Twitter.SendTweet(Mainwindow.textBox1.Text);
                        Mainwindow.textBox1.Text = "";
                        Mainwindow.charleft.Text = "140";

                        Mainwindow.testbutton.Content = "Tweet";
                        Mainwindow.TweetElements.Margin = new Thickness(0, 0, 0, 17);
                        Mainwindow.textBox1.Visibility = Visibility.Collapsed;
                        Mainwindow.charleft.Visibility = Visibility.Collapsed;
                        Mainwindow.TweetLbl.Visibility = Visibility.Collapsed;
                        inreply = false;
                        replystatus = null;
                    }
                }
                else
                {
                    Mainwindow.testbutton.Content = "Tweet";
                    Mainwindow.TweetElements.Margin = new Thickness(0, 0, 0, 17);
                    Mainwindow.textBox1.Visibility = Visibility.Collapsed;
                    Mainwindow.charleft.Visibility = Visibility.Collapsed;
                    Mainwindow.TweetLbl.Visibility = Visibility.Collapsed;
                    Mainwindow.charleft.Foreground = new SolidColorBrush(Colors.Red);
                    Mainwindow.charleft.Text = "no text";
                    inreply = false;
                    replystatus = null;
                }
            }
            else
            {
                Mainwindow.charleft.Foreground = new SolidColorBrush(Colors.Red);
                Mainwindow.charleft.Text = "too long";
            }

        }

       

        void o3o_NewTweet(TwitterStatus status, UserDatabase.User _usr)
        {
            dostuffdel = new dostuff(FillHome);
            maindispatcher.Invoke(dostuffdel, new object[] { status, _usr});

            dostuffdel = new dostuff(Notification);
            maindispatcher.Invoke(dostuffdel, new object[] { status,_usr });
           
        }

        void o3o_TweetDeleted(Twitterizer.Streaming.TwitterStreamDeletedEvent deletedreason)
        {

            //The calling thread cannot access this object because a different thread owns it.

            //foreach (TweetElement tweet in Mainwindow.TweetElements.Items)
            //{
            //    if (tweet.tweetElement.ID == deletedreason.Id.ToString())
            //    {
            //        Mainwindow.TweetElements.Items.Remove(tweet);
            //    }
            //}

        }

        void o3o_NewDM(TwitterDirectMessage DM, UserDatabase.User _usr)  // PLZ CHECK IF WORK
        {
            DMElement element = new DMElement(Mainwindow, DM, _usr); 
            element.polyOpacity = polygonOpacity;
            Mainwindow.TweetMessages.Items.Add(element);
            if (Mainwindow.TweetMessages.Items.Count > o3o.Properties.Settings.Default.amountOfTWeetsToDisplay)
            {
                Mainwindow.TweetMessages.Items.RemoveAt(Mainwindow.TweetElements.Items.Count);
            }
        }

        public void FillHome(TwitterStatus status, UserDatabase.User _usr) 
        {
            if (status.InReplyToScreenName == UsrDB.Users.Find(u => u.UserDetails.ScreenName == _usr.UserDetails.ScreenName).UserDetails.ScreenName)
            {
                FillMentions(status, _usr) ;
            }
            TweetElement element = new TweetElement(Mainwindow, status, _usr);
            element.polyOpacity = polygonOpacity;
            Mainwindow.TweetElements.Items.Insert(0, element);
            if (Mainwindow.TweetElements.Items.Count > o3o.Properties.Settings.Default.amountOfTWeetsToDisplay)
            {
               // Mainwindow.TweetElements.Items[Mainwindow.TweetElements.Items.Count - 1].Dispose();               
                //Mainwindow.TweetElements.Items.RemoveAt(Mainwindow.TweetElements.Items.Count-1);
                TweetElement el = Mainwindow.TweetElements.Items[Mainwindow.TweetElements.Items.Count - 1];
                Mainwindow.TweetElements.Items.Remove(el);
                el.Dispose();
            }
            
        }

        public void FillMentions(TwitterStatus status, UserDatabase.User _usr) 
        {
            TweetElement element = new TweetElement(Mainwindow, status, _usr);
            element.polyOpacity = polygonOpacity;
            Mainwindow.TweetMentions.Items.Insert(0, element);
            if (Mainwindow.TweetMentions.Items.Count > o3o.Properties.Settings.Default.amountOfTWeetsToDisplay)
            {
                //Mainwindow.TweetMentions.Items.RemoveAt(Mainwindow.TweetElements.Items.Count-1);
                TweetElement el = Mainwindow.TweetMentions.Items[Mainwindow.TweetElements.Items.Count - 1];
                Mainwindow.TweetMentions.Items.Remove(el);
                el.Dispose();
            }
        }

        public void Notification(TwitterStatus status, UserDatabase.User _usr)
        {
            notify notification = new notify();
            TweetElement element = new TweetElement(Mainwindow, status, _usr);
           
            element.polyOpacity = polygonOpacity;
            element.replyBtn.Source = new BitmapImage(new Uri("/o3o;component/Images/reply.png", UriKind.Relative));
            notification.content.Items.Add(element);
            playsound();
        }
        #endregion

        #region sound
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
                Mainwindow.soundselection.Items.Add(SoundMenuItem);
            }

            CurrentSelectedSound = sounds[0]; // Bleep.mp3 will be the default sound, unless the user picked a different one previously
            Mainwindow.soundselection.SelectedIndex = 0;


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
                Mainwindow.soundselection.Items.Add(SoundMenuItem);
            }

        }
        #endregion

        #region misc

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

            Point WindowPosition = new Point((int)Mainwindow.Left, (int)Mainwindow.Top);
            o3o.Properties.Settings.Default.LastWindowPosition = WindowPosition;
            o3o.Properties.Settings.Default.Save();
        }

        public void setLayOut(int index)
        {
            o3o.Properties.Settings.Default.Layout = index;
            o3o.Properties.Settings.Default.Save();
            System.Diagnostics.Process.Start(System.Windows.Application.ResourceAssembly.Location);
            System.Windows.Application.Current.Shutdown();
        }
       
        #endregion

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

        public bool inreply = false;
        public TwitterStatus replystatus;
    }
}
