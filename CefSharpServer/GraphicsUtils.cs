using OpenTK.Graphics.OpenGL;
using System.Drawing;
using static System.Drawing.Color;
using static CefSharpServer.Debug;

namespace CefSharpServer
{
    static class GraphicsUtils
    {
        public static int MakeTexture()
        {

            var textureId = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, textureId);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            return textureId;
        }

        public static Bitmap MissingTexture(dynamic str)
        {
            
            var b = new Bitmap(512, 512);
            using (var g = Graphics.FromImage(b))
            {
                //g.Clear(Magenta);
                g.DrawString($"No Texture\n{str}\n☢☃\n----------\n^^^^^^^^^^", new Font("Consolas", 64), Brushes.White, PointF.Empty);
                //g.DrawString($"No Texture!\n{id}", new Font("Tahoma", 64), Brushes.Cyan, PointF.Empty);
                g.Flush();
            }
            return b;
        }
        
        public static void CopyTo(this Bitmap b, int openGlId)
        {
            
            var data = b.LockBits(
                new Rectangle(Point.Empty, b.Size),
                System.Drawing.Imaging.ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppPArgb
            );
            
            GL.BindTexture(TextureTarget.Texture2D, openGlId);

            GL.TexImage2D(
                TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                b.Width, b.Height,
                0, PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0
            );

            b.UnlockBits(data);
        }
    }
}
