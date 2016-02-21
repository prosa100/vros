using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using CefSharp.Internals;
using CefSharp.OffScreen;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using OpenTK.Input;
using static System.Drawing.Color;
using System.Runtime.CompilerServices;
using CefSharp;
using System.Net;
using System.Drawing.Imaging;
using static CefSharpServer.Debug;
namespace CefSharpServer
{
    public class Program : GameWindow
    {
        const int Port = 8084;
        int textureId = -1;
        static ChromiumWebBrowser browser;
        [STAThread]
        public static void Main(string[] args)
        {
            //var app = new Program();
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
            //browser.Load("https://www.youtube.com/watch?v=fPmruHc4S9Q");
            browser.Load("http://www.timeanddate.com/worldclock/fullscreen.html?n=3704");
            browser.NewScreenshot += Browser_NewScreenshot;
           // CefBrowserHostWrapper cbhw;
            //CefSharpBrowserWrapper csbw;
            

            Log("Hello");
            using (HttpListener listener = new HttpListener())
            {
                listener.Prefixes.Add($"http://*:{Port}/");
                listener.Start();
                while (listener.IsListening)
                {
                    Log("Go Go GO!!!");
                    var ctx = listener.GetContext();
                    var url = ctx.Request.QueryString["url"];
                    if (url != null)
                        browser.Load(url);
                    ctx.Response.ContentType = "image/png";
                    ctx.Response.StatusCode = 200;
                    buffer.Position = 0;
                    buffer.CopyTo(ctx.Response.OutputStream);
                    //var ss = browser.ScreenshotOrNull();
                    //ss?.Save(ctx.Response.OutputStream, ImageFormat.Png);
                    ctx.Response.OutputStream.Close();
                }
            }

            //app.Run(60);
        }

        static MemoryStream buffer = new MemoryStream(512 * 512 * 4);

        private static void Browser_NewScreenshot(object sender, EventArgs e)
        {
            buffer.SetLength(0);//semi-clear. Does not have to be too fast.
            var screenShot = browser.ScreenshotOrNull();
            screenShot.Save(buffer, ImageFormat.Png);
            screenShot.Dispose();
        }

        protected override void OnLoad(EventArgs e)
        {
            VSync = VSyncMode.On;
            Title = $"CefSharpServer <Using {GL.GetString(StringName.Version)}>";

            var browser = new OpenGlChromiumWebBrowser(this, "en.m.wikipedia.org");
            textureId = browser.textureId;
            //Load Gen+Load fail texture.
            //Format32bppPArgb

            base.OnLoad(e);
        }
        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
            base.OnResize(e);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            if (Keyboard[Key.Escape])
                Exit();
            base.OnUpdateFrame(e);
        }


        static void setupFrame()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.MatrixMode(MatrixMode.Projection);
            GL.Enable(EnableCap.Texture2D);
            GL.LoadIdentity();
            GL.Ortho(0, 1, 0, 1, -1, 1);
            GL.Color3(White);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            //render graphics
            setupFrame();
            GL.BindTexture(TextureTarget.Texture2D, textureId);
            GraphicsUtils.MissingTexture("???").CopyTo(textureId);
            GL.Begin(PrimitiveType.Quads);
            Action<double, double> t = (x, y) => { GL.Vertex2(x, y); GL.TexCoord2(y, x); };
            t(0, 0);t(0, 1);t(1, 1);t(1, 0);

            GL.End();

            base.OnRenderFrame(e);

            SwapBuffers();
        }
    }
}