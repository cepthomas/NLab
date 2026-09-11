using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using NAudio.Wave;
using Ephemera.NBagOfTricks;
using Ephemera.NBagOfUis;
using W32 = Ephemera.Win32.Internals;
using WM = Ephemera.Win32.WindowManagement;
using System.Collections.Concurrent;


namespace NLab
{
    ///// A useable comm task. /////
    class MyComm
    {
        readonly ConcurrentQueue<byte[]> _qSend = new();

        /// <summary>Constructor.</summary>
        /// <param name="config"></param>
        public MyComm(List<string> config)
        {
            // process config
        }

        ///// IComm implementation. /////
        public void Send(byte[] req)
        {
            _qSend.Enqueue(req);
        }

        public async Task Run(string name, CancellationToken token, IProgress<string> progress)
        {
            // store/init vars
            int _index = 0;
            bool done = false;

            while (!done) // !token.IsCancellationRequested
            {
                token.ThrowIfCancellationRequested();

                // do work
                await Task.Delay(500, token);
                progress.Report($"MyComm iter{_index++}");

                // send?
                if (_qSend.TryDequeue(out byte[]? msg))
                {
                    // send the message
                    progress.Report($"MyComm send {msg}");
                }

                if (_index >= 3)
                {
                    // Normal-ish exit.
                    //done = true;

                    // Fake error occurred.
                    progress.Report($"MyComm Requested to fail.");
                    throw new LabException("MyComm throw Requested to fail.");
                }
            }

            // Tasks could handle these locally then re-throw.
            // try...
            // catch (OperationCanceledException)
            // catch (Exception ex)
        }
    }

    ///// Infrastructure task. /////
    class KbdReader
    {
        public async Task Run(CancellationToken token, IProgress<string> progress)
        {
            // store/init vars
            int _index = 0;
            bool done = false;

            while (!done) // !token.IsCancellationRequested
            {
                token.ThrowIfCancellationRequested();

                // do work
                await Task.Delay(1000, token);
                progress.Report($"DoKeyboard iter{_index++}");

                if (_index >= 3)
                {
                    // Normal-ish exit.
                    //done = true;

                    // Fake error occurred.
                    progress.Report($"DoKeyboard Requested to fail.");
                    throw new LabException("DoKeyboard throw Requested to fail.");
                }
            }
        }
    }

    ///// Host impl /////
    public class MyHost // -> partial class MainForm or App.Run()
    {
        public async Task DoTheWork(CancellationToken token)
        {
            try
            {
                // Create tasks.
                var _comm = new MyComm([]);
                var _kbd = new KbdReader();

                // Hook up progress reporting.
                var rxHandler = new Progress<string>(value => { Console.WriteLine($"RX:{value}"); });
                var kbdHandler = new Progress<string>(value => { Console.WriteLine($"KB:{value}"); });

                // Fire off multiple long-running async background operations
                // TIL: Don't call explicit Dispose() on tasks. That includes using ... statements.
                // https://devblogs.microsoft.com/dotnet/do-i-need-to-dispose-of-tasks/
                Task taskKeyboard = _kbd.Run(token, kbdHandler);
                Task taskComm = _comm.Run("booga", token, rxHandler);

                // These are forever tasks. If any stops it indicates normal shutdown or an error.
                Console.WriteLine("MyHost Start wait for any loop to complete");
                await Task.WhenAny(taskKeyboard, taskComm);
                Console.WriteLine($"MyHost Some but not necessary all tasks completed kbd:{taskKeyboard.Status} comm:{taskComm.Status}");

                // Check for task errors (and/or Status?) and do something with them.
                if (taskKeyboard.Exception is not null)
                {
                    Console.WriteLine(taskKeyboard.Exception.InnerException.Message);
                }
                if (taskComm.Exception is not null)
                {
                    Console.WriteLine(taskComm.Exception.InnerException.Message);
                }
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine($"MyHost Normal TaskCanceledException [{ex.Message}]");
            }
            catch (OperationCanceledException ex)
            {
                Console.WriteLine($"MyHost Normal?? OperationCanceledException [{ex.Message}]");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MyHost other exception {ex.GetType().Name} [{ex.Message}]");
                Console.WriteLine(string.Join(Environment.NewLine, Leftovers.DumpStack("    ")));                
            }
        }
    }
}