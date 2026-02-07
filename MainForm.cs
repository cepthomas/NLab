using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
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
using Ephemera.NBagOfTricks;
using Ephemera.NBagOfUis;
using static NLab.Utils;
using static NLab.TRACER;


// lots from - https://markheath.net/post/starting-threads-in-dotnet


namespace NLab
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            BtnAsync.Click += AsyncClick;
            BtnTasks.Click += TasksClick;
            BtnScoper.Click += ScoperClick;

            // Init output.
            Dictionary<string, Color> colors = new()
            {
                { "ERR", Color.Red },
                { "DBG", Color.Cyan }
            };
            tvOutput.MatchText = colors;
            Output = tvOutput;
        }

        async void AsyncClick(object? sender, EventArgs e)
        {
            await DoAsync();
        }

        void TasksClick(object? sender, EventArgs e)
        {
            DoTasks();
        }

        void ScoperClick(object? sender, EventArgs e)
        {
            TLOG_INIT(Console.WriteLine);
            //TLOG_INIT(tvOutput.AppendLine);

            DoScoperTest(12.34, new(50, 60, 70, 80));
        }


        #region ================== Full async/await =====================

        // 0000 1 INF MainForm.cs(59)  <Do1Async> enter
        // 0006 1 INF MainForm.cs(83)  <LongRunningOperationAsync> enter
        // 0310 1 INF MainForm.cs(67)  <Do1Async> 100
        // 0311 1 INF MainForm.cs(99)  <AwaitableBackgroundTask> enter
        // 0311 1 INF MainForm.cs(107) <AwaitableBackgroundTask> 100
        // 0318 1 INF MainForm.cs(112) <AwaitableBackgroundTask> exit [16]
        // 0318 1 INF MainForm.cs(71)  <Do1Async> 200
        // 0643 1 INF MainForm.cs(77)  <Do1Async> exit [Sound Code - Mark He]
        // 1014 1 INF MainForm.cs(87)  <LongRunningOperationAsync> exit

        async Task DoAsync()
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
        }

        // A long-running operation that returns an int.
        async Task<int> LongRunningOperationAsync()
        {
            Tell(INF, $"enter");

            await Task.Delay(1000); // 1 second delay

            Tell(INF, $"exit");

            return 999;
        }

        //One of the easier methods is to use Task.Run, very similar to your existing code.
        //However, I do not recommend implementing a CalculateAsync method since that implies the
        //processing is asynchronous (which it is not). Instead, use Task.Run at the point of the call.
        //was async Task AwaitableBackgroundTask(string state)
        //And if from your UI thread you want to offload work to a worker thread, and you use Task.Run to do so,
        //you often typically want to do some work back on the UI thread once that background work is done,
        //and these language features make that kind of coordination easy and seamless.
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
        #endregion


        #region ================== Tasks no async/await =====================

        // 0000 1 INF MainForm.cs(166) <DoWorkAsync> enter [some data for Worker1]
        // 0011 1 INF MainForm.cs(168) <DoWorkAsync> exit
        // 0011 7 INF MainForm.cs(175) <DoWork> enter [some data for Worker1]
        // 0011 1 INF MainForm.cs(166) <DoWorkAsync> enter [some data for Worker2]
        // 0011 1 INF MainForm.cs(168) <DoWorkAsync> exit
        // 0011 11 INF MainForm.cs(175) <DoWork> enter [some data for Worker2]
        // 0012 1 INF MainForm.cs(166) <DoWorkAsync> enter [some data for Worker3]
        // 0012 1 INF MainForm.cs(168) <DoWorkAsync> exit
        // 0012 5 INF MainForm.cs(175) <DoWork> enter [some data for Worker3]
        // 0012 1 INF MainForm.cs(195) <DoTasks> Waiting
        // 0111 7 INF MainForm.cs(177) <DoWork> exit
        // 0212 11 INF MainForm.cs(177) <DoWork> exit
        // 0312 5 INF MainForm.cs(177) <DoWork> exit
        // 0314 5 INF MainForm.cs(185) <DoTasks> Everything done

        // You're looking for Task.WhenAll. Create a bunch of Tasks that do what you want them to,
        // then wait on all of the tasks and ContinueWith your callback. I split out an async version of
        // the DoWork method - if you're always going to be calling it asynchronously you don't necessarily need to do that.

        void DoTasks()
        {
            void Callback()
            {
                Tell(INF, "Callback()");
            }

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
        #endregion


        #region ================== Scoper etc =====================

        int DoScoperTest(double dval, Rectangle rect)
        {
        //    TLOG_CONTEXT();

            // raw style
            using var sc = new Scoper();

            // Check args.
            sc.TLOG_EQUAL(dval, 6.7);
            sc.TLOG_EQUAL(rect.Height, 999);

            //using var sc = new Scoper();

            var m1res = TestMethod1("here-we-go", new(10101));

            var m2res = TestMethod1("try-again", new(20202));

            var res = m2res - m1res;
            sc.TLOG_EQUAL(res, 543);

            sc.TLOG_INFO($">>> Leaving");

            // What happened.
            //Tell(INF, $"Captures:");
            //Scoper.Captures.ForEach(c => Tell(INF, $"    [{c}]"));

            return res;
        }


        //TODO Use attribute? https://medium.com/@nwonahr/creating-and-using-custom-attributes-in-c-for-asp-net-core-c4f7d3db1829
        [LogMethodExecution("testing level 1")]
        int TestMethod1(string s, Worker w)
        {
            //using var sc = new Scoper($"Scoper for [{s}]");
            //using var sc = new Scoper();
            //TLOG_CONTEXT();

            // raw style
            using var sc = new Scoper();

            //var cm = System.Reflection.MethodBase.GetCurrentMethod();


            sc.TLOG_INFO($"entry s:{s} w:{w.Name}");

            // do something
            s = new string(s.Reverse().ToArray());

            sc.TLOG_INFO($"exit s:{s}");

            return s.Length;
        }

        //[MethodImpl(MethodImplOptions.NoInlining)]
        //public static MethodBase? GetMyCaller()
        //{
        //    var st = new StackTrace(new StackFrame(1));
        //    var cm = st.GetFrame(0).GetMethod();
        //    return cm;
        //}


        //[MethodImpl(MethodImplOptions.NoInlining)]
        //public static string GetMyMethodName()
        //{
        //    var st = new StackTrace(new StackFrame(1));
        //    return st.GetFrame(0).GetMethod().Name;
        //}

        #endregion
    }

    public class Worker(int id)
    {
        public string Name { get { return $"Worker{_id}"; } }

        int _id = id;

        public Task DoWorkAsync(string data)
        {
            Tell(INF, $"enter [{data}]");
            var t = Task.Run(() => DoWork(data));
            Tell(INF, $"exit");
            return t;
        }

        // sync do work
        public void DoWork(string data)
        {
            Tell(INF, $"enter [{data}]");
            SyncTimeEater(100 * _id);
            Tell(INF, $"exit");
        }
    }


    #region ================== Various socket async TODO =====================

    class AsyncSocket
    {
        //  https://stackoverflow.com/a/53403824   c# 7.0 in a nutshell
        const int packet_length = 2;  // user defined packet length

        void DoAsync()
        {
            //// Tweak config.
            //var config = BuildConfig("tcp 127.0.0.1 59120");
            //File.WriteAllLines(cfile, config);
            //RunServerAsync();
            //Go(cfile);
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


    /*  Various socket async
    /////////////////////////////////////////////////////////////
    var listener = new TcpListener(System.Net.IPAddress.Any, 55555);
    listener.Start();
    var cts = new CancellationTokenSource(400);
    var sw = Stopwatch.StartNew();

    try
    {
        await listener.AcceptTcpClientAsync(cts.Token);
    }
    catch(Exception e)
    {
        Console.WriteLine(e);
    }
    finally
    {
        sw.Stop();
        Console.WriteLine(sw.Elapsed);
    }
    listener.Stop();


    //////////////////////////////////////////////////////////////////
    // well, dunno. maybe the code u where u await is already in an unawaited task
    // this for example doesnt make VS pause execution

    _ = Task.Run(async () =>  // why: https://stackoverflow.com/a/22645114
    {
        var listener = new TcpListener(System.Net.IPAddress.Any, 55555);
        listener.Start();

        var cts = new CancellationTokenSource(400);
        var sw = Stopwatch.StartNew();
        await listener.AcceptTcpClientAsync(cts.Token);
        sw.Stop();
        Console.WriteLine(sw.Elapsed);

        listener.Stop();
    });


    //using await Task.Run(...); instead makes it pause




    //////////////////////////////////////////////////////////////////////
    //  https://stackoverflow.com/a/53403824   c# 7.0 in a nutshell


    const int packet_length = 2;  // user defined packet length

    async void RunServerAsync()
    {
        var listner = new TcpListener(IPAddress.Any, 9999);
        listner.Start();
        try
        {
            while (true)
            {
                TcpClient client = await listner.AcceptTcpClientAsync();

                await Accept(client);


                // await Accept(await listner.AcceptTcpClientAsync());
            }
        }
        finally { listner.Stop(); }
    }



    async Task Accept(TcpClient client)
    {
        await Task.Yield(); 
        try
        {
            using(client)
            using(NetworkStream n = client.GetStream())
            {
                byte[] data = new byte[packet_length];
                int bytesRead = 0;
                int chunkSize = 1;

                while (bytesRead < data.Length && chunkSize > 0)
                    bytesRead += chunkSize = 
                        await n.ReadAsync(data, bytesRead, data.Length - bytesRead);

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
        catch(Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    ////////////////////////////////////////////////////////////////////////
    //https://gist.github.com/Maxwe11/cf8cc6331ad73671846e
    ////////////////////////////////////////////////////////////////////////

    public class Program_not
    {
        public static void Main()
        {
            MainAsync().GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {
            Console.WriteLine("Starting...");
            var server = new TcpListener(IPAddress.Parse("0.0.0.0"), 66);
            server.Start();
            Console.WriteLine("Started.");
            while (true)
            {
                var client = await server.AcceptTcpClientAsync().ConfigureAwait(false);
                var cw = new ClientWorking(client, true);
                Task.Run((Func<Task>)cw.DoSomethingWithClientAsync);
            }
        }
    }

    class ClientWorking
    {
        TcpClient _client;
        bool _ownsClient;

        public ClientWorking(TcpClient client, bool ownsClient)
        {
            _client = client;
            _ownsClient = ownsClient;
        }

        public async Task DoSomethingWithClientAsync()
        {
            try
            {
                using (var stream = _client.GetStream())
                {
                    using (var sr = new StreamReader(stream))
                    using (var sw = new StreamWriter(stream))
                    {
                        await sw.WriteLineAsync("Hi. This is x2 TCP/IP easy-to-use server").ConfigureAwait(false);
                        await sw.FlushAsync().ConfigureAwait(false);
                        var data = default(string);
                        while (!((data = await sr.ReadLineAsync().ConfigureAwait(false)).Equals("exit", StringComparison.OrdinalIgnoreCase)))
                        {
                            await sw.WriteLineAsync(data).ConfigureAwait(false);
                            await sw.FlushAsync().ConfigureAwait(false);
                        }
                    }

                }
            }
            finally
            {
                if (_ownsClient && _client != null)
                {
                    (_client as IDisposable).Dispose();
                    _client = null;
                }
            }
        }
    }
    */

    #endregion
}
