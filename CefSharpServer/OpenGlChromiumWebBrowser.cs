using System.Drawing;

using CefSharp;
using CefSharp.Internals;
using static CefSharpServer.Debug;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace CefSharpServer
{
    public class OpenGlChromiumWebBrowser : CefSharp.OffScreen.ChromiumWebBrowser
    {

        public OpenGlChromiumWebBrowser(GameWindow game, string address = "",
            BrowserSettings browserSettings = null, RequestContext requestcontext = null,
            bool automaticallyCreateBrowser = true)
            : base(address, browserSettings, requestcontext, automaticallyCreateBrowser)
        {
            
            Size = new Size(512, 512);

            GraphicsUtils.MissingTexture("S-Man").CopyTo(textureId = GraphicsUtils.MakeTexture());

            game.Closed += (sender, e) =>
            {
                Cef.Shutdown();
            };

            game.UpdateFrame += Update;
        }

        private void Update(object sender, FrameEventArgs e)
        {
            GraphicsUtils.MissingTexture("gMan").CopyTo(textureId);
        }

        //public event EventHandler<BitmapInfo> OnInvokeRenderAsync;

        public override void InvokeRenderAsync(BitmapInfo bitmapInfo)
        {
            Log("Browser Rendered");
            GL.BindTexture(TextureTarget.Texture2D, textureId);
            GL.TexImage2D(
                    TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                    Size.Width, Size.Height,
                    0, PixelFormat.Bgra, PixelType.UnsignedByte, bitmapInfo.BackBufferHandle);
            base.InvokeRenderAsync(bitmapInfo);
            Log(bitmapInfo.BackBufferHandle);
            var b= ScreenshotOrNull();
            b.CopyTo(textureId);
            //Enum.GetNames()
            //Log( GL.GetError().ToString());
            b.Save("s.png");

            
            //GraphicsUtils.MissingTexture("The Render Man").CopyTo(textureId);
        }

        public int textureId;
    }
}
