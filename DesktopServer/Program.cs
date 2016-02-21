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
using System.Net;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace DesktopServer
{
    class Program
    {
        const int Port = 7117;

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        static extern bool DeleteObject(IntPtr hObject);


        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, int nFlags);

        static Image Capture(SystemWindow w)
        {
            Bitmap bmp;
            if (w.Position.Width != 0 && w.Position.Height != 0)
            {

                bmp = new Bitmap(w.Position.Width, w.Position.Height);
            }
            else {
                return bmp = new Bitmap(2, 2);
            }
            Graphics g = Graphics.FromImage(bmp);
            IntPtr hdc = g.GetHdc();

            bool success = PrintWindow(w.HWnd, hdc, 0);

            g.ReleaseHdc(hdc);

            //Draw the cursor, for the win.

            var mousePos = System.Windows.Forms.Control.MousePosition;
            mousePos.X -= w.Rectangle.Left;
            mousePos.Y -= w.Rectangle.Top;
            
            

            //g.FillRectangle(Brushes.Magenta, new Rectangle(mousePos, Cursor.Current.Size));

            Cursor.Current.Draw(g, new Rectangle(mousePos, Cursor.Current.Size));

            //g.DrawString(System.Windows.Forms.Control.MousePosition.ToString(), new Font(FontFamily.GenericMonospace, 32), Brushes.Magenta, 0, 0);

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

        static void Status()
        {
            foreach (var window in AllToplevelWindows)
            {
            }
        }

        static void Main(string[] args)
        {
            using (HttpListener listener = new HttpListener())
            {
                listener.Prefixes.Add($"http://*:{Port}/");
                listener.Start();
                while (listener.IsListening)
                {
                    var ctx = listener.GetContext();
                    //var url = ctx.Request.QueryString["url"];
                    //if (url != null)
                    //    browser.Load(url);

                    //Split into it's own command.
                    //ForegroundWindow.Size = new Size(512, 512);

                    ctx.Response.ContentType = "image/png";
                    ctx.Response.StatusCode = 200;
                    using (var pic = Capture(ForegroundWindow))
                    {
                        pic.Save(ctx.Response.OutputStream, ImageFormat.Png);
                    }



                    //var ss = browser.ScreenshotOrNull();
                    //ss?.Save(ctx.Response.OutputStream, ImageFormat.Png);
                    ctx.Response.OutputStream.Close();
                }
            }

        }
    }
}
