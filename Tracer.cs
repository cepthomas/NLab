using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;
//using static NLab.Utils;


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
            Console.WriteLine($"Tracer constructor T:{_thread}");
        }

        ~Tracer()
        {
            Console.WriteLine($"Tracer destructor {_id} T:{_thread}");
        }

        public void Dispose()
        {
            Console.WriteLine($"Tracer dispose {_id} T:{_thread}");
        }

        public void Info(string text)
        {
            Console.WriteLine($"INF {text}");
        }

        public void Assert(bool condition, object? actual = null, [CallerArgumentExpression(nameof(condition))] string expr = "???")
        {
            if (!condition)
            {
                if (actual is null)
                {
                    Console.WriteLine($"ERR {expr}");
                }
                else
                {
                    Console.WriteLine($"ERR {expr} actual:{actual}");
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

    public class TracerTest
    {
        public int Go(double dval, Rectangle rect)
        {
            using var tr = new Tracer();

            // Check args.
            tr.Assert(dval == 6.7); // false - fail
            tr.Assert(rect.Height == 999); // false - fail

            var m1res = TestMethod1("here-we-go", 10101);

            var m2res = TestMethod1("try-again", 20202);

            var res = m2res - m1res;
            tr.Assert(res == 543); // false - fail

            tr.Assert(m1res < m2res); // false - fail

            tr.Info($"more asserts");
            List<int>? ls = [23, 4, 695, 81, -34, 10000];
            tr.Assert(ls == null); // false - fail
            tr.Assert(ls != null); // true - pass
            tr.Assert(ls[1] == 4, ls[1]); // true - pass
            tr.Assert(ls[2] == 696, ls[2]); // false - fail

            tr.Info($">>> Leaving");

            return res;
        }

        [TracerMethod("Tracer testing level 1", 707)]
        public int TestMethod1(string s, int w)
        {
            using var tr = new Tracer();

            tr.Info($"entry s:{s} w:{w}");

            // do something
            s = new string(s.Reverse().ToArray());

            tr.Info($"exit s:{s}");

            return s.Length;
        }

        public void PlayWithAttribute()
        {
            var info = typeof(TracerTest).GetMember("TestMethod1");
            var attr = info[0].GetCustomAttribute<TracerMethodAttribute>();
            Console.WriteLine($"{attr.Num}:{attr.Message}");
        }
    }

    public static class Verify // TODO parts may be useful?
    {
        public static void Argument(bool condition, string message,
            [CallerArgumentExpression("condition")] string? conditionExpression = null)
        {
            if (!condition) throw new ArgumentException(message: message, paramName: conditionExpression);
        }

        public static void InRange(int argument, int low, int high,
            [CallerArgumentExpression("argument")] string? argumentExpression = null,
            [CallerArgumentExpression("low")] string? lowExpression = null,
            [CallerArgumentExpression("high")] string? highExpression = null)
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

        public static void NotNull<T>(T argument,
            [CallerArgumentExpression("argument")] string? argumentExpression = null)
            where T : class
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
            Verify.NotNull(array);
            Verify.InRange(index, 0, array.Length - 1);
            return array[index];
        }
    }
}
