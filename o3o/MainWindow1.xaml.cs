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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow1 : Window
    {
        #region loading stuff
       

        public MainWindow1()
        {
            InitializeComponent();
            MouseDown += delegate { if (MouseButtonState.Pressed == System.Windows.Input.Mouse.LeftButton) { DragMove(); } };
            
        }
        public void setglass()
        {
            this.SetAeroGlass();
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
                ((App)System.Windows.Application.Current).SendTweet();
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
                ((App)System.Windows.Application.Current).SendTweet();
            }
            if (e.Key == Key.Escape)
            {
                textBox1.Text = "";
                testbutton.Content = "Tweet";
                TweetElements.Margin = new Thickness(0, 0, 0, 17);
                textBox1.Visibility = Visibility.Collapsed;
                charleft.Visibility = Visibility.Collapsed;
                TweetLbl.Visibility = Visibility.Collapsed;
                ((App)System.Windows.Application.Current).inreply = false;
                ((App)System.Windows.Application.Current).replystatus = null;
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
            ((App)System.Windows.Application.Current).clearuserdata();
        }

        private void OpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ((App)System.Windows.Application.Current).polygonOpacity = (float)OpacitySlider.Value;
            Properties.Settings.Default.PolygonOpacity = ((App)System.Windows.Application.Current).polygonOpacity;
            foreach (TweetElement tweet in TweetElements.Items)
            {
                tweet.PolyOpacity = ((App)System.Windows.Application.Current).polygonOpacity;
            }

            foreach (TweetElement tweet in TweetMentions.Items)
            {
                tweet.PolyOpacity = ((App)System.Windows.Application.Current).polygonOpacity;
            }
        }

        private void volumeLabel_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (volumeLabel.Text.Length > 1)
                volumeLabel.Text = volumeLabel.Text.Substring(0, 2);
            else if (volumeLabel.Text.Length > 0)
                volumeLabel.Text = volumeLabel.Text.Substring(0, 1);
        }



        private void SettingsAmountOfTweetsTextb_TextChanged(object sender, TextChangedEventArgs e)
        {
            Properties.Settings.Default.amountOfTWeetsToDisplay = Convert.ToInt32(SettingsAmountOfTweetsTextb.Text);
            while (TweetElements.Items.Count > Properties.Settings.Default.amountOfTWeetsToDisplay)
            {
                TweetElements.Items.RemoveAt(TweetElements.Items.Count - 1);
            }
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
            UserAccounts AccountsWindow = new UserAccounts();
            AccountsWindow.ShowDialog();
            //grid1.Children.Clear();
        }
        #endregion

        #region misc stuff
        
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
        private void Window_Closed(object sender, EventArgs e)
        {
            ((App)System.Windows.Application.Current).Window_Closed(sender, e);
        }

        #endregion

        #region sound stuff

            private void soundselection_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                ((App)System.Windows.Application.Current).changesound(soundselection.SelectedIndex);
                        
            }

            private void playbutton_Click(object sender, RoutedEventArgs e)
            {
                ((App)System.Windows.Application.Current).playsound();
            }

            private void button1_Click(object sender, RoutedEventArgs e)
            {
                ((App)System.Windows.Application.Current).opensoundfile();
            }

            private void slider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
            {
                ((App)System.Windows.Application.Current).Volume = Convert.ToInt32(Math.Round(slider1.Value));
                Properties.Settings.Default.Volume = ((App)System.Windows.Application.Current).Volume;
                float newvol = ((App)System.Windows.Application.Current).Volume / 100f;
                al.Sourcef(((App)System.Windows.Application.Current).FSource, al.GAIN, newvol);
            }
        #endregion



            private void LayoutButton2_Checked(object sender, RoutedEventArgs e)
            {
                ((App)System.Windows.Application.Current).setLayOut(2);
            }

            private void LayoutButton3_Checked(object sender, RoutedEventArgs e)
            {
                ((App)System.Windows.Application.Current).setLayOut(3);
                
            }

            public void favoriteTweet(decimal id, string user)
            {
                ((App)System.Windows.Application.Current).favoriteTweet(id, user);
            }
            public void unfavoriteTweet(decimal id, string user)
            {
                ((App)System.Windows.Application.Current).favoriteTweet(id, user);
            }

            public void retweet(decimal id, string user)
            {
                ((App)System.Windows.Application.Current).retweet(id, user);
            }

            public void reply( TwitterStatus Status)
            {
                ((App)System.Windows.Application.Current).inreply = true;
                ((App)System.Windows.Application.Current).replystatus = Status;

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

            
    }

}
