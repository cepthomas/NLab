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

        void Send(byte[] msg);

        // remove:
        //object? GetReceive();
        //void Reset();
    }


    ///// A useable task. /////
    class TcpComm : IComm
    {
        readonly ConcurrentQueue<byte[]> _qSend = new();

        /// <summary>Constructor.</summary>
        /// <param name="config"></param>
        /// <exception cref="ConfigException"></exception>
        public TcpComm(List<string> config)
        {
            // process config
        }

        /// <summary>Clean up.</summary>
        public void Dispose()
        {
        }

        public async Task Run(string name, CancellationToken token, IProgress<string> progress)
        {
            // store/init vars
            int _index = 0;
            //token.ThrowIfCancellationRequested(); // this??

            while (!token.IsCancellationRequested)
            {
                // send?
                if (_qSend.TryDequeue(out byte[]? msg))
                {
                    // send the message
                }

                // Fake receive.
                progress.Report($"iter{_index++}");
                await Task.Delay(500, token);
            }
        }

        ///// IComm implementation. /////
        public void Send(byte[] req)
        {
            _qSend.Enqueue(req);
        }
    }

    
    ///// host impl /////
    public class MyHost // -> partial class MainForm or App.Run()
    {
        readonly CancellationTokenSource _cts = new();

        public void Cancel()
        {
            _cts?.Cancel();
        }

        public async Task DoAsync()
        {
            try
            {
                var _comm = new TcpComm([]);
                var token = _cts.Token;

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
                    }
                });

                // Fire off multiple long-running async background operations
                using Task taskKeyboard = DoKeyboard(token, kbdHandler);
                using Task taskComm = _comm.Run("booga", token, rxHandler);

                // Do one of these:
                // 1) Direct user console.
                //WriteLine("Press any key to stop the background operations...");
                //Console.ReadKey();

                // 2) Wait for all loops to wrap up cleanly.
                await Task.WhenAll(taskKeyboard, taskComm);
                //Console.WriteLine("All threads/tasks cleanly stopped.");

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
            catch (Exception ex)
            {
                Console.WriteLine($"MyHost other exception {ex.GetType().Name} [{ex.Message}]");
            }
        }

        ///// A local task. /////
        public async Task DoKeyboard(CancellationToken token, IProgress<string> progress)
        {
            // store/init vars
            int _index = 0;
            //token.ThrowIfCancellationRequested(); // this??

            while (!token.IsCancellationRequested)
            {
                // do work
                await Task.Delay(1000, token);
                progress.Report($"iter{_index++}");

                // Fake exit.
                if (_index >= 3)
                {
                    progress.Report($"DONE");
                }
            }
        }
    }
    #endregion

    #region Async background worker - my first stab
    public class BgwHost
    {
        readonly CancellationTokenSource _cts = new();
        int _loopCount = 0;

        void Cancel()
        {
            _cts?.Cancel();
        }

        public async Task Run(int count = 0) // was Main()
        {
            try
            {
                _loopCount = count;

                //using var cts = new CancellationTokenSource();
                var token = _cts.Token;

                // Hook up progress reporting.
                var progressHandler = new Progress<string>(value => { Console.WriteLine(value); });
                var progress = progressHandler as IProgress<string>;

                // Fire off multiple long-running async background operations
                Task task1 = BackgroundWorkerAsync("Consumer-A", _cts.Token, progress);
                Task task2 = BackgroundWorkerAsync("Consumer-B", _cts.Token, progress);

                // Do one of these:
                // 1) Direct user console.
                //WriteLine("Press any key to stop the background operations...");
                //Console.ReadKey();

                // 2) Wait for all loops to wrap up cleanly.
                await Task.WhenAll(task1, task2);
                Console.WriteLine("All threads/tasks cleanly stopped.");

                // 3) Handle Ctrl+C gracefully.
                //Console.CancelKeyPress += (s, e) =>
                //{
                //    e.Cancel = true;
                //    _cts.Cancel();
                //};

                // 4) Explicit.
                //Cancel();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Main Task {ex.GetType().Name}: {ex.Message}");
            }
        }

        async Task BackgroundWorkerAsync(string name, CancellationToken token, IProgress<string> progress)
        {
            // Simulate a FOREVER I/O bound wait (polling a queue, listening to API)
            while (!token.IsCancellationRequested)
            {
                try
                {
                    // Do the task work here.
                    token.ThrowIfCancellationRequested(); // this??

                    // Use Task.Delay, NEVER Thread.Sleep inside an async method
                    // await Task.Delay: Unlike Thread.Sleep(), this releases the thread back to the thread pool during the wait time.
                    await Task.Delay(500, token);

                    if (_loopCount > 0)
                    {
                        progress.Report($"[{name}] processed batch {_loopCount} at {DateTime.Now:HH:mm:ss}");
                        _loopCount--;
                    }
                    else if (_loopCount == 0)
                    {
                        _loopCount = -999;
                        progress.Report($"[{name}] Requested to fail at {DateTime.Now:HH:mm:ss}");
                        throw new InvalidOperationException("Requested to fail.");
                    }
                }
                catch (OperationCanceledException)
                {
                    // This is expected when token.IsCancellationRequested triggers
                    progress.Report($"[{name}] Requested to fail at {DateTime.Now:HH:mm:ss}");
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"BackgroundWorkerAsync {ex.GetType().Name}: {ex.Message}");
                }
            }
        }
    }
    #endregion

    #region Async example from google AI
    public class AsyncTcpClient
    {
        const string ServerIp = "127.0.0.1";
        const int ServerPort = 13000;

        //public async Task Main_gogogo(string[] args)
        public async Task GoGo() // was Main(string[] args)
        {
            using var cts = new CancellationTokenSource();

            // Handle Ctrl+C gracefully
            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };

            try
            {
                await RunClientAsync(ServerIp, ServerPort, cts.Token);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("\nClient shutdown initiated by user.");
            }
        }

        async Task RunClientAsync(string ip, int port, CancellationToken cancellationToken)
        {
            // 1. Instantiate and connect asynchronously
            using TcpClient client = new();
            Console.WriteLine($"Connecting to {ip}:{port}...");
            await client.ConnectAsync(ip, port, cancellationToken);
            Console.WriteLine("Connected to server!");

            // 2. Get the communication stream
            using NetworkStream stream = client.GetStream();

            // 3. Start a background task to continuously read server messages
            Task receiveTask = ReceiveMessagesAsync(stream, cancellationToken);

            // 4. Main loop for sending data from console input
            Console.WriteLine("Type messages and press Enter to send (or 'exit' to quit):");
            while (!cancellationToken.IsCancellationRequested)
            {
                string? message = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(message)) continue;
                if (message.Equals("exit", StringComparison.OrdinalIgnoreCase)) break;

                // Convert string to bytes and send
                byte[] data = Encoding.UTF8.GetBytes(message);
                await stream.WriteAsync(data, 0, data.Length, cancellationToken);
            }

            // Clean up connection
            client.Close();
        }

        async Task ReceiveMessagesAsync(NetworkStream stream, CancellationToken cancellationToken)
        {
            byte[] buffer = new byte[1024];

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    // Read incoming bytes asynchronously
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);

                    // If ReadAsync returns 0, the server closed the connection gracefully
                    if (bytesRead == 0)
                    {
                        Console.WriteLine("\nServer disconnected.");
                        break;
                    }

                    // Decode and print the message
                    string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"\n[Server]: {response}");
                }
            }
            catch (Exception ex) when (ex is ObjectDisposedException || ex is IOException)
            {
                // Expected exceptions when the connection drops or is closed intentionally
                Console.WriteLine("\nConnection lost.");
            }
        }
    }
    #endregion

    #region Cleary - Long-running processes - not forever
    // https://blog.stephencleary.com/2013/05/taskrun-vs-backgroundworker-round-1.html
    // I’ll leave you with a “combined” example. The code below starts a cancelable background
    // operation that reports progress, and will either throw an exception or return a value.
    // These are all the basic operations of BackgroundWorker. One of these uses BackgroundWorker
    // and the other uses Task.Run. Don’t just look at the length of the code; consider all the
    // little nuances of how it works (type safety, how easily the API can be misused, etc).
    // Then ask yourself: which code would I rather maintain?

    // NB!!! - These are long-running but not forever, there's a significant difference for the app level.

    // other maybe:
    // https://grantwinney.com/convert-backgroundworker-to-task-with-taskcompletionsource/
    // https://docs.lextudio.com/blog/how-to-replace-backgroundworker-with-async-await-and-tasks-80d7c8ed89dc

    //await Task.Delay: Unlike Thread.Sleep(), this releases the thread back to the thread pool during the wait time.
    //CancellationToken: Provides a safe mechanism to cleanly shut down the loop when your application stops.

    class ExampleTask // from Cleary example
    {
        readonly CancellationTokenSource _cts = new();

        async void Go() // ==> was Main(string[] args)
        {
            var fail = true; // false
            var token = _cts.Token;

            var progressHandler = new Progress<string>(value => { Console.WriteLine(value); });
            var progress = progressHandler as IProgress<string>;

            try
            {
                var result = await Task.Run(() =>
                {
                    for (int i = 0; i != 100; ++i)
                    {
                        progress?.Report(i + "%");
                        token.ThrowIfCancellationRequested();
                        Thread.Sleep(100);
                    }

                    if (fail)
                    {
                        throw new InvalidOperationException("Requested to fail.");
                    }

                    return 13;
                });

                Console.WriteLine("Completed: " + result);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Cancelled.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.GetType().Name + ": " + ex.Message);
            }
        }

        void Cancel()
        {
            _cts?.Cancel();
        }
    }

    // Corresponding OG bgw.
    public class BackgroundWorker
    {
        readonly CancellationTokenSource _cts = new();

        public void Start()
        {
            // Fire-and-forget the infinite background task
            Task.Run(() => DoWorkAsync(_cts.Token));
        }

        private async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // 1. Do your work here
                    Console.WriteLine("Processing data...");

                    // 2. Pause efficiently without blocking the thread pool
                    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    // Clean exit when cancelled
                    break; 
                }
                catch (Exception ex)
                {
                    // Prevent unhandled exceptions from crashing the loop
                    Console.WriteLine($"Error occurred: {ex.Message}");
                }
            }
            Console.WriteLine("Loop safely stopped.");
        }

        public void Stop()
        {
            _cts.Cancel(); // Triggers the cancellation token
        }
    }

    class Reporter
    {
        /// <summary>
        /// A progress implementation that sends progress reports to an observer stream.
        /// Optionally ends the stream when the task completes.
        /// https://gist.github.com/StephenCleary/7330384
        /// </summary>
        /// <typeparam name="T">The type of progress value.</typeparam>
        internal sealed class ObserverProgress<T> : IProgress<T>
        {
            /// <summary>
            /// The observer to pass progress reports to.
            /// </summary>
            readonly IObserver<T> _observer;

            /// <summary>
            /// Initializes a new instance of the <see cref="ObserverProgress&lt;T&gt;"/> class.
            /// </summary>
            /// <param name="observer">The observer to pass progress reports to. May not be <c>null</c>.</param>
            public ObserverProgress(IObserver<T> observer)
            {
                _observer = observer;
            }

            void IProgress<T>.Report(T value)
            {
                _observer.OnNext(value);
            }

            /// <summary>
            /// Watches the task, and completes the observer (via <see cref="IObserver{T}.OnError"/> or <see cref="IObserver{T}.OnCompleted"/>) when the task completes.
            /// </summary>
            /// <param name="task">The task to watch. May not be <c>null</c>.</param>
            public void ObserveTaskForCompletion(Task task)
            {
                task.ContinueWith(_ =>
                {
                    if (task.IsFaulted)
                    {
                        _observer.OnError(task.Exception.InnerException!);
                    }
                    else
                    {
                        _observer.OnCompleted();
                    }
                }, TaskScheduler.Default);
            }
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

    #region TODO My old crap - mostly useless
    class OtherStuff
    {
        readonly CancellationTokenSource _cts = new();

        void Ex1()
        {
            var task = Task.Run(async () =>  // <- marked async
            {
                while (true)
                {
                    //DoWork();
                    await Task.Delay(500, _cts.Token); // <- await with cancellation
                }
            }, _cts.Token);
        }

        void EEx2()
        {
            Task t = Task.Run(async () =>
            {
                while (true)
                {
                    _cts.Token.ThrowIfCancellationRequested(); // not long-running
                    try
                    {
                        Console.WriteLine("Running..."); // not long-running
                        await Task.Delay(500, _cts.Token); // not executed by the thread pool
                    }
                    catch (TaskCanceledException) { }
                }
            });
        }
    }

    public class AsyncPlay
    {
        async void AsyncClick(object? sender, EventArgs e)
        {
            //Reset();
            var x = new AsyncAwait();
            var res = await x.Go();
            Console.WriteLine($"res:{res}");
        }

        void TasksClick(object? sender, EventArgs e)
        {
            //Reset();
            var x = new TaskWithoutAsync();
            x.Go();
        }
    }

    class AsyncAwait
    {
        public async Task<int> Go()
        {
            string state = "Async_Await";

            Console.WriteLine($"enter");

            var lroa_result = LongRunningOperationAsync();

            // task independent stuff here
            new SyncTimeEater(300);

            Console.WriteLine($"100");

            await AwaitableBackgroundTask(state);

            Console.WriteLine($"200");

            // execute sync function as async
            var xdoc = new XmlDocument();
            await Task.Run(() => xdoc.Load("http://feeds.feedburner.com/soundcode"));

            Console.WriteLine($"exit [{xdoc.ChildNodes[1].InnerText.Left(32)}]");

            return 909;
        }

        // A long-running async operation that returns an int.
        async Task<int> LongRunningOperationAsync()
        {
            Console.WriteLine($"enter");

            await Task.Delay(1000);

            Console.WriteLine($"exit");

            return 999;
        }

        // async version of bgw.
        async Task AwaitableBackgroundTask(string state)
        {
            Console.WriteLine($"enter");

            int i = 5;
            var task = Task.Run(() => { return SyncFunction(state); });

            // a synchronous function - runs in new thread
            int SyncFunction(string s)
            {
                Console.WriteLine($"enter SyncFunction");
                return s.Length + i;
            }

            Console.WriteLine($"100");

            // run calculate as async - returns int answer
            var myOutput = await task;

            Console.WriteLine($"exit [{myOutput}]");
        }
    }

    class TaskWithoutAsync
    {
        public void Go()
        {
            void Callback()
            {
                Console.WriteLine("Callback()");
            }

            int id = 1;
            List<Worker> workers = [new(id++), new(id++), new(id++)];

            var tasks = workers.Select(t => t.DoWorkAsync($"some data for {t.Name}"));

            Task.WhenAll(tasks).ContinueWith(task => Callback());

            Console.WriteLine("Waiting");

            // TODO stuff like this:
            // using CancellationTokenSource ts = new();
            // using Task taskKeyboard = Task.Run(() => DoKeyboard(ts.Token));
            // using Task taskComm = Task.Run(() => _comm.Run(ts.Token));
            // ----
            // ts.Cancel();
            // Task.WaitAll([taskKeyboard, taskComm]);
        }
    }

    class OtherNotUseful
    {
        static async Task Main_not()
        {
            using var cts = new CancellationTokenSource();

            // Fire off multiple long-running async background operations
            Task task1 = RunBackgroundConsumerAsync("Consumer-A", cts.Token);
            Task task2 = RunBackgroundConsumerAsync("Consumer-B", cts.Token);

            Console.WriteLine("Press any key to stop the background operations...");
            Console.ReadKey();

            // Gracefully cancel the long-running loops
            cts.Cancel();

            // Wait for all loops to wrap up cleanly
            await Task.WhenAll(task1, task2);
            Console.WriteLine("All threads/tasks cleanly stopped.");
        }

        static async Task RunBackgroundConsumerAsync(string name, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    // Simulate an I/O bound wait (polling a queue, listening to API)
                    // Use Task.Delay, NEVER Thread.Sleep inside an async method
                    await Task.Delay(1000, token);

                    Console.WriteLine($"[{name}] processed a batch at {DateTime.Now:HH:mm:ss}");
                }
                catch (OperationCanceledException)
                {
                    // This is expected when token.IsCancellationRequested triggers
                    break;
                }
            }
        }
    }
    #endregion
}