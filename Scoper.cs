using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Reflection;
using static NLab.Utils;


namespace NLab
{
    /// <summary>TODO1 rename? Experimental class to log enter/exit scope.</summary>
    public class Scoper : IDisposable
    {
        readonly string _id;
        readonly int _tid;
        private bool disposedValue;
        //static readonly List<string> _captures = [];

        //public static List<string> Captures { get { return _captures; } }

        //public delegate void Writer(string s);
        //static Writer _writer; // = writer;
        //// Do once in module:
        //[MethodImpl(MethodImplOptions.NoInlining)]
        //public static void TLOG_INIT(Writer writer)
        //{
        //    _writer = writer;
        //}


        [MethodImpl(MethodImplOptions.NoInlining)]
        public Scoper()
        {
            _id = "TODO1";

            // Get thread id.
            _tid = Environment.CurrentManagedThreadId;

            Tell(DBG, $"Scoper constructor T:{_tid}", 2);
            //_captures.Add($"{_func}<{_tid}>: Scoper constructor");
        }

        ~Scoper()
        {
            Tell(DBG, $"Scoper destructor {_id} T!{_tid}", 1);
            //Tell(DBG, $"Scoper destructor {_func}({line}) T!{_tid}");
            //_captures.Add($"{_func}<{_tid}>: Scoper destructor");
        }

        public void Dispose()
        {
            Tell(DBG, $"Scoper dispose {_id} T{_tid}", 1);
            //Tell(DBG, $"Scoper dispose {_func}({line}) T{_tid}");
            //_captures.Add($"{_func}<{_tid}>: Scoper dispose");
        }


        ////////////////////////////// TODO1 these /////////////////////////////////////////////////////////
        public bool TLOG_EQUAL<T>(T value1, T value2) where T : IComparable
        {
            bool pass = true;
            if (value1.CompareTo(value2) != 0)
            {
                Tell(ERR, $"[{value1}] should be [{value2}]", 2);
                pass = false;
            }
            return pass;
        }

        public void TLOG_ASSERT(bool b)
        {
            if (!b)
            {
                Tell(ERR, $"assert failed", 2);
            }
        }

        public void TLOG_INFO(string text)
        {
            Tell(INF, $"{text}", 2);
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

    //public class LogMethodExecutionFilter : IActionFilter
    //{
    //    public void OnActionExecuting(ActionExecutingContext context)
    //    {
    //        var method = context.ActionDescriptor.MethodInfo;
    //        var logAttribute = method.GetCustomAttribute<LogMethodExecutionAttribute>();
    //        if (logAttribute != null)
    //        {
    //            // Your logging logic here
    //            Console.WriteLine($"Log: {logAttribute.LogMessage}");
    //        }
    //    }
    //    public void OnActionExecuted(ActionExecutedContext context)
    //    {
    //        // Post-action logic
    //    }
    //}
}
