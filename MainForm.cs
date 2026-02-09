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
using Ephemera.NBagOfTricks;
using Ephemera.NBagOfUis;
using static NLab.Utils;


// https://markheath.net/post/starting-threads-in-dotnet


namespace NLab
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            Move += (sender, e) => { Text = $"L:{Left} T:{Top} W:{Width} H:{Height}"; };

            BtnAsync.Click += AsyncClick;
            BtnTasks.Click += TasksClick;
            BtnTracer.Click += TracerClick;
            BtnStuff.Click += StuffClick;
        }

        async void AsyncClick(object? sender, EventArgs e)
        {
            Reset();
            var x = new AsyncAwait();
            var res = await x.Go();
        }

        void TasksClick(object? sender, EventArgs e)
        {
            Reset();
            var x = new TaskWithoutAsync();
            x.Go();
        }

        void TracerClick(object? sender, EventArgs e)
        {
            Reset();
            var x = new TracerPlay();
            x.Go(12.34, new(50, 60, 70, 80));
            x.PlayWithAttribute();
        }

        void StuffClick(object? sender, EventArgs e)
        {
            Reset();
            var x = new Stuff();
        }
    }

    class Stuff
    {

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

    class TracerPlay
    {
        public int Go(double dval, Rectangle rect)
        {
            using var tr = new Tracer();

            // Check args.
            tr.Assert(dval == 6.7); // false - fail
            tr.Assert(rect.Height == 999); // false - fail

            var m1res = TestMethod1("here-we-go", new(10101));

            var m2res = TestMethod1("try-again", new(20202));

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
        public int TestMethod1(string s, Worker w)
        {
            using var tr = new Tracer();

            tr.Info($"entry s:{s} w:{w.Name}");

            // do something
            s = new string(s.Reverse().ToArray());

            tr.Info($"exit s:{s}");

            return s.Length;
        }

        public void PlayWithAttribute()
        {
            var info = typeof(TracerPlay).GetMember("TestMethod1");
            var attr = info[0].GetCustomAttribute<TracerMethodAttribute>();
            Tell(INF, $"{attr.Num}:{attr.Message}", 2);
        }
    }

    class DelegateLambda // TODO
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

    // General purpose target class for tests.
    class Worker(int id)
    {
        public string Name { get { return $"Worker{_id}"; } }

        int _id = id;

        public Task DoWorkAsync(string data)
        {
            Tell(INF, $"enter [{data}]");
            // Task.Run() runs sync code asynchronously.
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

    class AsyncSocket // TODO dev and migrate to nterm
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
