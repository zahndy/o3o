using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace o3o
{
    public struct AeroMargin
    {
        public int Left, Right, Top, Bottom;
        public AeroMargin(int Left, int Right, int Top, int Bottom)
        {
            this.Left = Left;
            this.Right = Right;
            this.Top = Top;
            this.Bottom = Bottom;
        }
    }

    public static class AeroGlassHelper
    {
        [DllImport("dwmapi.dll", PreserveSig = false)]
        static extern void DwmExtendFrameIntoClientArea(IntPtr hwnd, ref AeroMargin margins);
        [DllImport("dwmapi.dll", PreserveSig = false)]
        static extern bool DwmIsCompositionEnabled();

        public static bool CheckDwm(this Window Window)
        {
            return DwmIsCompositionEnabled();
        }

        public static string GetColor()
        {
            int argbColor = (int)Microsoft.Win32.Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\DWM", "ColorizationColor", null);
            var color = System.Drawing.Color.FromArgb(argbColor);
            return ConverterToHex(color);
        }

        private static String ConverterToHex(System.Drawing.Color c)
        {
            return String.Format("#{0}{1}{2}", c.R.ToString("X2"), c.G.ToString("X2"), c.B.ToString("X2"));
        }

        public static bool SetAeroGlass(this Window Window)
        {
            return SetAeroGlass(Window, new AeroMargin(-1, -1, -1, -1));
        }
        public static bool SetAeroGlass(this Window Window, AeroMargin Margin)
        {
            if (!DwmIsCompositionEnabled()) return false;
            IntPtr hwnd = new WindowInteropHelper(Window).Handle;
            Window.Background = Brushes.Transparent;
            HwndSource.FromHwnd(hwnd).CompositionTarget.BackgroundColor = Colors.Transparent;
            DwmExtendFrameIntoClientArea(hwnd, ref Margin);
            return true;
        }
        

    }
}
