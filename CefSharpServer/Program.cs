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
        private static ChromiumWebBrowser[] browsers = new ChromiumWebBrowser[20];
        private static Mutex mutex = new Mutex();
        [STAThread]
        public static void Main(string[] args)
        {
            if (!Cef.IsInitialized)
                Cef.Initialize(new CefSettings()
                {
                    UserAgent = "Mozilla/5.0 (Windows; U; Android 5.0.3; en-us; vrshare) AppleWebkit/537.36 (KHTML, like Gecko) Chrome/47.0.2526.106 Mobile Safari/537.36"
                }, shutdownOnProcessExit: true, performDependencyCheck: true);
            else
                Log("Why?");

            
            //browser = new ChromiumWebBrowser();
            //browser.Size = new Size(512, 512);
           
            //while (!browser.IsBrowserInitialized)
            //    Thread.Sleep(0);
            //browser.Load("en.m.wikipedia.org");

            //browser.Load("http://www.timeanddate.com/worldclock/fullscreen.html?n=3704");
            //browser.NewScreenshot += Browser_NewScreenshot;
            
            Log("Hello");
            using (HttpListener listener = new HttpListener())
            {
                listener.Prefixes.Add($"http://*:{Port}/");
                listener.Start();
                while (listener.IsListening)
                {
                    /**
                     * Commands:
                     *    http:localhost:8084/open?tab=[id]
                     *         -- create a new tab, id'd by #id.
                     *    http:localhost:8084/close?tab=[id]
                     *         -- close tab #id.
                     *    http:localhost:8084/goto?url=[dest]?tab=[id]
                     *         -- this loads the page specified by url
                     *            in the browser tagged by #id.
                     *    http:localhost:8084/click?pos=[x] [y]?tab=[id]
                     *         -- this clicks on coordinate (x, y) in #id.
                     *    http:localhost:8084/scroll?dir=[direction]?tab=[id]
                     *         -- scrolls 50 px up, down, left or right in #id.
                     */
                    var ctx   = listener.GetContext();
                    var rqst  = ctx.Request.RawUrl;
                    int tad = 0;
                    string[] splitreq = rqst.Split('?');
                    string cmd, args1, tabarg;
                    if (splitreq.Length > 0)
                    {
                        cmd = splitreq[0];
                        args1 = (splitreq.Length > 1) ? splitreq[1] : null;
                        tabarg = (splitreq.Length > 2) ? splitreq[2].Substring(splitreq[2].IndexOf("=")) : null;

                        if (tabarg != null && int.TryParse(tabarg, out tad)) ;
                        else continue;
                    }
                    else continue;
                    int tabid = tad;
                    switch (cmd)
                    {
                        case ("open"):
                            int tabd;
                            try { if (int.TryParse(ctx.Request.QueryString["tab"], out tabd))
                                {
                                    if ((tabd < 20) && (browsers[tabd] == null))
                                    {
                                        browsers[tabid = tabd] = new ChromiumWebBrowser("https://google.com");
                                        browsers[tabd].Size = new Size(512, 512);
                                        for (; !browsers[tabd].IsBrowserInitialized;)
                                            Thread.Sleep(10);
                                    }
                                }
                            } catch
                            {
                                /** Malformed requests are ignored. **/
                            }
                            goto default;
                       case ("close"):
                            int closer;
                            try { if (int.TryParse(ctx.Request.QueryString["tab"], out closer))
                                {
                                    browsers[closer] = null;
                                }
                            } catch
                            {
                                /** caught **/
                            }
                            
                            goto default;
                        case ("goto"):
                            var url = ctx.Request.QueryString["url"];
                            if (browsers[tabid] != null)
                                browsers[tabid].Load(url);                      
                            goto default;
                        case ("click"):
                            var clk_s = ctx.Request.QueryString["pos"];
                            int clk_x, clk_y, tidc;

                            try {
                                if (int.TryParse(clk_s.Substring(0, clk_s.IndexOf(" ")), out clk_x) &&
                                    int.TryParse(clk_s.Substring(clk_s.IndexOf(" ")), out clk_y) &&
                                    int.TryParse(tabarg, out tidc))
                                {
                                    if (browsers[tabid] != null)
                                        browsers[tabid].ExecuteScriptAsync("document.elementFromPoint(" + clk_x + ", " + clk_y + ").click()");
                                }
                            } catch
                            {
                                /** Malformed Requests will be ignored. **/
                            }
                            goto default;
                        case ("scroll"):
                            var dir = ctx.Request.QueryString["dir"];   
                            switch (dir)
                            {
                                case ("up"):
                                    browsers[tabid].ExecuteScriptAsync("scrollBy(0,-50)");
                                    break;
                                case ("down"):
                                    browsers[tabid].ExecuteScriptAsync("scrollBy(0, 50)");
                                    break;
                                case ("left"):
                                    browsers[tabid].ExecuteScriptAsync("scrollBy(-50,0)");
                                    break;
                                case ("right"):
                                    browsers[tabid].ExecuteScriptAsync("scrollBy(0, 50)");
                                    break;
                            }
                            goto default;
                        default:
                            /** Grab the next screenshot **/
                            if (browsers[tabid] == null)
                                continue;
                            for (; browsers[tabid].IsLoading;)
                                Thread.Sleep(100);
                            browsers[tabid].NewScreenshot += Browser_NewScreenshot;
                            break;
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
            var screenShot = ((ChromiumWebBrowser) sender).ScreenshotOrNull();
            screenShot.Save(buffer, ImageFormat.Png);
            screenShot.Dispose();
        }
       
     
    }
}