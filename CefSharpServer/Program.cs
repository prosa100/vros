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
        const int Port = 8084;
        static ChromiumWebBrowser browser;
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

            
            browser = new ChromiumWebBrowser();
            browser.Size = new Size(512, 512);
           
            while (!browser.IsBrowserInitialized)
                Thread.Sleep(0);
            browser.Load("en.m.wikipedia.org");

            browser.Load("http://www.timeanddate.com/worldclock/fullscreen.html?n=3704");
            browser.Load("https://en.m.wikipedia.org/wiki/Virtual_reality");

            browser.Load("https://m.reddit.com");

            browser.NewScreenshot += Browser_NewScreenshot;
            
            Log("Hello");
            using (HttpListener listener = new HttpListener())
            {
                listener.Prefixes.Add($"http://*:{Port}/");
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
                    buffer.Position = 0;
                    buffer.CopyTo(ctx.Response.OutputStream);
                    ctx.Response.OutputStream.Close();
                }
            }
        }

        static MemoryStream buffer = new MemoryStream(512 * 512 * 4);

        private static void Browser_NewScreenshot(object sender, EventArgs e)
        {
            buffer.SetLength(0);//semi-clear. Does not have to be too fast.
            var screenShot = browser.ScreenshotOrNull();
            screenShot.Save(buffer, ImageFormat.Png);
            screenShot.Dispose();
        }
       
     
    }
}