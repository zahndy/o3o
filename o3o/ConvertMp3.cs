using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;
using System.IO;

namespace o3o
{
    public partial class ConvertMp3 : Form
    {
        [DllImport("dwmapi.dll")]
        public static extern int DwmExtendFrameIntoClientArea(
           IntPtr hWnd,
           ref MARGINS pMarInset
        );

        [StructLayout(LayoutKind.Sequential)]
        public struct MARGINS
        {
            public int cxLeftWidth;
            public int cxRightWidth;
            public int cyTopHeight;
            public int cyBottomHeight;
        }

        string SourcePath;
        string TargetPath;
        string FileToDelte;
        public ConvertMp3(string sourcepath, string targetpath, string filetodelete = null)
        {
            InitializeComponent();
            SourcePath = sourcepath;
            TargetPath = targetpath;
            FileToDelte = filetodelete;
            this.TopMost = true;
        }

        private void ConvertMp3_Load(object sender, EventArgs e)
        {
            MARGINS margins = new MARGINS();
            margins.cxLeftWidth = 0;
            margins.cxRightWidth = 0;
            margins.cyTopHeight = 45;
            margins.cyBottomHeight = 0;

            IntPtr hWnd = this.Handle;
            int result = DwmExtendFrameIntoClientArea(hWnd, ref margins);
           
            Process bacon = new Process();
            ProcessStartInfo p = new ProcessStartInfo();
            string sArgs = String.Format(" -i {0} {1}", SourcePath, TargetPath);
            p.FileName = Directory.GetCurrentDirectory()+@"\ffmpeg.exe";
            p.CreateNoWindow = true;
            p.RedirectStandardOutput = true;
            p.UseShellExecute = false;

            bacon.Exited += new EventHandler(myProcess_Exited);

            p.Arguments = sArgs;
            bacon.StartInfo = p;
            bacon.Start();
            bacon.WaitForExit();
            bacon.StandardOutput.ReadToEnd();
            if (FileToDelte != null)
            {
                try { File.Delete(FileToDelte); }
                catch(Exception) { }
            }
            this.Close();
        }
        private void myProcess_Exited(object sender, System.EventArgs e)
        {
            if (FileToDelte != null)
            {
                try { File.Delete(FileToDelte); }
                catch (Exception) { }
            }
            this.Close();
        }
    }
}
