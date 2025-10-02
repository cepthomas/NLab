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
    public partial class MainForm : Form
    {
        readonly long _startTick = Stopwatch.GetTimestamp();

        public static MainForm Instance { get; private set; }   

        public MainForm()
        {
            InitializeComponent();

            Instance = this;

            //AsyncPlay ap = new();
            //ap.Notif += (object? sender, NotifEventArgs e) => Print(e.Message);
            //ap.Go();

        }

        protected override void OnLoad(EventArgs e)
        {

            Print("MainForm.OnLoad() calling TestMethod() 1");

            RunTestMethod("here we go");

            Print("MainForm.OnLoad() calling TestMethod() 2");

            RunTestMethod("try again");

            Print("MainForm.OnLoad() finished TestMethod()");

            base.OnLoad(e);
        }


        [LogMethodExecution("testing 123")]
        void RunTestMethod(string s)
        {
            Print($"MainForm.RunTestMethod({s}) entry");
            using var sc = new Scoper("1-1-1-1");
            int l = s.Length;
            Print($"string is {l} long")
            Print($"MainForm.RunTestMethod() exit");
        }

        public void Print(string s)
        {
            long tick = Stopwatch.GetTimestamp();
            double sec = 1.0 * (tick - _startTick) / Stopwatch.Frequency;
            //double msec = 1000.0 * (tick - _startTick) / Stopwatch.Frequency;
            s = $"{sec:000.000} {s}{Environment.NewLine}";
            Output.AppendText(s);
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
            MainForm.Instance.Print($"Scoper.Scoper({id})");
        }

        public void Dispose()
        {
           // _captures.Add($"{_id}: Scoper.Dispose()");
            MainForm.Instance.Print($"Scoper.Dispose() [{_id}]");
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
