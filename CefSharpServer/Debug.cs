using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CefSharpServer
{
    public static class Debug
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string GetCurrentMethod(int index = 1) => new StackTrace().GetFrame(index).GetMethod().Name;

        public static void Log(dynamic str) => Console.WriteLine(str);
    
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void PrintName() => Log(GetCurrentMethod(2));
    }
}
