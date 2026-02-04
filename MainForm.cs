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
using static NLab.Utils;
 

// lots from - https://markheath.net/post/starting-threads-in-dotnet


namespace NLab
{
    public partial class MainForm : Form
    {



        public MainForm()
        {
            InitializeComponent();

            btnGo1.Click += OnButtonAsyncAwaitClick;
        }

        private async void OnButtonAsyncAwaitClick(object? sender, EventArgs e)
        {
            const string state = "Async Await";
            Cursor = Cursors.WaitCursor;

            try
            {
                Task<int> access = DoSomethingAsync();
                // task independent stuff here

                label1.Text = $"{state} Started";

                await AwaitableBackgroundTask(state);
                label1.Text = $"About to load XML";

                var xdoc = new XmlDocument();
                await Task.Run(() => xdoc.Load("http://feeds.feedburner.com/soundcode"));

                label1.Text = $"{state} Done {xdoc.FirstChild!.Name}";
            }
            catch (Exception ex)
            {
                label1.Text = ex.Message;
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        async Task<int> DoSomethingAsync()
        {
            // is this executed on a background thread?
            await Task.Delay(1000); // 1 second delay
            return 1;
        }


        //One of the easier methods is to use Task.Run, very similar to your existing code.
        //However, I do not recommend implementing a CalculateAsync method since that implies the
        //processing is asynchronous (which it is not). Instead, use Task.Run at the point of the call:
        async Task AwaitableBackgroundTask(string state)
        {
            Tell(INF, $"do some stuff");


            int i = 5;
            var task = Task.Run(() => { return Calculate(state); });

            // a synchronous function
            int Calculate(string s)
            {
                return s.Length + i;
            }

            Tell(INF, $"do other stuff");

            // run calculate as async - returns int answer
            var myOutput = await task;

            Tell(INF, $"some more stuff");


            Tell(INF, $"myOutput:{myOutput}");
        }


        public async Task MyMethodAsync()
        {
            Task<int> longRunningTask = LongRunningOperationAsync();

            // independent work which doesn't need the result of LongRunningOperationAsync can be done here

            //and now we call await on the task 
            int result = await longRunningTask;

            //use the result 
            Console.WriteLine(result);
        }



        public async Task<int> LongRunningOperationAsync() // assume we return an int from this long running operation 
        {
            await Task.Delay(1000); // 1 second delay
            return 1;
        }


        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////

        public async Task<int> MyFunc1Async(string s)
        {
            Tell(INF, $"MyFunc1Async() enter");

            await Task.Delay(100);

            Tell(INF, $"MyFunc1Async() exit");

            return s.Length;
        }

        // To achieve the benefits of asynchrony, can’t I just wrap my synchronous methods in calls to Task.Run?
        // -- It depends on your goals for why you want to invoke the methods asynchronously. If your goal is simply to offload the 
        // work you’re doing to another thread, so as to, for example, maintain the responsiveness of your UI thread, then sure. 
        // If your goal is to help with scalability, then no, just wrapping a synchronous call in a Task.Run won’t help. 
        //And if from your UI thread you want to offload work to a worker thread, and you use Task.Run to do so,
        //you often typically want to do some work back on the UI thread once that background work is done,
        //and these language features make that kind of coordination easy and seamless.
        async Task RunOnThreadAsync()
        {
            Tell(INF, $"RunOnThreadAsync() enter");
            await Task.Delay(1000);
            Tell(INF, $"RunOnThreadAsync() exit");
        }

        // We can await Tasks, regardless of where they come from.
        public async Task ComposeAsync()
        {
            Tell(INF, $"ComposeAsync() enter");
            await RunOnThreadAsync();
            Tell(INF, $"ComposeAsync() mid");
            await MyFunc1Async("gogogo");
            Tell(INF, $"ComposeAsync() exit");
        }


        #region ================== async sockets =====================
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

        public void Callback()
        {
            Console.WriteLine("Everything done");
        }


        public void Run()
        {
            var workers = new List<IWorker> { new Worker(), new Worker(), new Worker() };
            var tasks = workers.Select(t => t.DoWorkAsync("some data"));
            Task.WhenAll(tasks).ContinueWith(task => Callback());
            Console.WriteLine("Waiting");
        }
        #endregion




        #region ================== Tasks no async/await =====================
        // You're looking for Task.WhenAll. Create a bunch of Tasks that do what you want them to, then wait on all of the tasks and ContinueWith your callback. I split out an async version of the DoWork method - if you're always going to be calling it asynchronously you don't necessarily need to do that.
        public interface IWorker
        {
            Task DoWorkAsync(string data);
            void DoWork(string data);
        }

        public class Worker : IWorker
        {
            public Task DoWorkAsync(string data)
            {
                return Task.Run(() => DoWork(data));
            }

            public void DoWork(string data)
            {
                Console.WriteLine(data);
                Thread.Sleep(100);
            }
        }

        public class Runner
        {
            public void Callback()
            {
                Console.WriteLine("Everything done");
            }

            public void Run()
            {
                var workers = new List<IWorker> { new Worker(), new Worker(), new Worker() };
                var tasks = workers.Select(t => t.DoWorkAsync("some data"));
                Task.WhenAll(tasks).ContinueWith(task => Callback());
                Console.WriteLine("Waiting");
            }
        }
        #endregion




        #region ============== Internal ==================
        #endregion

    }
}


// Various socket async

/*
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
