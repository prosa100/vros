using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using CefSharp.Internals;
using CefSharp.OffScreen;
using System.Drawing;
using static System.Drawing.Color;
using System.Runtime.CompilerServices;
using CefSharp;
using System.Net;
using System.Drawing.Imaging;
using static CefSharpServer.Debug;
namespace CefSharpServer
{
    public class Program 
    {
        const int Port = 6661;
        const int Size = 1024;
        static ChromiumWebBrowser browser;
        static HttpListener listener;

        private static void OnQuit(object sender, EventArgs e)
        {
            listener.Stop();
            Cef.Shutdown();
        }

        [STAThread]

        public static void Main()
        {
            if (!Cef.IsInitialized)
                Cef.Initialize(new CefSettings()
                {
                    UserAgent = "Mozilla/5.0 (Windows; U; Android 5.0.3; en-us; vrshare) AppleWebkit/537.36 (KHTML, like Gecko) Chrome/47.0.2526.106 Mobile Safari/537.36"
                }, shutdownOnProcessExit: true, performDependencyCheck: true);
            else
                Log("Why?");

            AppDomain.CurrentDomain.ProcessExit += OnQuit;
            browser = new ChromiumWebBrowser();
            browser.Size = new Size(Size, Size);
           
            while (!browser.IsBrowserInitialized)
                Thread.Sleep(0);
            //browser.Load("en.m.wikipedia.org");

            browser.Load("http://www.timeanddate.com/worldclock/fullscreen.html?n=3704");
            //browser.Load("https://en.m.wikipedia.org/wiki/Virtual_reality");

            //browser.Load("https://m.reddit.com");
            //browser.Load(@"http://learningwebgl.com/lessons/lesson05/index.html");

            
            browser.NewScreenshot += Browser_NewScreenshot;
            
            Log("Hello");
            using (listener = new HttpListener())
            {
                listener.Prefixes.Add($"http://localhost:{Port}/");
                listener.Start();
                while (listener.IsListening)
                {
                    /**
                     * Commands:
                     *    http:localhost:8084/goto?url=[dest]
                     *         -- this loads the page specified by url.
                     *    http:localhost:8084/click?pos=[x] [y]
                     *         -- this clicks on coordinate (x, y)
                     *    http:localhost:8084/scroll?dir=[direction]
                     *         -- scrolls 50 px up, down, left or right.
                     */
                    var ctx  = listener.GetContext();
                    

                    var rqst = ctx.Request.RawUrl;
                    int indx = rqst.IndexOf("?")  - 1;
                    string cmd;
                    if (indx > 0)
                    {
                        cmd = rqst.Substring(1, indx);

                        switch (cmd)
                        {
                            case ("goto"):
                                var url = ctx.Request.QueryString["url"];
                                if (url != null)
                                    browser.Load(url);
                                break;
                            case ("click"):
                                var clk_s = ctx.Request.QueryString["pos"];
                                int clk_x, clk_y;

                                try
                                {
                                    if (int.TryParse(clk_s.Substring(0, clk_s.IndexOf(" ")), out clk_x) &&
                                        int.TryParse(clk_s.Substring(clk_s.IndexOf(" ")), out clk_y))
                                    {
                                        browser.ExecuteScriptAsync("document.elementFromPoint(" + clk_x + ", " + clk_y + ").click()");
                                    }
                                }
                                catch
                                {
                                    /** Malformed Requests will be ignored. **/
                                }
                                break;
                            case ("scroll"):
                                var dir = ctx.Request.QueryString["dir"];
                                switch (dir)
                                {
                                    case ("up"):
                                        browser.ExecuteScriptAsync("scrollBy(0,-50)");
                                        break;
                                    case ("down"):
                                        browser.ExecuteScriptAsync("scrollBy(0, 50)");
                                        break;
                                    case ("left"):
                                        browser.ExecuteScriptAsync("scrollBy(-50,0)");
                                        break;
                                    case ("right"):
                                        browser.ExecuteScriptAsync("scrollBy(0, 50)");
                                        break;
                                }
                                break;
                        }
                    }

                    ctx.Response.ContentType = "image/png";
                    ctx.Response.StatusCode = 200;
                    var bufferT = buffer;
                    buffer.Position = 0;
                    buffer.CopyTo(ctx.Response.OutputStream);
                    ctx.Response.OutputStream.Close();
                }
            }
        }

        static bool swap;

        static MemoryStream buffer { get { return swap ? buffera : bufferb; } }
        static MemoryStream backbuffer { get { return (!swap) ? buffera : bufferb; } }
        static MemoryStream bufferb = new MemoryStream(Size * Size * 4);
        static MemoryStream buffera = new MemoryStream(Size * Size * 4);
        static int ongoingScreenshots = 0;

        static DateTime last = DateTime.Now;

        private static void Browser_NewScreenshot(object sender, EventArgs e)
        {
            var now = DateTime.Now;
            Log(1 / (now - last).TotalSeconds + "hz");
            last = now;


            var iAm = Interlocked.Increment(ref ongoingScreenshots);
            if (iAm > 1)
                Log("Theading error!!!");

            var screenShot = browser.ScreenshotOrNull();
            if (screenShot != null)
            {
                backbuffer.SetLength(0);//'clear'
                screenShot.Save(backbuffer, ImageFormat.Png);
                screenShot.Dispose();
                swap ^= true;
            }
            Interlocked.Decrement(ref ongoingScreenshots);
        }
       
     
    }
}