using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DesktopServer;
using ManagedWinapi.Windows;
using static ManagedWinapi.Windows.SystemWindow;
using System.Drawing;
using System.Runtime.InteropServices;

namespace DesktopServer
{
    class Program
    {

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        static extern bool DeleteObject(IntPtr hObject);


        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, int nFlags);

        static Image Capture(SystemWindow w)
        {
            Bitmap bmp = new Bitmap(w.Position.Width, w.Position.Height);
            Graphics g = Graphics.FromImage(bmp);
            IntPtr hdc = g.GetHdc();

            bool success = PrintWindow(w.HWnd, hdc, 0);
            g.ReleaseHdc(hdc);
            g.Dispose();

            if (!success)
            {
                Console.WriteLine("Error copying image");
            }

            /*
            IntPtr pSource = CreateCompatibleDC(pTarget);
            IntPtr pOrig = SelectObject(pSource, bmp.GetHbitmap());
            w.PrintWindow(HWnd, pTarget, 0);
            IntPtr pNew = SelectObject(pSource, pOrig);
            DeleteObject(pOrig);
            DeleteObject(pNew);
            DeleteObject(pSource);
            g.ReleaseHdc(pTarget);
            g.Dispose();
            */
            return bmp;
        }

        static void Main(string[] args)
        {
            var last = DateTime.Now;
            foreach (var window in SystemWindow.AllToplevelWindows)
            {
                using (var image = Capture(window))
                    image.Save(window.Title + ".png");
            }

            while (!Console.KeyAvailable)
            {
                

                var now = DateTime.Now;
                //Console.WriteLine(1/(now - last).TotalSeconds);
                last = now;
            }
            Console.ReadLine();
        }
    }
}
