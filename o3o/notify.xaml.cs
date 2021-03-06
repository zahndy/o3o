﻿using System;
using System.Windows;

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
        MainWindow1 parent;
        bool BringToFrontOnce = true;
        
        public notify(MainWindow1 parentWindow)
        {
            parent = parentWindow;
            InitializeComponent();
            this.Left = ((parent.Displays[Properties.Settings.Default.DisplayIndex].Bounds.Location.X + parent.Displays[Properties.Settings.Default.DisplayIndex].Bounds.Width) - this.Width) - 107;
            this.Top =  parent.Displays[Properties.Settings.Default.DisplayIndex].Bounds.Location.Y-this.Height;

            ypos = parent.Displays[Properties.Settings.Default.DisplayIndex].Bounds.Location.Y;
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
                 wait--;
                 if (this.Top <= (ypos - this.Height) && wait <= 0)
                 {
                     this.Close();
                 }
             }
             else if (this.Top < ypos && wait > 0)  //going down
             {
                 this.Top += 5;
             }
             if (wait < -100)
             {
                 this.Close();
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

         private void Window_Activated(object sender, EventArgs e)
         {
             if (BringToFrontOnce)
             {
                 parent.Activate();
                 parent.Topmost = true;
                 parent.Topmost = false;
                 parent.Focus();
                 BringToFrontOnce = false;
             }
         }

    }
}
