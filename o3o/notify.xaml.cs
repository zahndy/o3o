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
using System.Windows.Shapes;
using System.Windows.Forms;
namespace o3o
{
    /// <summary>
    /// Interaction logic for notify.xaml
    /// </summary>
    /// 
    public partial class notify : Window
    {

        static System.Windows.Forms.Timer Timer = new System.Windows.Forms.Timer();
        float wait = 300;
        int ypos;
        
        public notify()
        {
            InitializeComponent();
            this.Left = ((((App)System.Windows.Application.Current).Displays[Properties.Settings.Default.DisplayIndex].Bounds.Location.X + ((App)System.Windows.Application.Current).Displays[Properties.Settings.Default.DisplayIndex].Bounds.Width) - this.Width) - 105;
            this.Top =  ((App)System.Windows.Application.Current).Displays[Properties.Settings.Default.DisplayIndex].Bounds.Location.Y-this.Height;
            ypos = ((App)System.Windows.Application.Current).Displays[Properties.Settings.Default.DisplayIndex].Bounds.Location.Y;
            this.Show();
            this.SetAeroGlass();
            this.Topmost = Properties.Settings.Default.TopMostNotify;
            Timer.Tick += new EventHandler(timer_Tick);
            Timer.Interval = (1);
            Timer.Start();
        }


         private void timer_Tick(Object myObject, EventArgs myEventArgs)
         {
             if (this.Top >= (ypos-5) && wait > 0) // waiting
             {
                 wait--;
             }
             else if ((this.Top >= (ypos - this.Height)) && wait <= 0 && !mousehover()) // going up
             {
                 this.Top -= 5;
                 if (this.Top == (ypos - this.Height) && wait <= 0)
                 {
                     this.Close();
                 }
             }
             else if ( this.Top <0 && wait > 0)  //going down
             {
                 this.Top += 5;
             }
         }

         bool mousehover()
         {
              System.Drawing.Point mousePt = System.Windows.Forms.Cursor.Position;
              if (mousePt.X > this.Left &&
                  mousePt.X < this.Left + this.Width &&
                  mousePt.Y > this.Top &&
                  mousePt.Y < this.Top + this.Height)
              {
                  return true;
              }
              else
                  return false;
         }

         private void button1_Click(object sender, RoutedEventArgs e)
         {
             this.Close();
         }
    }
}
