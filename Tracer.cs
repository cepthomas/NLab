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
    /// <summary>Experimental class to log enter/exit scope.</summary>
    public class Tracer : IDisposable
    {
        static int _nextid = 1;
        readonly int _id;
        readonly int _thread;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Tracer()
        {
            _id = _nextid++;
            _thread = Environment.CurrentManagedThreadId;
            Tell(DBG, $"Tracer constructor T:{_thread}", 2);
        }

        ~Tracer()
        {
            Tell(DBG, $"Tracer destructor {_id} T!{_thread}", 1);
        }

        public void Dispose()
        {
            Tell(DBG, $"Tracer dispose {_id} T{_thread}", 1);
        }

        public void Info(string text)
        {
            Tell(INF, $"{text}", 2);
        }

        public void Assert(bool b)
        {
            if (!b)
            {
                Tell(ERR, $"assert failed", 2);
            }
        }

        public bool AssertEqual<T>(T value1, T value2) where T : IComparable // TODO more flavors like this?
        {
            bool pass = true;
            if (value1.CompareTo(value2) != 0)
            {
                Tell(ERR, $"[{value1}] should be [{value2}]", 2);
                pass = false;
            }
            return pass;
        }
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class TracerMethodAttribute(string msg, int num) : Attribute
    {
        public string Message { get; } = msg;
        public int Num { get; } = num;
    }
}
