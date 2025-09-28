using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;



//https://blog.stephencleary.com/2012/02/async-and-await.html

//https://devblogs.microsoft.com/dotnet/asyncawait-faq/

// To achieve the benefits of asynchrony, can’t I just wrap my synchronous methods in calls to Task.Run?
// -- It depends on your goals for why you want to invoke the methods asynchronously. If your goal is simply to offload the 
// work you’re doing to another thread, so as to, for example, maintain the responsiveness of your UI thread, then sure. 
// If your goal is to help with scalability, then no, just wrapping a synchronous call in a Task.Run won’t help. 

// For more information, see Should I expose asynchronous wrappers for synchronous methods? And if from your UI thread you want 
// to offload work to a worker thread, and you use Task.Run to do so, you often typically want to do some work back on the 
// UI thread once that background work is done, and these language features make that kind of coordination easy and seamless.


//https://learn.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/task-based-asynchronous-pattern-tap


namespace NLab
{
    //https://stackoverflow.com/questions/14455293/how-and-when-to-use-async-and-await
    public class AAAA
    {

        // I don't understand why this method must be marked as `async`.
        private async void button1_Click(object sender, EventArgs e)
        {
            Task<int> access = DoSomethingAsync();
            // task independent stuff here

            // this line is reached after the 5 seconds sleep from 
            // DoSomethingAsync() method. Shouldn't it be reached immediately? 
            int a = 1;

            // from my understanding the waiting should be done here.
            int x = await access;
        }

        async Task<int> DoSomethingAsync()
        {
            // is this executed on a background thread?
            System.Threading.Thread.Sleep(5000);
            return 1;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////

        // Here's an example on which I hope I can explain some of the high-level details that are going on:

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
    }


    public class AsyncPlay
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

    class YetMore
    {
        async void Go()
        {
            // ... inside a method or class

            TcpClient client = new TcpClient();
            await client.ConnectAsync("server_ip_address", 1234);// port_number);
            NetworkStream stream = client.GetStream();

            //////////////

            byte[] buffer = new byte[1024]; // Or a suitable buffer size
            int bytesRead;

            // Run this in a separate task or thread
            await Task.Run(async () =>
            {
                try
                {
                    while (client.Connected) // Or a cancellation token for graceful shutdown
                    {
                        bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                        if (bytesRead == 0)
                        {
                            // Connection closed by remote host
                            break;
                        }

                        // Process the received data
                        string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        Console.WriteLine($"Received: {receivedData}");

                        // You might need to handle partial messages if your protocol sends them
                        // For example, if messages are length-prefixed or terminated by a specific character.
                    }
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"Network error: {ex.Message}");
                    // Handle connection loss or other network issues
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred during continuous read: {ex.Message}");
                }
                finally
                {
                    stream.Close();
                    client.Close();
                }
            });
        }
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
    public static void Main(string[] args)
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
