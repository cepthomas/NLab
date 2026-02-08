using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Ephemera.NBagOfTricks;
using Ephemera.NBagOfUis;


namespace NLab
{
    public static class Utils
    {
        public const string ERR = "ERR";
        public const string INF = "INF";
        public const string DBG = "---";

        static long _startTick = 0;// Stopwatch.GetTimestamp();

        /// <summary>Tell me something good.</summary>
        /// <param name="cat">What</param>
        /// <param name="msg">What</param>
        /// <param name="depth">Info stack position</param>
        public static void Tell(string cat, string msg, int depth = 2) // 2 is usual
        {
            var fn = "???";
            var line = -1;

            if (depth > 0)
            {
                // Get the caller info.
                var st = new StackTrace(true);
                var frm = st.GetFrame(depth);

                if (frm is not null)
                {
                    fn = Path.GetFileName(frm.GetFileName());
                    line = frm.GetFileLineNumber();
                }
            }

            long tick = Stopwatch.GetTimestamp();
            int tid = Environment.CurrentManagedThreadId;
            double msec = Msec();
            double sec = msec / 1000.0;

            var s = $"{(int)msec:0000.000} T:{tid} {cat} {fn}({line}) [{msg}]";

            Console.ForegroundColor = cat switch
            {
                ERR => ConsoleColor.Red,
                DBG => ConsoleColor.Cyan,
                _ => ConsoleColor.White
            };

            Console.WriteLine(s);
            Console.ResetColor();
        }

        public static void Reset()
        {
            _startTick = Stopwatch.GetTimestamp();
        }

        /// <summary>
        /// Get current msec.
        /// </summary>
        /// <returns></returns>
        public static int Msec()
        {
            return (int)(1000 * (Stopwatch.GetTimestamp() - _startTick) / Stopwatch.Frequency);
        }

        /// <summary>
        /// Simulate synchronous real-world/time work. This is a bad idea except for very short delays.
        /// </summary>
        /// <param name="msec"></param>
        public static void SyncTimeEater(int msec)
        {
            var start = Msec();
            while (Msec() < start + msec) { }
        }
    }
}
