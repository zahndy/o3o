using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Interop;

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
