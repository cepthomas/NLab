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

        //void Deconstruct() { }

        public void Info(string text)
        {
            Tell(INF, $"{text}", 2);
        }

        public void Assert(bool condition, object? actual = null, [CallerArgumentExpression(nameof(condition))] string expr = "???")
        {
            if (!condition)
            {
                if (actual is null)
                {
                    Tell(ERR, $"{expr}");
                }
                else
                {
                    Tell(ERR, $"{expr} actual:{actual}");
                }
            }
        }
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class TracerMethodAttribute(string msg, int num) : Attribute
    {
        public string Message { get; } = msg;
        public int Num { get; } = num;
    }


    public static class Verify // TODO parts may be useful for tracer.
    {
        public static void Argument(bool condition, string message, [CallerArgumentExpression("condition")] string conditionExpression = null)
        {
            if (!condition) throw new ArgumentException(message: message, paramName: conditionExpression);
        }

        public static void InRange(int argument, int low, int high,
            [CallerArgumentExpression("argument")] string argumentExpression = null,
            [CallerArgumentExpression("low")] string lowExpression = null,
            [CallerArgumentExpression("high")] string highExpression = null)
        {
            if (argument < low)
            {
                throw new ArgumentOutOfRangeException(paramName: argumentExpression, message: $"{argumentExpression} ({argument}) cannot be less than {lowExpression} ({low}).");
            }

            if (argument > high)
            {
                throw new ArgumentOutOfRangeException(paramName: argumentExpression, message: $"{argumentExpression} ({argument}) cannot be greater than {highExpression} ({high}).");
            }
        }

        public static void NotNull<T>(T argument, [CallerArgumentExpression("argument")] string argumentExpression = null) where T : class
        {
            if (argument == null) throw new ArgumentNullException(paramName: argumentExpression);
        }

        static T Single<T>(this T[] array)
        {
            Verify.NotNull(array); // paramName: "array"
            Verify.Argument(array.Length == 1, "Array must contain a single element."); // paramName: "array.Length == 1"
            return array[0];
        }

        static T ElementAt<T>(this T[] array, int index)
        {
            Verify.NotNull(array); // paramName: "array"
                                   // paramName: "index"
                                   // message: "index (-1) cannot be less than 0 (0).", or
                                   //          "index (6) cannot be greater than array.Length - 1 (5)."
            Verify.InRange(index, 0, array.Length - 1);
            return array[index];
        }
    }

}
