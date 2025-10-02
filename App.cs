using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Ephemera.NBagOfTricks;
using ClassHierarchy;


namespace NLab
{
    public class App
    {
        static void Main(string[] args)
        {
            using var app = new App();
        }
        
        readonly long _startTick = Stopwatch.GetTimestamp();

        public App()
        {
            Print(Cat.None, "App.OnLoad() calling TestMethod() 1");

            RunTestMethod("here we go");

            Print(Cat.None, "App.OnLoad() calling TestMethod() 2");

            RunTestMethod("try again");

            Print(Cat.None, "App.OnLoad() finished TestMethod()");

            AsyncPlay ap = new();
            ap.Notif += (object? sender, NotifEventArgs e) => Print(Cat.Info, e.Message);
            ap.Go();
        }


        [LogMethodExecution("testing 123")]
        void RunTestMethod(string s)
        {
            Print(Cat.None, $"App.RunTestMethod({s}) entry");
            using var sc = new Scoper("1-1-1-1");
            int l = s.Length;
            Print(Cat.Info, $"string is {l} long");
            Print(Cat.None, $"App.RunTestMethod() exit");
        }


        /// <summary>
        /// Write a line to console.
        /// </summary>
        /// <param name="cat">Category</param>
        /// <param name="text">What to print</param>
        void Print(Cat cat, string text)
        {
            long tick = Stopwatch.GetTimestamp();
            double sec = 1.0 * (tick - _startTick) / Stopwatch.Frequency;
            //double msec = 1000.0 * (tick - _startTick) / Stopwatch.Frequency;
            text = $"{sec:000.000} {text}{Environment.NewLine}";

            var catColor = cat switch
            {
                Cat.Error => ConsoleColorEx.Red,
                Cat.Info => ConsoleColorEx.Cyan,
                _ => ConsoleColorEx.None
            };

            // //  If color not explicitly specified, look for text matches.
            // if (catColor == ConsoleColorEx.None)
            // {
            //     foreach (var m in _config.Matchers)
            //     {
            //         if (text.Contains(m.Key)) // faster than compiled regexes
            //         {
            //             catColor = m.Value;
            //             break;
            //         }
            //     }
            // }

            if (catColor != ConsoleColorEx.None)
            {
                Console.ForegroundColor = (ConsoleColor)catColor;
            }

            Console.Write(text);
            Console.Write(Environment.NewLine);
            Console.ResetColor();
            
            Log(cat, text);
        }

        /// <summary>
        /// Write to logger.
        /// </summary>
        /// <param name="cat">Category</param>
        /// <param name="text">What to print</param>
        void Log(Cat cat, string text)
        {
            //if (_logStream is not null)
            //{
            //    long tick = Stopwatch.GetTimestamp();
            //    double sec = 1.0 * (tick - _startTick) / Stopwatch.Frequency;
            //    //double msec = 1000.0 * (tick - _startTick) / Stopwatch.Frequency;

            //    var scat = cat switch
            //    {
            //        Cat.Error => "!!!",
            //        Cat.Info => "---",
            //        Cat.None => "---",
            //        _ => throw new NotImplementedException(),
            //    };

            //    var s = $"{sec:000.000} {scat} {text}{Environment.NewLine}";
            //    _logStream.Write(Encoding.Default.GetBytes(s));
            //    _logStream.Flush();
            //}
        }

        /// <summary>
        /// 
        /// </summary>
        void Prompt()
        {
            Console.Write("Prompt");
        }


        
    }


    ///////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////// Scoper //////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////


    /// <summary>TODO Experimental class to log enter/exit scope.</summary>
    //TODO Use attribute? https://medium.com/@nwonahr/creating-and-using-custom-attributes-in-c-for-asp-net-core-c4f7d3db1829
    public class Scoper : IDisposable
    {
        readonly string _id;
        static readonly List<string> _captures = [];

        public static List<string> Captures { get { return _captures; } }

        public Scoper(string id)
        {
            _id = id;
           // _captures.Add($"{_id}: Scoper.Scoper(string id)");
            Console.WriteLine($"Scoper.Scoper({id})");
        }

        public void Dispose()
        {
            // _captures.Add($"{_id}: Scoper.Dispose()");
            Console.WriteLine($"Scoper.Dispose() [{_id}]");
        }
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class LogMethodExecutionAttribute : Attribute
    {
        public string LogMessage { get; }

        public LogMethodExecutionAttribute(string logMessage)
        {
            LogMessage = logMessage;
        }
    }
}
