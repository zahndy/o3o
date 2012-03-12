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
using System.Web.UI.WebControls;

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
            MouseDown += delegate { DragMove(); };
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
        ObservableCollection<TweetElement> items = new ObservableCollection<TweetElement>();
        private void testbutton_Click(object sender, RoutedEventArgs e)
        {
            //Notification("test");
            TweetElement element = new TweetElement();
           element.Tweet = "test";
            
            items.Add(element );
            TweetElements.ItemsSource = items;
            
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

        }

    }

    


}
