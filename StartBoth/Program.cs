using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StartBoth
{
    class Program
    {
        static void Main(string[] args)
        {
            var cefSharpServer = new Thread(CefSharpServer.Program.Main);
            var desktopServer = new Thread(DesktopServer.Program.Main);
            cefSharpServer.Start();
            desktopServer.Start();
            Console.ReadLine();
        }
    }
}
