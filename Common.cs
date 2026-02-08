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
    #region Types
    ///// <summary>General categories, mainly for logging.</summary>
    //public enum Cat { Error, Info, Debug }

    ///// <summary>Comm has something to tell the user.</summary>
    //public class NotifEventArgs(Cat cat, string msg) : EventArgs
    //{
    //    public Cat Cat { get; init; } = cat;
    //    public string Message { get; init; } = msg;
    //}
    #endregion


    public static class Utils
    {
        public const string ERR = "ERR";
        public const string INF = "INF";
        public const string DBG = "---";

        static readonly long _startTick = Stopwatch.GetTimestamp();

        static public TextViewer? Output { get; set; } = null;

        /// <summary>Tell me something good.</summary>
        /// <param name="msg">What</param>
        public static void Tell(string cat, string msg, int depth = 0)
        {
            // The caller info is usually not useful as the material of interest is known only to the calling function.
            //var fn = Path.GetFileName(callerFile);

            var fn = "???";
            var line = -1;

            if (depth > 0)
            {
                // Get the caller info.
                var st = new StackTrace(true);
                var frm = st.GetFrame(depth);

                if (frm is not null )
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

            if (Output is not null) //TODO1 fix these
            {
                Output.AppendMatch(s);
            }
            else
            {
                Console.ForegroundColor = cat switch
                {
                    ERR => ConsoleColor.Red,
                    DBG => ConsoleColor.Cyan,
                    _ => ConsoleColor.White
                };

                Console.WriteLine(s);
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Get current msec.
        /// </summary>
        /// <returns></returns>
        static int Msec()
        {
            return (int)(1000 * (Stopwatch.GetTimestamp() - _startTick) / Stopwatch.Frequency);
        }

        /// <summary>
        /// Simulate synchronous real-world/time work.
        /// </summary>
        /// <param name="msec"></param>
        public static void SyncTimeEater(int msec)
        {
            var start = Msec();

            while (Msec() < start + msec)
            {
                // This is a bad idea except for very short delays.
            }
        }
    }
}
