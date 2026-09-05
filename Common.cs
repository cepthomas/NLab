using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Ephemera.NBagOfTricks;
using Ephemera.NBagOfUis;


namespace NLab
{
    ///// <summary>Internal exception.</summary>
    //public class LabException(string msg, bool isError = true) : Exception(msg)
    //{
    //    public bool IsError { get; } = isError;
    //}

    [Serializable]
    public sealed class HotKey_old
    {
        public string Key { get; set; } = "?";
        public bool Ctrl { get; set; } = false;
        public bool Alt { get; set; } = false;
        public bool Shift { get; set; } = false;
        public bool Win { get; set; } = false;
    }

    public static class Utils_old
    {
        public const string ERR = "ERR";
        public const string INF = "INF";
        public const string DBG = "---";

        static long _startTick = 0;// Stopwatch.GetTimestamp();

        /// <summary>Tell me something good.</summary>
        /// <param name="cat">What</param>
        /// <param name="msg">What</param>
        /// <param name="depth">Info stack position</param>
        public static void TellXXX(string cat, string msg, int depth = 2) // 2 is usual
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
    }

}
