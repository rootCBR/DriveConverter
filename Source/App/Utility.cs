using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DriveConverter.App
{
    public static class Utility
    {
        public static class Log
        {
            public static void ToConsole(string text)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine(text);
                Console.ResetColor();
            }
        }
    }
}
