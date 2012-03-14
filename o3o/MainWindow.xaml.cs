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
using System.Collections.ObjectModel;

namespace o3o
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region noicon

        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter, int x, int y, int width, int height, uint flags);

        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam);

        const int GWL_EXSTYLE = -20;
        const int WS_EX_DLGMODALFRAME = 0x0001;
        const int SWP_NOSIZE = 0x0001;
        const int SWP_NOMOVE = 0x0002;
        const int SWP_NOZORDER = 0x0004;
        const int SWP_FRAMECHANGED = 0x0020;
        const uint WM_SETICON = 0x0080;

        #endregion

        public MainWindow()
        {
            InitializeComponent();
            MouseDown += delegate { if (MouseButtonState.Pressed == Mouse.LeftButton) { DragMove(); } };
            this.Loaded += new RoutedEventHandler(Window1_Loaded);  

        }
        
        #region removeicon

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            int extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_DLGMODALFRAME);
            SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
        }

        #endregion


        void Window1_Loaded(object sender, RoutedEventArgs e)
        {
            this.SetAeroGlass();
            
        }

        int i;
        private void testbutton_Click(object sender, RoutedEventArgs e)
        {
            i++;

            Tweet(i.ToString(), "zanderroxley", "somedate");
            
        }

        public void Tweet(string message, string user, string date) // image is fetched in Tweetelement.xaml.cs
        {
            TweetElement element = new TweetElement();
            element.Tweet = message;
            element.name = user;
            element.Date = date;
            TweetElements.Items.Insert(0, element);
        }

        public void Notification(string message)
        {
            notify notification = new notify();
            notification.Text = message;
            
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
                charleft.Foreground = new SolidColorBrush(Colors.Red); 
            else
                charleft.Foreground = new SolidColorBrush(Colors.Black); 
        }

        #region scrollviewgrab


        private Point myMousePlacementPoint;

        private void OnListViewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                myMousePlacementPoint = this.PointToScreen(Mouse.GetPosition(this));
            }
        }

        private void OnListViewMouseMove(object sender, MouseEventArgs e)
        {
            ScrollViewer scrollViewer = GetScrollViewer(TweetElements) as ScrollViewer;

            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                var currentPoint = this.PointToScreen(Mouse.GetPosition(this));

                if (currentPoint.Y < myMousePlacementPoint.Y)
                {
                    scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - 3);
                }
                else if (currentPoint.Y > myMousePlacementPoint.Y)
                {
                    scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + 3);
                }

                if (currentPoint.X < myMousePlacementPoint.X)
                {
                    scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - 3);
                }
                else if (currentPoint.X > myMousePlacementPoint.X)
                {
                    scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + 3);
                }
            }
        }

        public static DependencyObject GetScrollViewer(DependencyObject o)
        {
            // Return the DependencyObject if it is a ScrollViewer
            if (o is ScrollViewer)
            { return o; }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(o); i++)
            {
                var child = VisualTreeHelper.GetChild(o, i);

                var result = GetScrollViewer(child);
                if (result == null)
                {
                    continue;
                }
                else
                {
                    return result;
                }
            }
            return null;
        }


        #endregion

    }

    


}
