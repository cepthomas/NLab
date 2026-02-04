using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
//using Ephemera.NBagOfTricks;


namespace NLab
{
    #region Types
    /// <summary>General categories, mainly for logging.</summary>
    public enum Cat { None, Error, Info }

    /// <summary>Comm has something to tell the user.</summary>
    public class NotifEventArgs(Cat cat, string msg) : EventArgs
    {
        public Cat Cat { get; init; } = cat;
        public string Message { get; init; } = msg;
    }
    #endregion


    public static class Utils
    {
        public const string ERR = "ERR";
        public const string WRN = "WRN";
        public const string INF = "INF";
        public const string NON = "---";

        static long _startTick = Stopwatch.GetTimestamp();

        /// <summary>Tell me something good.</summary>
        /// <param name="msg">What</param>
        public static void Tell(string cat, string msg,
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = -1,
            [CallerMemberName] string member = "???")
        {
            long tick = Stopwatch.GetTimestamp();
            int tid = Environment.CurrentManagedThreadId;
            var fn = Path.GetFileName(file);
            double sec = 1.0 * (tick - _startTick) / Stopwatch.Frequency;
            //double msec = 1000.0 * (tick - _startTick) / Stopwatch.Frequency;

            var s = $"{sec:000.000}({tid}) {cat} {fn}({line}) <{member}> {msg}";

            Console.ForegroundColor = cat switch
            {
                ERR => ConsoleColor.Red,
                INF => ConsoleColor.Cyan,
                _ => ConsoleColor.White
            };

            Console.WriteLine(s);
            Console.ResetColor();

            //Log(cat, s);
            //txtViewer.AppendLine(s);
        }

    }
}
