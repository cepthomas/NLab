using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ephemera.NBagOfTricks;


// TODO clean up

namespace NLab
{
    class DelegateLambda // play?
    {
        // Delegates are really just structural typing for functions. You could do the same thing with nominal typing and 
        // implementing an anonymous class that implements an interface or abstract class, but that ends up being a lot of 
        // code when only one function is needed.

        // Lambda comes from the idea of lambda calculus of Alonzo Church in the 1930s. It is an anonymous way of creating 
        // functions. They become especially useful for composing functions

        // So while some might say lambda is syntactic sugar for delegates, I would says delegates are a bridge for easing 
        // people into lambdas in c#.

        // One difference is that an anonymous delegate can omit parameters while a lambda must match the exact signature. Given:
        public delegate string TestDelegate(int i);

        public void Test(TestDelegate d) { }

        // you can call it in the following four ways (note that the second line has an anonymous delegate that does not have any parameters):
        void Callit()
        {
            Test(delegate (int i) { return string.Empty; });
            Test(delegate { return string.Empty; });
            Test(i => string.Empty);
            Test(D);
        }

        private string D(int i)
        {
            return string.Empty;
        }

        private string D2()
        {
            return string.Empty;
        }

        // // You cannot pass in a lambda expression that has no parameters or a method that has no parameters. These are not allowed:
        // Test(() => String.Empty); // Not allowed, lambda must match signature
        // Test(D2); // Not allowed, method must match signature
    }

    static class Stuff
    {
        static void DumpStack()
        {
            // Get the caller info.
            var cinfo = new List<string>();
            var st = new StackTrace(true);
            int index = 0;
            while (index >= 0)
            {
                var frm = st.GetFrame(index);
                if (frm == null)
                {
                    index = -999; // done
                }
                else if (frm.GetFileName() is not null && frm.HasSource())
                {
                    var sfrm = $"<{index}: {frm.GetMethod().Name} in {frm.GetFileName()}({frm.GetFileLineNumber()})>";
                    cinfo.Add(sfrm);
                    index++;
                }
                else
                {
                    index++;
                }
            }

            var sinfo = string.Join($"{Environment.NewLine}", cinfo);
            // _output.Append(sinfo);
        }

        public static List<string> Dump()
        {
           List<string> res = [];
           //_itemds.ForEach(itemd => res.Add(itemd.Item.ToString()));
           return res;
        } // >>>>
        public static IEnumerable<U> Map<T, U>(this IEnumerable<T> s, Func<T, U> f)
        {
           foreach (var item in s)
               yield return f(item);
        }
    }
    
    class NTermTest
    {
        #region Fields
        /// <summary>User input</summary>
        readonly ConcurrentQueue<string> _qUserCli = new();

        /// <summary>LF=10  CR=13  NUL=0</summary>
        readonly byte _delim = 0;

        /// <summary>Config to use</summary>
        string _configFile = "???";

        /// <summary>Target executable</summary>
        string _ntermExe = "???";
        #endregion

        /// <summary>
        /// 
        /// </summary>
        public void Run()
        {
            Console.WriteLine($"========= Test =========");
            _configFile = Path.Combine(MiscUtils.GetSourcePath(), "test_config.ini");
            _ntermExe = Path.Combine(MiscUtils.GetSourcePath(), "..", "bin", "net8.0-windows", "win-x64", "NTerm.exe");

            using CancellationTokenSource ts = new();
            //using Task taskKeyboard = Task.Run(() => _qUserCli.Enqueue(Console.ReadLine() ?? ""));

            try
            {
                // Target flavors run binary NTerm.exe.
                //DoBasicTarget(ts);
                //DoConfigTarget(ts);
                //DoTcpTarget(ts);
                DoUdpTarget(ts);

                // Debugger flavors require starting NTerm with matching cmd line.
                //DoTcpDebugger(ts);
                //DoUdpDebugger(ts);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Fatal!! {e}");
                //Task.WaitAll([taskKeyboard]);
            }
        }

        /// <summary>
        /// Simple first test from cmd line. TODO also tcp/udp?
        /// </summary>
        void DoBasicTarget(CancellationTokenSource ts)
        {
            Console.WriteLine($"DoBasicTarget()");
            List<string> config = ["[nterm]", "comm = null", "delim = NUL", "prompt = >", "meta = -"];
            File.WriteAllLines(_configFile, config);
            var proc = RunTarget(_configFile);
        }

        /// <summary>
        /// Test config functions.
        /// </summary>
        void DoConfigTarget(CancellationTokenSource ts)
        {
            Console.WriteLine($"DoConfigTarget()");
            List<string> config = [
                "[nterm]", "comm = null", "delim = NUL", "prompt = >", "meta = -",
                "info_color = darkcyan", "err_color = green",
            "[macros]", "dox = \"do xxxxxxx\"", "s3 = \"hey, send 333333333\"", "tm = \"  xmagentax   -yellow-  \"",
            "[matchers]", "\"mag\" = magenta", "\"yel\" = yellow"];
            File.WriteAllLines(_configFile, config);
            var proc = RunTarget(_configFile);
        }

        /// <summary>
        /// Test tcp in command/response mode.
        /// </summary>
        void DoTcpTarget(CancellationTokenSource ts)
        {
            Console.WriteLine($"DoTcpTarget()");
            // Tweak config.
            List<string> config = [
                "[nterm]", "comm = tcp 127.0.0.1 59120", "delim = NUL", "prompt = >", "meta = -",
                "info_color = darkcyan", "err_color = green",
            "[macros]", "dox = \"do xxxxxxx\"", "s3 = \"hey, send 333333333\"", "tm = \"  xmagentax   -yellow-  \"",
            "[matchers]", "\"mag\" = magenta", "\"yel\" = yellow"];
            File.WriteAllLines(_configFile, config);
            var proc = RunTarget(_configFile);
            TcpServerStuff srv = new(59120, _delim);
            var err = srv.Run(ts);
        }

        /// <summary>
        /// Test udp in continuous mode.
        /// </summary>
        void DoUdpTarget(CancellationTokenSource ts)
        {
            Console.WriteLine($"DoUdpTarget()");
            // Tweak config.
            List<string> config = [
                "[nterm]", "comm = udp 127.0.0.1 59140", "delim = NUL", "prompt = >", "meta = -",
                "info_color = darkcyan", "err_color = green",
            "[macros]", "dox = \"do xxxxxxx\"", "s3 = \"hey, send 333333333\"", "tm = \"  xmagentax   -yellow-  \"",
            "[matchers]", "\"mag\" = magenta", "\"yel\" = yellow"];
            File.WriteAllLines(_configFile, config);
            var proc = RunTarget(_configFile);
            UdpSenderStuff srv = new(59140, _delim);
            srv.Run(ts);
        }

        /// <summary>
        /// Test tcp in command/response mode.
        /// </summary>
        void DoTcpDebugger(CancellationTokenSource ts)
        {
            Console.WriteLine($"DoTcpDebugger()");
            // Runs forever.
            TcpServerStuff srv = new(59120, _delim);
            srv.Run(ts);
        }

        /// <summary>
        /// Test udp in continuous mode.
        /// </summary>
        void DoUdpDebugger(CancellationTokenSource ts)
        {
            Console.WriteLine($"DoUdpDebugger()");
            // Always do once.
            UdpSenderStuff srv = new(59140, _delim);
            srv.Run(ts);
        }

        /// <summary>
        /// Run the exe with full user cli.
        /// </summary>
        /// <param name="args"></param>
        Process RunTarget(string args, bool capture = false)
        {
            ProcessStartInfo pinfo = new(_ntermExe, args)
            {
                UseShellExecute = !capture,
                RedirectStandardOutput = capture,
                RedirectStandardError = capture,
            };

            using Process proc = new() { StartInfo = pinfo };

            Console.WriteLine("Start process...");
            proc.Start();

            // if (capture)
            // {
            //     // TIL: To avoid deadlocks, always read the output stream first and then wait.
            //     var stdout = proc.StandardOutput.ReadToEnd();
            //     var stderr = proc.StandardError.ReadToEnd();
            // }

            //Console.WriteLine("Wait for exit...");
            //proc.WaitForExit();
            //Console.WriteLine("Exited...");

            // if (capture)
            // {
            //     return new(proc.ExitCode, stdout, stderr);
            // }

            return proc;
        }
    }
}
