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
using static NLab.Utils;


namespace NLab
{
    // https://blog.stephencleary.com/2013/05/taskrun-vs-backgroundworker-round-1.html
    // I’ll leave you with a “combined” example. The code below starts a cancelable background
    // operation that reports progress, and will either throw an exception or return a value.
    // These are all the basic operations of BackgroundWorker. One of these uses BackgroundWorker
    // and the other uses Task.Run. Don’t just look at the length of the code; consider all the
    // little nuances of how it works (type safety, how easily the API can be misused, etc).
    // Then ask yourself: which code would I rather maintain?

    // other maybe
    // https://grantwinney.com/convert-backgroundworker-to-task-with-taskcompletionsource/
    // https://docs.lextudio.com/blog/how-to-replace-backgroundworker-with-async-await-and-tasks-80d7c8ed89dc

    ///////////////////////////////////////////////////////////////////////////////////////
    class TopLevel
    {
        CancellationTokenSource _cts = new();

        // using Task taskComm = Task.Run(() => _comm.Run(ts.Token));


        async void Go() // ==> was Main(string[] args)
        {
            var fail = true; // false
            //_cts = new CancellationTokenSource();
            var token = _cts.Token;

            var progressHandler = new Progress<string>(value =>
            {
                Console.WriteLine(value);
            });
            var progress = progressHandler as IProgress<string>;

            string args = "aaa bbb";
            var w1 = new MyWork1(_cts, args);

            //var w1_task = w1.DoIt();
        }

        void Cancel()
        {
            _cts?.Cancel();
        }
    }

    class MyWork1
    {
        string _args;
        CancellationTokenSource _cts;

        public MyWork1(CancellationTokenSource cts, string args)
        {
            _cts = cts;
            _args = args;
        }

        public int SomeFunc()
        {
            return _args.Length;
        }

        /// <summary>Main work loop.</summary>
        /// <see cref="IComm"/>
        public async void DoIt(CancellationToken token, IProgress<string> progress)
        {
            bool fail = false;
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
                        throw new InvalidOperationException("MyWork1 Requested to fail.");
                    }

                    return 13;
                });

                Console.WriteLine("MyWork1 Completed: " + result);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("MyWork1 Cancelled.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.GetType().Name + ": " + ex.Message);
            }
        }
    }



    ///////////////////////////////////////////////////////////////////////////////////////
    class ExTask // from example
    {
        CancellationTokenSource _cts = new();

        async void Go() // ==> was Main(string[] args)
        {
            var fail = true; // false
            //_cts = new CancellationTokenSource();
            var token = _cts.Token;

            var progressHandler = new Progress<string>(value =>
            {
                Console.WriteLine(value);
            });
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



#if _XXX
public class BackgroundWorker
{
    //await Task.Delay: Unlike Thread.Sleep(), this releases the thread back to the thread pool during the wait time.
    //CancellationToken: Provides a safe mechanism to cleanly shut down the loop when your application stops.

    private CancellationTokenSource _cts = new CancellationTokenSource();

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


task = Task.Run(async () =>  // <- marked async
{
    while (true)
    {
        DoWork();
        await Task.Delay(10000, wtoken.Token); // <- await with cancellation
    }
}, wtoken.Token);


Task t = Task.Run(async () =>
{
  while (true)
  {
    cts.Token.ThrowIfCancellationRequested(); // not long-running
    try
    {
      "Running...".Dump(); // not long-running
      await Task.Delay(500, cts.Token); // not executed by the thread pool
    }
    catch (TaskCanceledException ex) { }
  }
});
#endif



    class reporter
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
                        _observer.OnError(task.Exception.InnerException);
                    }
                    else
                    {
                        _observer.OnCompleted();
                    }
                }, TaskScheduler.Default);
            }
        }
    }


    ///////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////


    public class AsyncPlay
    {
        // host:
        // BtnAsync.Click += AsyncClick;
        // BtnTasks.Click += TasksClick;

        async void AsyncClick(object? sender, EventArgs e)
        {
            Reset();
            var x = new AsyncAwait();
            var res = await x.Go();
            Tell(INF, $"res:{res}");
        }

        void TasksClick(object? sender, EventArgs e)
        {
            Reset();
            var x = new TaskWithoutAsync();
            x.Go();
        }
    }

    class AsyncAwait
    {
        public async Task<int> Go()
        {
            string state = "Async_Await";

            Tell(INF, $"enter");

            var lroa_result = LongRunningOperationAsync();

            // task independent stuff here
            SyncTimeEater(300);

            Tell(INF, $"100");

            await AwaitableBackgroundTask(state);

            Tell(INF, $"200");

            // execute sync function as async
            var xdoc = new XmlDocument();
            await Task.Run(() => xdoc.Load("http://feeds.feedburner.com/soundcode"));

            Tell(INF, $"exit [{xdoc.ChildNodes[1].InnerText.Left(32)}]");

            return 909;
        }

        // A long-running operation that returns an int.
        async Task<int> LongRunningOperationAsync()
        {
            Tell(INF, $"enter");

            await Task.Delay(1000); // 1 second delay

            Tell(INF, $"exit");

            return 999;
        }

        async Task AwaitableBackgroundTask(string state)
        {
            Tell(INF, $"enter");

            int i = 5;
            var task = Task.Run(() => { return SyncFunction(state); });

            // a synchronous function - runs in new thread
            int SyncFunction(string s)
            {
                Tell(INF, $"enter SyncFunction");
                return s.Length + i;
            }

            Tell(INF, $"100");

            // run calculate as async - returns int answer
            var myOutput = await task;

            Tell(INF, $"exit [{myOutput}]");
        }
    }

    class TaskWithoutAsync
    {
        public void Go()
        {
            void Callback() { Tell(INF, "Callback()"); }

            int id = 1;
            List<Worker> workers = [new(id++), new(id++), new(id++)];

            var tasks = workers.Select(t => t.DoWorkAsync($"some data for {t.Name}"));

            Task.WhenAll(tasks).ContinueWith(task => Callback());

            Tell(INF, "Waiting");

            // TODO stuff like this:
            // using CancellationTokenSource ts = new();
            // using Task taskKeyboard = Task.Run(() => DoKeyboard(ts.Token));
            // using Task taskComm = Task.Run(() => _comm.Run(ts.Token));
            // ----
            // ts.Cancel();
            // Task.WaitAll([taskKeyboard, taskComm]);
        }
    }

    class AsyncSocket // TODO1 dev and migrate to nterm
    {
        //  https://stackoverflow.com/a/53403824   c# 7.0 in a nutshell
        const int packet_length = 2;  // user defined packet length

        void DoAsync()
        {
            RunServerAsync();
        }

        async void RunServerAsync()
        {
            var listner = new TcpListener(IPAddress.Any, 59120);
            listner.Start();
            try
            {
                while (true)
                {
                    // was await Accept(await listner.AcceptTcpClientAsync());
                    TcpClient client = await listner.AcceptTcpClientAsync();
                    await Accept(client);
                }
            }
            finally
            {
                listner.Stop();
            }
        }

        async Task Accept(TcpClient client)
        {
            await Task.Yield();
            try
            {
                using (client)
                using (NetworkStream n = client.GetStream())
                {
                    byte[] data = new byte[packet_length];
                    int bytesRead = 0;
                    int chunkSize = 1;

                    while (bytesRead < data.Length && chunkSize > 0)
                    {
                        bytesRead += chunkSize = await n.ReadAsync(data, bytesRead, data.Length - bytesRead);
                    }

                    // get data
                    string str = Encoding.Default.GetString(data);
                    Console.WriteLine("[server] received : {0}", str);

                    // To do
                    // ...

                    // send the result to client
                    string send_str = "server_send_test";
                    byte[] send_data = Encoding.ASCII.GetBytes(send_str);
                    await n.WriteAsync(send_data, 0, send_data.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}    