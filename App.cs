using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClassHierarchy;


namespace NLab
{
    public class App
    {
        readonly long _startTick = Stopwatch.GetTimestamp();

        public static App Instance { get; private set; }   

        public App()
        {
            InitializeComponent();

            Instance = this;

            //AsyncPlay ap = new();
            //ap.Notif += (object? sender, NotifEventArgs e) => Print(e.Message);
            //ap.Go();

        }

        protected override void OnLoad(EventArgs e)
        {

            Print("App.OnLoad() calling TestMethod() 1");

            RunTestMethod("here we go");

            Print("App.OnLoad() calling TestMethod() 2");

            RunTestMethod("try again");

            Print("App.OnLoad() finished TestMethod()");

            base.OnLoad(e);
        }


        [LogMethodExecution("testing 123")]
        void RunTestMethod(string s)
        {
            Print($"App.RunTestMethod({s}) entry");
            using var sc = new Scoper("1-1-1-1");
            int l = s.Length;
            Print($"string is {l} long")
            Print($"App.RunTestMethod() exit");
        }

        public void Print(string s)
        {
            long tick = Stopwatch.GetTimestamp();
            double sec = 1.0 * (tick - _startTick) / Stopwatch.Frequency;
            //double msec = 1000.0 * (tick - _startTick) / Stopwatch.Frequency;
            s = $"{sec:000.000} {s}{Environment.NewLine}";
            Output.AppendText(s);
        }


        /// <summary>
        /// Write a line to console.
        /// </summary>
        /// <param name="cat">Category</param>
        /// <param name="text">What to print</param>
        void Print(Cat cat, string text)
        {
            var catColor = cat switch
            {
                Cat.Error => _config.ErrorColor,
                Cat.Info => _config.InfoColor,
                _ => ConsoleColorEx.None
            };

            //  If color not explicitly specified, look for text matches.
            if (catColor == ConsoleColorEx.None)
            {
                foreach (var m in _config.Matchers)
                {
                    if (text.Contains(m.Key)) // faster than compiled regexes
                    {
                        catColor = m.Value;
                        break;
                    }
                }
            }

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
            if (_logStream is not null)
            {
                long tick = Stopwatch.GetTimestamp();
                double sec = 1.0 * (tick - _startTick) / Stopwatch.Frequency;
                //double msec = 1000.0 * (tick - _startTick) / Stopwatch.Frequency;

                var scat = cat switch
                {
                    Cat.Send => ">>>",
                    Cat.Receive => "<<<",
                    Cat.Error => "!!!",
                    Cat.Info => "---",
                    Cat.None => "---",
                    _ => throw new NotImplementedException(),
                };

                var s = $"{sec:000.000} {scat} {text}{Environment.NewLine}";
                _logStream.Write(Encoding.Default.GetBytes(s));
                _logStream.Flush();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void Prompt()
        {
            Console.Write(_config.Prompt);
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
            App.Instance.Print($"Scoper.Scoper({id})");
        }

        public void Dispose()
        {
           // _captures.Add($"{_id}: Scoper.Dispose()");
            App.Instance.Print($"Scoper.Dispose() [{_id}]");
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
