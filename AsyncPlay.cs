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

// TODO clean up

namespace NLab
{
    #region Async background worker - practical
    interface IComm : IDisposable
    {
        // change:
        //void Run(CancellationToken token);
        Task Run(string name, CancellationToken token, IProgress<string> progress);
        // ok:
        void Send(byte[] msg);
        // remove:
        //object? GetReceive();
        //void Reset();
    }


    ///// A useable comm task. /////
    class TcpComm : IComm
    {
        readonly ConcurrentQueue<byte[]> _qSend = new();

        /// <summary>Constructor.</summary>
        /// <param name="config"></param>
        public TcpComm(List<string> config)
        {
            // process config
        }

        /// <summary>Clean up.</summary>
        public void Dispose()
        {
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

            while (!done)
            {
                token.ThrowIfCancellationRequested();

                // do work
                await Task.Delay(500, token);
                progress.Report($"TcpComm iter{_index++}");

                // send?
                if (_qSend.TryDequeue(out byte[]? msg))
                {
                    // send the message
                    progress.Report($"TcpComm send {msg}");
                }

                if (_index >= 3)
                {
                    // Normal-ish exit.
                    //progress.Report($"DONE");
                    done = true;

                    // Fake error occurred.
                    //progress.Report($"DoKeyboard blowing up...");
                    //throw new LabException("DoKeyboard Requested to fail.");
                }
            }

            //while (!token.IsCancellationRequested)
            //{
            //    token.ThrowIfCancellationRequested(); // this??

            //    // send?
            //    if (_qSend.TryDequeue(out byte[]? msg))
            //    {
            //        // send the message
            //    }

            //    // Fake receive.
            //    progress.Report($"TcpComm iter{_index++}");
            //    await Task.Delay(500, token);
            //}
        }
    }


    ///// host impl /////
    public class MyHost // -> partial class MainForm or App.Run()
    {
        readonly CancellationTokenSource _cts = new();

        public void Cancel()
        {
//            _cts.Cancel();
        }



        public async Task DoTheWork()
        {
            try
            {
                //using CancellationTokenSource ctsComm = new();
                //using CancellationTokenSource ctsKbd = new();

                var _comm = new TcpComm([]);
                //var token = _cts.Token;

                ///// Hook up progress reporting. /////
                var rxHandler = new Progress<string>(value =>
                {
                    Console.WriteLine($"RX:{value}");
                });

                var kbdHandler = new Progress<string>(value =>
                {
                    Console.WriteLine($"KB:{value}");
                    if (value == "DONE")
                    {
                        Cancel();
                        //ctsComm.Cancel();
                        //ctsKbd.Cancel();
                    }
                });

                // Fire off multiple long-running async background operations
                //using Task taskKeyboard = DoKeyboard(ctsComm.Token, kbdHandler);
                //using Task taskComm = _comm.Run("booga", ctsKbd.Token, rxHandler);
                using Task taskKeyboard = DoKeyboard(_cts.Token, kbdHandler);
                using Task taskComm = _comm.Run("booga", _cts.Token, rxHandler);

                // Do one of these:
                // 1) Direct user console.
                //WriteLine("Press any key to stop the background operations...");
                //Console.ReadKey();

                // 2) Wait for all loops to wrap up cleanly.
                //Console.WriteLine("MyHost Wait for all loops to wrap up cleanly");
                //await Task.WhenAll(taskKeyboard, taskComm);
                //Console.WriteLine("MyHost All threads/tasks cleanly stopped.");

                Console.WriteLine("MyHost Wait for any loops to wrap up cleanly");
                await Task.WhenAny(taskKeyboard, taskComm);
                Console.WriteLine("MyHost Any threads/tasks cleanly stopped.");

                // 3) Handle Ctrl+C gracefully.
                //Console.CancelKeyPress += (s, e) =>
                //{
                //    e.Cancel = true;
                //    _cts.Cancel();
                //};

                // 4) Explicit.
                //Cancel();
            }
            //catch (ConfigException ex) // known ini error
            //catch (IniSyntaxException ex) // known ini error
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
            }
            finally
            {
                Console.WriteLine($"MyHost _cts.Dispose()");
//                _cts.Dispose();
            }
        }

        ///// A local task. TODO make like comm? /////
        public async Task DoKeyboard(CancellationToken token, IProgress<string> progress)
        {
            // store/init vars
            int _index = 0;
            bool done = false;

            // while (!token.IsCancellationRequested)
            while (!done)
            {
                token.ThrowIfCancellationRequested();

                // do work
                await Task.Delay(1000, token);
                progress.Report($"DoKeyboard iter{_index++}");

                if (_index >= 3)
                {
                    // Normal-ish exit.
                    //progress.Report($"DONE");
                    done = true;

                    // Fake error occurred.
                    //progress.Report($"DoKeyboard blowing up...");
                    //throw new LabException("DoKeyboard Requested to fail.");
                }
            }




#if _OPTIONS
            try
            {
                while (_index >= 0)
                {
                    token.ThrowIfCancellationRequested(); // this??

                    // do work
                    await Task.Delay(1000, token);
                    progress.Report($"DoKeyboard iter{_index++}");

                    if (_index >= 3)
                    {
                        // Normal exit.
                        //progress.Report($"DONE");

                        // Fake error occurred.
                        progress.Report($"DoKeyboard blowing up...");
                        throw new OperationCanceledException("DoKeyboard Requested to fail.");
                        //throw new InvalidOperationException("DoKeyboard Requested to fail.");
                    }
                }

                // original
                // for (int i = 0; i < 100; i++)
                // {
                //     // 1. Check if the user requested a cancellation
                //     cancellationToken.ThrowIfCancellationRequested();

                //     // 2. Perform a chunk of heavy work
                //     await Task.Delay(50, cancellationToken); 
                //     Console.WriteLine($"Processing step {i}...");
                // }
            }

            //catch (OperationCanceledException)
            //{
            //    // 3. Gracefully handle the cancellation
            //    Console.WriteLine("DoKeyboard The operation was safely canceled.");
            //    throw; // Re-throw if you want the caller to know it was canceled
            //}

            catch (Exception ex)
            {
                // 3. Gracefully handle the cancellation
                Console.WriteLine($"DoKeyboard exception {ex.Message}");
                throw; // Re-throw if you want the caller to know it was canceled
            }
#endif
        }
    }
    #endregion

    #region Helpers
    /// <summary>Simulate synchronous real-world/time work. For test purposes only.
    class SyncTimeEater
    {
        public SyncTimeEater(int msec)
        {
            var start = Msec();
            while (Msec() < start + msec) { }
        }

        public static int Msec()
        {
            return (int)(1000 * (Stopwatch.GetTimestamp()) / Stopwatch.Frequency);
        }

    }

    // General purpose target class for tests.
    class Worker(int id)
    {
        public string Name { get { return $"Worker{_id}"; } }

        readonly int _id = id;

        public Task DoWorkAsync(string data)
        {
            Console.WriteLine($"enter [{data}]");
            // Task.Run() runs sync code asynchronously.
            var t = Task.Run(() => DoWorkSync(data));
            Console.WriteLine($"exit");
            return t;
        }

        // sync do work
        public void DoWorkSync(string data)
        {
            Console.WriteLine($"enter [{data}]");
            new SyncTimeEater(100 * _id);
            Console.WriteLine($"exit");
        }
    }
    #endregion

}